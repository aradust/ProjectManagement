namespace RadustovTestTask.BLL.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public class AuthService : IAuthService
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthMapper _authMapper;

        public AuthService(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            IConfiguration configuration,
            IAuthMapper authMapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _authMapper = authMapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            Employee? existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            Employee user = _authMapper.ToEntity(registerDto);

            IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            Employee? user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<UserInfoDto> GetCurrentUserAsync(long userId)
        {
            Employee? user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception("User not found");
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            return _authMapper.ToUserInfoDto(user, roles);
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(Employee user)
        {
            IConfigurationSection jwtSettings = _configuration.GetSection("JwtSettings");
            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            List<Claim> authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                expires: DateTime.Now.AddHours(double.Parse(jwtSettings["ExpirationInHours"]!)),
                claims: authClaims,
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return _authMapper.ToAuthResponseDto(user, tokenString, userRoles);
        }
    }
}