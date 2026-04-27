namespace RadustovTestTask.BLL.Services
{
    using Microsoft.AspNetCore.Http;
    using RadustovTestTask.BLL.Interfaces;
    using System.Security.Claims;

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? GetCurrentUserId()
        {
            ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            string? claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(claim, out long userId))
            {
                return userId;
            }

            return null;
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
        }
    }
}