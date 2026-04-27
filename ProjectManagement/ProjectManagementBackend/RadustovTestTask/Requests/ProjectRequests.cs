namespace RadustovTestTask.API.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Project name is required")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Customer company name is required")]
        public string CustomerCompanyName { get; set; }

        [Required(ErrorMessage = "Executor company name is required")]
        public string ExecutorCompanyName { get; set; }

        [Required(ErrorMessage = "Project manager ID is required")]
        public long ProjectManagerId { get; set; }

        [Required(ErrorMessage = "Project start date is required")]
        public DateTime ProjectStart { get; set; }

        [Required(ErrorMessage = "Project end date is required")]
        public DateTime ProjectEnd { get; set; }

        [Range(1, 10, ErrorMessage = "Priority must be between 1 and 10")]
        public int Priority { get; set; }

        public List<long> EmployeeIds { get; set; } = new List<long>();

        public bool UpdateTeamOnly { get; set; }
    }

    public class UpdateProjectRequest
    {
        [Required(ErrorMessage = "Project ID is required")]
        public long Id { get; set; }

        public string ProjectName { get; set; }

        public string CustomerCompanyName { get; set; }

        public string ExecutorCompanyName { get; set; }

        public long ProjectManagerId { get; set; }

        public DateTime ProjectStart { get; set; }

        public DateTime ProjectEnd { get; set; }

        [Range(1, 10, ErrorMessage = "Priority must be between 1 and 10")]
        public int Priority { get; set; }

        public List<long> EmployeeIds { get; set; } = new List<long>();

        public bool UpdateTeamOnly { get; set; }
    }
}