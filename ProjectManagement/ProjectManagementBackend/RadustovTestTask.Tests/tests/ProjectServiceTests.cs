using RadustovTestTask.BLL.Constants;
using RadustovTestTask.BLL.DTO;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.BLL.Mappers;
using RadustovTestTask.BLL.Services;
using Xunit;

namespace RadustovTestTask.Tests.tests
{
    public class ProjectServiceTests : TestBase
    {
        private readonly IProjectMapper _projectMapper = new ProjectMapper();
        private readonly IDocumentMapper _documentMapper = new DocumentMapper();

        private ProjectService CreateProjectService()
        {
            return new ProjectService(
                DbContext,
                CurrentUserServiceMock.Object,
                _projectMapper,
                _documentMapper
            );
        }

        [Fact]
        public async Task CreateProjectAsync_AsChief_Succeeds()
        {
            var chiefId = 1L;
            ResetCurrentUser();
            SetupCurrentUser(chiefId, AppRoles.Chief);
            var service = CreateProjectService();

            var dto = new ProjectDto
            {
                ProjectName = "Chief Project",
                CustomerCompanyName = "Customer",
                ExecutorCompanyName = "Executor",
                ProjectManagerId = 10,
                ProjectStart = DateTime.UtcNow,
                ProjectEnd = DateTime.UtcNow.AddDays(10),
                Priority = 5
            };
            var employeeIds = new List<long> { 20, 30 };

            var result = await service.CreateProjectAsync(dto, employeeIds);

            Assert.NotEqual(0, result.Id);
            var projectInDb = await DbContext.Projects.FindAsync(result.Id);
            Assert.NotNull(projectInDb);
            Assert.Equal(2, projectInDb.ProjectEmployees.Count);
        }

        [Fact]
        public async Task CreateProjectAsync_AsManager_Succeeds()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateProjectService();

            var dto = new ProjectDto
            {
                ProjectName = "Manager Project",
                CustomerCompanyName = "Customer",
                ExecutorCompanyName = "Executor",
                ProjectManagerId = managerId,
                ProjectStart = DateTime.UtcNow,
                ProjectEnd = DateTime.UtcNow.AddDays(10),
                Priority = 5
            };
            var employeeIds = new List<long> { 20 };

            var result = await service.CreateProjectAsync(dto, employeeIds);

            Assert.NotEqual(0, result.Id);
        }

        [Fact]
        public async Task CreateProjectAsync_AsEmployee_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateProjectService();
            var dto = new ProjectDto();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.CreateProjectAsync(dto, new List<long>()));
        }

        [Fact]
        public async Task GetProjectByIdAsync_AsManager_ForOtherProject_ReturnsNull()
        {
            ResetCurrentUser();
            SetupCurrentUser(10L, AppRoles.Manager);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("Other Project", managerId: 99);

            var result = await service.GetProjectByIdAsync(project.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_AsEmployee_WithoutAccess_ReturnsNull()
        {
            ResetCurrentUser();
            SetupCurrentUser(50L, AppRoles.Employee);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("No Access", managerId: 10);

            var result = await service.GetProjectByIdAsync(project.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_AsManager_ForOwnProject_Succeeds()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("Old Name", managerId);

            var updateDto = new ProjectDto
            {
                Id = project.Id,
                ProjectName = "New Name",
                CustomerCompanyName = "New Customer",
                ExecutorCompanyName = "New Executor",
                ProjectManagerId = managerId,
                ProjectStart = DateTime.UtcNow,
                ProjectEnd = DateTime.UtcNow.AddDays(20),
                Priority = 8
            };

            var result = await service.UpdateProjectAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("New Name", result.ProjectName);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdateTeamOnly_UpdatesOnlyEmployees()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();
            var project = await CreateProjectAsync(
                "Team Project",
                managerId: 10,
                employeeIds: new List<long> { 100 });

            var updateDto = new ProjectDto
            {
                Id = project.Id,
                ProjectName = "Changed Name", 
                EmployeeIds = new List<long> { 200, 300 }
            };

            var result = await service.UpdateProjectAsync(updateDto, updateTeamOnly: true);

            Assert.NotNull(result);

            var updated = await DbContext.Projects.FindAsync(project.Id);
            Assert.Equal(2, updated.ProjectEmployees.Count);
        }

        [Fact]
        public async Task RemoveProjectAsync_AsChief_Succeeds()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("To Delete", managerId: 10);

            var result = await service.RemoveProjectAsync(project.Id);

            Assert.True(result);
            var deleted = await DbContext.Projects.FindAsync(project.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task RemoveProjectAsync_AsManager_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(10L, AppRoles.Manager);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("Cannot Delete", managerId: 10);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.RemoveProjectAsync(project.Id));
        }

        [Fact]
        public async Task AddEmployeeToProjectAsync_ValidData_Succeeds()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateProjectService();
            var project = await CreateProjectAsync("Add Employee", managerId);
            var employee = await CreateEmployeeAsync("emp@test.com", "Test", "Emp", AppRoles.Employee);

            var result = await service.AddEmployeeToProjectAsync(project.Id, employee.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task AddEmployeeToProjectAsync_AlreadyAssigned_ReturnsFalse()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateProjectService();
            var employee = await CreateEmployeeAsync("emp@test.com", "Test", "Emp", AppRoles.Employee);
            var project = await CreateProjectAsync("Project", managerId, new List<long> { employee.Id });

            var result = await service.AddEmployeeToProjectAsync(project.Id, employee.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task GetProjectEmployeesAsync_ReturnsAssignedEmployees()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();
            var emp1 = await CreateEmployeeAsync("emp1@test.com", "First1", "Last1", AppRoles.Employee);
            var emp2 = await CreateEmployeeAsync("emp2@test.com", "First2", "Last2", AppRoles.Employee);
            var project = await CreateProjectAsync("Team Project", 10, new List<long> { emp1.Id, emp2.Id });

            var result = await service.GetProjectEmployeesAsync(project.Id);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SearchProjectsAsync_WithFilters_ReturnsFilteredProjects()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();

            await CreateProjectAsync("Alpha Project", 10);
            await CreateProjectAsync("Beta Project", 10);
            await CreateProjectAsync("Gamma", 10);

            var result = await service.SearchProjectsAsync(query: "Alpha");

            Assert.Single(result);
            Assert.Equal("Alpha Project", result[0].ProjectName);
        }

        [Fact]
        public async Task ProjectNameExistsAsync_ExistingName_ReturnsTrue()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();
            await CreateProjectAsync("Unique Name", 10);

            var result = await service.ProjectNameExistsAsync("Unique Name", null);

            Assert.True(result);
        }

        [Fact]
        public async Task ProjectNameExistsAsync_NonExistingName_ReturnsFalse()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateProjectService();

            var result = await service.ProjectNameExistsAsync("Nonexistent", null);

            Assert.False(result);
        }
    }
}