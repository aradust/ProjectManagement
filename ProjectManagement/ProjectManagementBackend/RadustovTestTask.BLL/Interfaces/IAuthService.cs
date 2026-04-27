namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserInfoDto> GetCurrentUserAsync(long userId);
    }
}