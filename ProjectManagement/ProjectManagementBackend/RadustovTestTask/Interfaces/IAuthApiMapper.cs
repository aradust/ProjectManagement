namespace RadustovTestTask.API.Interfaces
{
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public interface IAuthApiMapper
    {
        RegisterDto ToRegisterDto(RegisterRequest request);
        LoginDto ToLoginDto(LoginRequest request);
    }
}