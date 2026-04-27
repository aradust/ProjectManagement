namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateEmployeeWithPasswordAsync(string firstName, string lastName, string? middleName, string email, string password, string role);
        Task<List<EmployeeDto>> GetAllEmployeesAsync();
        Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm);
        Task<EmployeeDto?> GetEmployeeByIdAsync(long id);
        Task<EmployeeDto?> UpdateEmployeeAsync(EmployeeDto dto);
        Task<bool> RemoveEmployeeAsync(long id);
        Task<bool> EmailExistsAsync(string email, long? excludeEmployeeId);
    }
}