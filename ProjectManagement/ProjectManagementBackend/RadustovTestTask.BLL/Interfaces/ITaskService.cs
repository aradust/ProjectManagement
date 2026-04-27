namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface ITaskService
    {
        Task<TaskItemDto> CreateAsync(TaskItemDto dto);
        Task<TaskItemDto?> GetByIdAsync(long id);
        Task<List<TaskItemDto>> GetAllAsync(long? projectId, int? status, string? sort);
        Task<TaskItemDto?> UpdateAsync(TaskItemDto dto);
        Task<bool> DeleteAsync(long id);
        Task<bool> TaskTitleExistsAsync(string title, long projectId, long? excludeTaskId);
    }
}