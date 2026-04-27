namespace RadustovTestTask.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuthApiMapper _authApiMapper;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(
            IAuthService authService,
            IAuthApiMapper authApiMapper,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _authApiMapper = authApiMapper;
            _currentUserService = currentUserService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                RegisterDto registerDto = _authApiMapper.ToRegisterDto(request);
                AuthResponseDto result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                LoginDto loginDto = _authApiMapper.ToLoginDto(request);
                AuthResponseDto result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            long? userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                UserInfoDto user = await _authService.GetCurrentUserAsync(userId.Value);
                return Ok(user);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}