namespace RadustovTestTask.BLL.Interfaces
{
    public interface ICurrentUserService
    {
        long? GetCurrentUserId();
        bool IsInRole(string role);
    }
}