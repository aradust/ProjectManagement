using Microsoft.AspNetCore.Identity;
using Moq;
using RadustovTestTask.BLL.Constants;
using RadustovTestTask.BLL.DTO;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.BLL.Mappers;
using RadustovTestTask.BLL.Services;
using RadustovTestTask.DAL.Entities;
using Xunit;

namespace RadustovTestTask.Tests.tests
{
    public class EmployeeServiceTests : TestBase
    {
        private readonly IEmployeeMapper _employeeMapper = new EmployeeMapper();

        private EmployeeService CreateEmployeeService()
        {
            return new EmployeeService(
                DbContext,
                UserManagerMock.Object,
                RoleManagerMock.Object,
                _employeeMapper
            );
        }

        [Fact]
        public async Task CreateEmployeeWithPasswordAsync_ValidData_CreatesEmployeeAndAssignsRole()
        {
            var service = CreateEmployeeService();
            var firstName = "John";
            var lastName = "Doe";
            var email = "john@test.com";
            var password = "Pass123";
            var role = AppRoles.Manager;

            UserManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((Employee?)null);
            UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>(), password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<Employee, string>((emp, pwd) => emp.Id = 1);
            RoleManagerMock.Setup(x => x.RoleExistsAsync(role)).ReturnsAsync(true);
            UserManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<Employee>(), role))
                .ReturnsAsync(IdentityResult.Success);
            UserManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<Employee>()))
                .ReturnsAsync(new List<string> { role });

            var result = await service.CreateEmployeeWithPasswordAsync(
                firstName, lastName, null, email, password, role);
            
            Assert.Equal(email, result.Email);
            Assert.Equal(role, result.Role);
        }

        [Fact]
        public async Task CreateEmployeeWithPasswordAsync_ExistingEmail_ThrowsInvalidOperationException()
        {
            var service = CreateEmployeeService();
            var email = "exists@test.com";
            var existing = new Employee { Email = email };
            UserManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(existing);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateEmployeeWithPasswordAsync("f", "l", null, email, "pwd", AppRoles.Employee));
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllEmployeesWithRoles()
        {
            var service = CreateEmployeeService();
            var emp1 = await CreateEmployeeAsync("emp1@test.com", "First1", "Last1", AppRoles.Employee);
            var emp2 = await CreateEmployeeAsync("emp2@test.com", "First2", "Last2", AppRoles.Manager);

            var result = await service.GetAllEmployeesAsync();
            
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.Email == "emp1@test.com");
            Assert.Contains(result, e => e.Email == "emp2@test.com");
        }

        [Fact]
        public async Task SearchEmployeesAsync_WithSearchTerm_ReturnsFilteredEmployees()
        {
            var service = CreateEmployeeService();
            await CreateEmployeeAsync("john@test.com", "John", "Doe", AppRoles.Employee);
            await CreateEmployeeAsync("jane@test.com", "Jane", "Smith", AppRoles.Employee);

            var result = await service.SearchEmployeesAsync("john");
            
            Assert.Single(result);
            Assert.Equal("john@test.com", result[0].Email);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ExistingEmployee_ReturnsEmployee()
        {
            var service = CreateEmployeeService();
            var employee = await CreateEmployeeAsync("test@test.com", "Test", "User", AppRoles.Employee);

            var result = await service.GetEmployeeByIdAsync(employee.Id);

            Assert.NotNull(result);
            Assert.Equal(employee.Email, result.Email);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_NonExistingEmployee_ReturnsNull()
        {
            var service = CreateEmployeeService();

            var result = await service.GetEmployeeByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ExistingEmployee_UpdatesAndReturnsEmployee()
        {
            var service = CreateEmployeeService();
            var employee = await CreateEmployeeAsync("old@test.com", "Old", "Name", AppRoles.Employee);

            var updateDto = new EmployeeDto
            {
                Id = employee.Id,
                FirstName = "New",
                LastName = "Name",
                Email = "new@test.com",
                Role = AppRoles.Manager
            };

            RoleManagerMock.Setup(x => x.RoleExistsAsync(AppRoles.Manager)).ReturnsAsync(true);
            UserManagerMock.Setup(x => x.GetRolesAsync(employee))
                .ReturnsAsync(new List<string> { AppRoles.Employee });
            UserManagerMock.Setup(x => x.RemoveFromRolesAsync(employee, It.IsAny<IList<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            UserManagerMock.Setup(x => x.AddToRoleAsync(employee, AppRoles.Manager))
                .ReturnsAsync(IdentityResult.Success);

            var result = await service.UpdateEmployeeAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("New", result.FirstName);
            Assert.Equal("new@test.com", result.Email);
        }

        [Fact]
        public async Task RemoveEmployeeAsync_WithActiveManagerAssignment_ThrowsInvalidOperationException()
        {
            var service = CreateEmployeeService();
            var employee = await CreateEmployeeAsync("manager@test.com", "Manager", "Test", AppRoles.Manager);
            await CreateProjectAsync("Managed Project", managerId: employee.Id);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RemoveEmployeeAsync(employee.Id));
            Assert.Contains("manager", ex.Message);
        }

        [Fact]
        public async Task RemoveEmployeeAsync_WithActiveProjectAssignment_ThrowsInvalidOperationException()
        {
            var service = CreateEmployeeService();
            var employee = await CreateEmployeeAsync("member@test.com", "Member", "Test", AppRoles.Employee);
            await CreateProjectAsync("Member Project", managerId: 999, employeeIds: new List<long> { employee.Id });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RemoveEmployeeAsync(employee.Id));
            Assert.Contains("assigned to one or more projects", ex.Message);
        }

        [Fact]
        public async Task RemoveEmployeeAsync_NoDependencies_DeletesSuccessfully()
        {
            var service = CreateEmployeeService();
            var employee = await CreateEmployeeAsync("free@test.com", "Free", "Emp", AppRoles.Employee);
            UserManagerMock.Setup(x => x.DeleteAsync(employee)).ReturnsAsync(IdentityResult.Success);

            var result = await service.RemoveEmployeeAsync(employee.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
        {
            var service = CreateEmployeeService();
            await CreateEmployeeAsync("exists@test.com", "Test", "User", AppRoles.Employee);

            var result = await service.EmailExistsAsync("exists@test.com", null);

            Assert.True(result);
        }

        [Fact]
        public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
        {
            var service = CreateEmployeeService();

            var result = await service.EmailExistsAsync("nonexistent@test.com", null);

            Assert.False(result);
        }
    }
}