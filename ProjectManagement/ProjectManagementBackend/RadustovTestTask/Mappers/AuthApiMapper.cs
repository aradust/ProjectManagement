namespace RadustovTestTask.API.Mappers
{
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public class AuthApiMapper : IAuthApiMapper
    {
        public RegisterDto ToRegisterDto(RegisterRequest request)
        {
            return new RegisterDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                Password = request.Password
            };
        }

        public LoginDto ToLoginDto(LoginRequest request)
        {
            return new LoginDto
            {
                Email = request.Email,
                Password = request.Password
            };
        }
    }
}