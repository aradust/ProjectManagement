namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.DAL.Entities;

    public interface IProjectMapper
    {
        Project ToEntity(ProjectDto dto);
        void UpdateEntity(Project entity, ProjectDto dto);
        ProjectDto ToDto(Project entity);
    }
}