using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RadustovTestTask.API.Interfaces;
using RadustovTestTask.API.Mappers;
using RadustovTestTask.BLL.Constants;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.BLL.Mappers;
using RadustovTestTask.BLL.Services;
using RadustovTestTask.DAL;
using RadustovTestTask.DAL.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<Employee, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITaskMapper, TaskMapper>();
builder.Services.AddScoped<IProjectMapper, ProjectMapper>();
builder.Services.AddScoped<IEmployeeMapper, EmployeeMapper>();
builder.Services.AddScoped<IAuthMapper, AuthMapper>();
builder.Services.AddScoped<ITaskApiMapper, TaskApiMapper>();
builder.Services.AddScoped<IProjectApiMapper, ProjectApiMapper>();
builder.Services.AddScoped<IEmployeeApiMapper, EmployeeApiMapper>();
builder.Services.AddScoped<IDocumentApiMapper, DocumentApiMapper>();
builder.Services.AddScoped<IAuthApiMapper, AuthApiMapper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentMapper, DocumentMapper>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ChiefOnly", policy => policy.RequireRole(AppRoles.Chief));
    options.AddPolicy("ChiefOrManager", policy => policy.RequireRole(AppRoles.Chief, AppRoles.Manager));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<Employee>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();

    if (!await roleManager.RoleExistsAsync(AppRoles.Chief))
        await roleManager.CreateAsync(new IdentityRole<long>(AppRoles.Chief));
    if (!await roleManager.RoleExistsAsync(AppRoles.Manager))
        await roleManager.CreateAsync(new IdentityRole<long>(AppRoles.Manager));
    if (!await roleManager.RoleExistsAsync(AppRoles.Employee))
        await roleManager.CreateAsync(new IdentityRole<long>(AppRoles.Employee));

    var adminSettings = builder.Configuration.GetSection("AdminSettings");
    var chiefEmail = adminSettings["Email"] ?? "chief@testmail.com";
    var chiefPassword = adminSettings["Password"] ?? "Admin123!";
    var chiefUser = await userManager.FindByEmailAsync(chiefEmail);

    if (chiefUser == null)
    {
        chiefUser = new Employee
        {
            UserName = chiefEmail,
            Email = chiefEmail,
            FirstName = adminSettings["FirstName"] ?? "Admin",
            LastName = adminSettings["LastName"] ?? "Chief",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(chiefUser, chiefPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(chiefUser, AppRoles.Chief);
        }
    }
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();