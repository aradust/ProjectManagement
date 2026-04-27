namespace RadustovTestTask.DAL.Entities
{
    public class TaskItem
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string? Comment { get; set; }

        public int Priority { get; set; }

        public TaskStatus Status { get; set; }

        public long ProjectId { get; set; }
        public Project Project { get; set; }

        public long AuthorId { get; set; }
        public Employee Author { get; set; }

        public long? ExecutorId { get; set; }
        public Employee? Executor { get; set; }
    }
}