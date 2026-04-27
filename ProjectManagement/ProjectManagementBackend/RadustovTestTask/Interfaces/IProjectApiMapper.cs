namespace RadustovTestTask.API.Interfaces
{
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public interface IProjectApiMapper
    {
        ProjectDto ToCreateDto(CreateProjectRequest request);
        ProjectDto ToUpdateDto(UpdateProjectRequest request);
    }
}