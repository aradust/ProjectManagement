namespace RadustovTestTask.BLL.Mappers
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;

    public class AuthMapper : IAuthMapper
    {
        public Employee ToEntity(RegisterDto registerDto)
        {
            return new Employee
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                MiddleName = registerDto.MiddleName,
                EmailConfirmed = true
            };
        }

        public AuthResponseDto ToAuthResponseDto(Employee user, string token, IList<string> roles)
        {
            return new AuthResponseDto
            {
                Token = token,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }

        public UserInfoDto ToUserInfoDto(Employee user, IList<string> roles)
        {
            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };
        }
    }
}