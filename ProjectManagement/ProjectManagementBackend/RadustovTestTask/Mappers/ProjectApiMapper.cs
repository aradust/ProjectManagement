namespace RadustovTestTask.API.Mappers
{
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public class ProjectApiMapper : IProjectApiMapper
    {
        public ProjectDto ToCreateDto(CreateProjectRequest request)
        {
            return new ProjectDto
            {
                ProjectName = request.ProjectName,
                CustomerCompanyName = request.CustomerCompanyName,
                ExecutorCompanyName = request.ExecutorCompanyName,
                ProjectManagerId = request.ProjectManagerId,
                ProjectStart = request.ProjectStart,
                ProjectEnd = request.ProjectEnd,
                Priority = request.Priority,
                EmployeeIds = request.EmployeeIds ?? new List<long>()
            };
        }

        public ProjectDto ToUpdateDto(UpdateProjectRequest request)
        {
            return new ProjectDto
            {
                Id = request.Id,
                ProjectName = request.ProjectName,
                CustomerCompanyName = request.CustomerCompanyName,
                ExecutorCompanyName = request.ExecutorCompanyName,
                ProjectManagerId = request.ProjectManagerId,
                ProjectStart = request.ProjectStart,
                ProjectEnd = request.ProjectEnd,
                Priority = request.Priority,
                EmployeeIds = request.EmployeeIds ?? new List<long>()
            };
        }
    }
}