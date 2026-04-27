namespace RadustovTestTask.API.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class CreateTaskRequest
    {
        [Required]
        public string Title { get; set; }

        public string? Comment { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; }

        [Required]
        public long ProjectId { get; set; }

        public long? ExecutorId { get; set; }
    }

    public class UpdateTaskRequest
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Comment { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; }

        public int Status { get; set; }

        public long? ExecutorId { get; set; }
    }
}
