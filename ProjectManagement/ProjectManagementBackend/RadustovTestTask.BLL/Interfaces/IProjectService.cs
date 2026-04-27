namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface IProjectService
    {
        Task<ProjectDto> CreateProjectAsync(ProjectDto dto, List<long> employeeIds);
        Task<ProjectDto?> GetProjectByIdAsync(long id);
        Task<ProjectDto?> UpdateProjectAsync(ProjectDto dto, bool updateTeamOnly = false);
        Task<bool> RemoveProjectAsync(long id);
        Task<bool> AddEmployeeToProjectAsync(long projectId, long employeeId);
        Task<bool> RemoveEmployeeFromProjectAsync(long projectId, long employeeId);
        Task<List<EmployeeDto>> GetProjectEmployeesAsync(long projectId);
        Task<List<ProjectDto>> SearchProjectsAsync(
            string? query = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            int? priorityFrom = null,
            int? priorityTo = null,
            string? sortBy = null);
        Task<bool> ProjectNameExistsAsync(string name, long? excludeProjectId);
    }
}