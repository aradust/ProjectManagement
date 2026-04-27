using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.DAL;
using RadustovTestTask.DAL.Entities;

namespace RadustovTestTask.Tests.tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly Mock<UserManager<Employee>> UserManagerMock;
        protected readonly Mock<RoleManager<IdentityRole<long>>> RoleManagerMock;
        protected readonly Mock<SignInManager<Employee>> SignInManagerMock;
        protected readonly Mock<IConfiguration> ConfigurationMock;
        protected readonly Mock<ICurrentUserService> CurrentUserServiceMock;
        protected readonly InMemoryFileStorage FileStorage;

        protected TestBase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            DbContext = new ApplicationDbContext(options);

            UserManagerMock = MockUserManager<Employee>();
            RoleManagerMock = MockRoleManager<IdentityRole<long>>();
            SignInManagerMock = MockSignInManager<Employee>();
            ConfigurationMock = new Mock<IConfiguration>();
            CurrentUserServiceMock = new Mock<ICurrentUserService>();

            FileStorage = new InMemoryFileStorage();

            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["SecretKey"]).Returns("this_is_a_very_long_secret_key_for_testing_1234567890");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");
            jwtSectionMock.Setup(x => x["ExpirationInHours"]).Returns("1");
            ConfigurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
        {
            var store = new Mock<IRoleStore<TRole>>();
            return new Mock<RoleManager<TRole>>(
                store.Object, null, null, null, null);
        }

        private static Mock<SignInManager<TUser>> MockSignInManager<TUser>() where TUser : class
        {
            var userManager = MockUserManager<TUser>();
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            return new Mock<SignInManager<TUser>>(
                userManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        protected void ResetCurrentUser()
        {
            CurrentUserServiceMock.Reset();
        }

        protected void SetupCurrentUser(long userId, string role)
        {
            CurrentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            CurrentUserServiceMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns<string>(r => r == role);
        }

        protected void SetupCurrentUserWithRoles(long userId, params string[] roles)
        {
            CurrentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            CurrentUserServiceMock.Setup(x => x.IsInRole(It.IsAny<string>()))
                .Returns<string>(r => roles.Contains(r));
        }

        protected void SetupNoCurrentUser()
        {
            CurrentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((long?)null);
            CurrentUserServiceMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
        }

        protected async Task<Employee> CreateEmployeeAsync(string email, string firstName, string lastName, string role)
        {
            var employee = new Employee
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };
            await DbContext.Employees.AddAsync(employee);
            await DbContext.SaveChangesAsync();

            UserManagerMock.Setup(x => x.GetRolesAsync(employee)).ReturnsAsync(new List<string> { role });

            return employee;
        }

        protected async Task<Project> CreateProjectAsync(string name, long managerId, List<long>? employeeIds = null)
        {
            var project = new Project
            {
                ProjectName = name,
                CustomerCompanyName = "Test Customer",
                ExecutorCompanyName = "Test Executor",
                ProjectManagerId = managerId,
                ProjectStart = DateTime.UtcNow,
                ProjectEnd = DateTime.UtcNow.AddDays(30),
                Priority = 5,
                ProjectEmployees = new List<ProjectEmployee>()
            };

            if (employeeIds != null)
            {
                foreach (var empId in employeeIds)
                {
                    project.ProjectEmployees.Add(new ProjectEmployee { EmployeeId = empId });
                }
            }

            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();
            return project;
        }

        protected async Task<TaskItem> CreateTaskAsync(string title, long projectId, long authorId, long? executorId = null)
        {
            var task = new TaskItem
            {
                Title = title,
                ProjectId = projectId,
                AuthorId = authorId,
                ExecutorId = executorId,
                Status = DAL.Entities.TaskStatus.ToDo,
                Priority = 5
            };
            await DbContext.TaskItem.AddAsync(task);
            await DbContext.SaveChangesAsync();
            return task;
        }
    }

    public class InMemoryFileStorage : IFileStorageService
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public Task<string> SaveFileAsync(string fileName, Stream fileStream)
        {
            using var ms = new MemoryStream();
            fileStream.CopyTo(ms);
            _storage[fileName] = ms.ToArray();
            return Task.FromResult(fileName);
        }

        public Task<byte[]> ReadFileAsync(string fileName)
        {
            if (_storage.TryGetValue(fileName, out var data))
                return Task.FromResult(data);
            throw new FileNotFoundException($"File '{fileName}' not found");
        }

        public Task DeleteFileAsync(string fileName)
        {
            _storage.Remove(fileName);
            return Task.CompletedTask;
        }

        public bool FileExists(string fileName) => _storage.ContainsKey(fileName);
    }
}