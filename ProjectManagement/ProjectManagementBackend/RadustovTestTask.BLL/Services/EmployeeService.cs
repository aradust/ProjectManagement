namespace RadustovTestTask.BLL.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using RadustovTestTask.BLL.Constants;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL;
    using RadustovTestTask.DAL.Entities;

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Employee> _userManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly IEmployeeMapper _employeeMapper;

        public EmployeeService(
            ApplicationDbContext dbContext,
            UserManager<Employee> userManager,
            RoleManager<IdentityRole<long>> roleManager,
            IEmployeeMapper employeeMapper)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeMapper = employeeMapper;
        }

        public async Task<EmployeeDto> CreateEmployeeWithPasswordAsync(
            string firstName,
            string lastName,
            string? middleName,
            string email,
            string password,
            string role)
        {
            Employee? existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                throw new InvalidOperationException("Employee with this email already exists");
            }

            Employee employee = new Employee
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                EmailConfirmed = true
            };

            IdentityResult createResult = await _userManager.CreateAsync(employee, password);
            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(role));
            }

            await _userManager.AddToRoleAsync(employee, role);

            return _employeeMapper.ToDtoFromCreate(employee, role);
        }

        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            List<Employee> employees = await _dbContext.Employees.ToListAsync();
            List<EmployeeDto> result = new List<EmployeeDto>();

            foreach (Employee emp in employees)
            {
                IList<string> roles = await _userManager.GetRolesAsync(emp);
                result.Add(_employeeMapper.ToDto(emp, roles.FirstOrDefault() ?? AppRoles.Employee));
            }

            return result;
        }

        public async Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm)
        {
            string term = searchTerm.ToLower();
            List<Employee> employees = await _dbContext.Employees
                .Where(e => EF.Functions.Like(e.FirstName.ToLower(), $"%{term}%") ||
                            EF.Functions.Like(e.LastName.ToLower(), $"%{term}%") ||
                            EF.Functions.Like(e.Email.ToLower(), $"%{term}%"))
                .ToListAsync();

            List<EmployeeDto> result = new List<EmployeeDto>();

            foreach (Employee emp in employees)
            {
                IList<string> roles = await _userManager.GetRolesAsync(emp);
                result.Add(_employeeMapper.ToDto(emp, roles.FirstOrDefault() ?? AppRoles.Employee));
            }

            return result;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(long id)
        {
            Employee? employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return null;
            }

            IList<string> roles = await _userManager.GetRolesAsync(employee);
            return _employeeMapper.ToDto(employee, roles.FirstOrDefault() ?? AppRoles.Employee);
        }

        public async Task<EmployeeDto?> UpdateEmployeeAsync(EmployeeDto dto)
        {
            Employee? employee = await _dbContext.Employees.FindAsync(dto.Id);
            if (employee == null)
            {
                return null;
            }

            _employeeMapper.UpdateEntity(employee, dto);

            if (!string.IsNullOrEmpty(dto.Role))
            {
                IList<string> currentRoles = await _userManager.GetRolesAsync(employee);
                await _userManager.RemoveFromRolesAsync(employee, currentRoles);

                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<long>(dto.Role));
                }

                await _userManager.AddToRoleAsync(employee, dto.Role);
            }

            await _dbContext.SaveChangesAsync();

            IList<string> updatedRoles = await _userManager.GetRolesAsync(employee);
            return _employeeMapper.ToDto(employee, updatedRoles.FirstOrDefault() ?? AppRoles.Employee);
        }

        public async Task<bool> RemoveEmployeeAsync(long id)
        {
            Employee? employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return false;
            }

            bool isManager = await _dbContext.Projects.AnyAsync(p => p.ProjectManagerId == id);
            if (isManager)
            {
                throw new InvalidOperationException("Cannot delete employee: they are manager of one or more projects. Reassign projects first.");
            }

            bool isMember = await _dbContext.ProjectEmployees.AnyAsync(pe => pe.EmployeeId == id);
            if (isMember)
            {
                throw new InvalidOperationException("Cannot delete employee: they are assigned to one or more projects. Remove them from projects first.");
            }

            bool isAuthor = await _dbContext.TaskItem.AnyAsync(t => t.AuthorId == id);
            if (isAuthor)
            {
                throw new InvalidOperationException("Cannot delete employee: they are author of tasks. Reassign tasks first.");
            }

            bool isExecutor = await _dbContext.TaskItem.AnyAsync(t => t.ExecutorId == id);
            if (isExecutor)
            {
                throw new InvalidOperationException("Cannot delete employee: they are assigned to tasks. Reassign tasks first.");
            }

            IdentityResult result = await _userManager.DeleteAsync(employee);
            return result.Succeeded;
        }

        public async Task<bool> EmailExistsAsync(string email, long? excludeEmployeeId)
        {
            IQueryable<Employee> query = _dbContext.Employees.AsQueryable();

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEmployeeId.Value);
            }

            return await query.AnyAsync(e => e.Email == email);
        }
    }
}