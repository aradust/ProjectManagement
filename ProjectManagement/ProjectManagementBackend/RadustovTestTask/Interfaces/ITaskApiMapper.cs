namespace RadustovTestTask.API.Interfaces
{
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public interface ITaskApiMapper
    {
        TaskItemDto ToCreateDto(CreateTaskRequest request, long authorId);
        TaskItemDto ToUpdateDto(UpdateTaskRequest request, long id);
    }
}