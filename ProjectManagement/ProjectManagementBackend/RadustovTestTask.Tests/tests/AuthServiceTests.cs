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
    public class AuthServiceTests : TestBase
    {
        private readonly IAuthMapper _authMapper = new AuthMapper();

        private AuthService CreateAuthService()
        {
            return new AuthService(
                UserManagerMock.Object,
                SignInManagerMock.Object,
                ConfigurationMock.Object,
                _authMapper
            );
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
        {
            var service = CreateAuthService();
            var registerDto = new RegisterDto
            {
                Email = "new@test.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123"
            };

            UserManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((Employee?)null);
            UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            UserManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<Employee>(), AppRoles.Employee))
                .ReturnsAsync(IdentityResult.Success);
            UserManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<Employee>()))
                .ReturnsAsync(new List<string> { AppRoles.Employee });

            var result = await service.RegisterAsync(registerDto);

            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(registerDto.Email, result.User.Email);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            var service = CreateAuthService();
            var registerDto = new RegisterDto { Email = "existing@test.com", Password = "Pass" };
            var existingEmployee = new Employee { Email = registerDto.Email };

            UserManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingEmployee);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(registerDto));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            var service = CreateAuthService();
            var loginDto = new LoginDto { Email = "user@test.com", Password = "Pass" };
            var user = new Employee { Id = 1, Email = loginDto.Email, FirstName = "Test", LastName = "User" };

            UserManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);
            UserManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { AppRoles.Employee });

            var result = await service.LoginAsync(loginDto);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.User.Id);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            var service = CreateAuthService();
            var loginDto = new LoginDto { Email = "user@test.com", Password = "wrong" };
            var user = new Employee { Email = loginDto.Email };

            UserManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithValidUserId_ReturnsUserInfo()
        {
            var service = CreateAuthService();
            var user = new Employee
            {
                Id = 1,
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            };

            UserManagerMock.Setup(x => x.FindByIdAsync("1"))
                .ReturnsAsync(user);
            UserManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { AppRoles.Employee });

            var result = await service.GetCurrentUserAsync(1);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }
    }
}