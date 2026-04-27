namespace RadustovTestTask.BLL.DTO
{
    public class TaskItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Comment { get; set; }
        public int Priority { get; set; }

        public TaskStatusDto Status { get; set; }

        public long ProjectId { get; set; }
        public long AuthorId { get; set; }
        public long? ExecutorId { get; set; }

    }

    public enum TaskStatusDto
    {
        ToDo,
        InProgress,
        Done
    }
}