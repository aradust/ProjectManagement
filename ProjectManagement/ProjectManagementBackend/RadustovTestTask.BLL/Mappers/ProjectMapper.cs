namespace RadustovTestTask.BLL.Mappers
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;

    public class ProjectMapper : IProjectMapper
    {
        public Project ToEntity(ProjectDto dto)
        {
            return new Project
            {
                ProjectName = dto.ProjectName,
                CustomerCompanyName = dto.CustomerCompanyName,
                ExecutorCompanyName = dto.ExecutorCompanyName,
                ProjectManagerId = dto.ProjectManagerId,
                ProjectStart = dto.ProjectStart,
                ProjectEnd = dto.ProjectEnd,
                Priority = dto.Priority
            };
        }

        public void UpdateEntity(Project entity, ProjectDto dto)
        {
            entity.ProjectName = dto.ProjectName;
            entity.CustomerCompanyName = dto.CustomerCompanyName;
            entity.ExecutorCompanyName = dto.ExecutorCompanyName;
            entity.ProjectManagerId = dto.ProjectManagerId;
            entity.ProjectStart = dto.ProjectStart;
            entity.ProjectEnd = dto.ProjectEnd;
            entity.Priority = dto.Priority;
        }

        public ProjectDto ToDto(Project entity)
        {
            return new ProjectDto
            {
                Id = entity.Id,
                ProjectName = entity.ProjectName,
                CustomerCompanyName = entity.CustomerCompanyName,
                ExecutorCompanyName = entity.ExecutorCompanyName,
                ProjectManagerId = entity.ProjectManagerId,
                ProjectStart = entity.ProjectStart,
                ProjectEnd = entity.ProjectEnd,
                Priority = entity.Priority,
                EmployeeIds = entity.ProjectEmployees?.Select(pe => pe.EmployeeId).ToList() ?? new List<long>()
            };
        }
    }
}