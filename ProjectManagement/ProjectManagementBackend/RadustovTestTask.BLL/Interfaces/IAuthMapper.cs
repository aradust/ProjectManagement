namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.DAL.Entities;

    public interface IAuthMapper
    {
        Employee ToEntity(RegisterDto registerDto);
        AuthResponseDto ToAuthResponseDto(Employee user, string token, IList<string> roles);
        UserInfoDto ToUserInfoDto(Employee user, IList<string> roles);
    }
}