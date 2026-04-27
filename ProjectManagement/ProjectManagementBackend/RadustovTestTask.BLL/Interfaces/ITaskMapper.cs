namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.DAL.Entities;

    public interface ITaskMapper
    {
        TaskItem ToEntity(TaskItemDto dto, long authorId);
        void UpdateEntity(TaskItem entity, TaskItemDto dto);
        TaskItemDto ToDto(TaskItem entity);
    }
}