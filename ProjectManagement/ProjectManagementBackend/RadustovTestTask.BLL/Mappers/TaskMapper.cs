namespace RadustovTestTask.BLL.Mappers
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;

    public class TaskMapper : ITaskMapper
    {
        public TaskItem ToEntity(TaskItemDto dto, long authorId)
        {
            return new TaskItem
            {
                Title = dto.Title,
                Comment = dto.Comment,
                Priority = dto.Priority,
                ProjectId = dto.ProjectId,
                AuthorId = authorId,
                ExecutorId = dto.ExecutorId,
                Status = (DAL.Entities.TaskStatus)dto.Status
            };
        }

        public void UpdateEntity(TaskItem entity, TaskItemDto dto)
        {
            entity.Title = dto.Title;
            entity.Comment = dto.Comment;
            entity.Priority = dto.Priority;
            entity.ExecutorId = dto.ExecutorId;
            entity.Status = (DAL.Entities.TaskStatus)dto.Status;
        }

        public TaskItemDto ToDto(TaskItem entity)
        {
            return new TaskItemDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Comment = entity.Comment,
                Priority = entity.Priority,
                Status = (TaskStatusDto)entity.Status,
                ProjectId = entity.ProjectId,
                AuthorId = entity.AuthorId,
                ExecutorId = entity.ExecutorId
            };
        }
    }
}