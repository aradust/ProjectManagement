namespace RadustovTestTask.API.Mappers
{
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public class TaskApiMapper : ITaskApiMapper
    {
        public TaskItemDto ToCreateDto(CreateTaskRequest request, long authorId)
        {
            return new TaskItemDto
            {
                Title = request.Title,
                Comment = request.Comment,
                Priority = request.Priority,
                ProjectId = request.ProjectId,
                AuthorId = authorId,
                ExecutorId = request.ExecutorId,
                Status = TaskStatusDto.ToDo
            };
        }

        public TaskItemDto ToUpdateDto(UpdateTaskRequest request, long id)
        {
            return new TaskItemDto
            {
                Id = id,
                Title = request.Title,
                Comment = request.Comment,
                Priority = request.Priority,
                Status = (TaskStatusDto)request.Status,
                ExecutorId = request.ExecutorId
            };
        }
    }
}