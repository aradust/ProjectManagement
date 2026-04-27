namespace RadustovTestTask.DAL.Entities
{
    public class Project
    {
        public long Id { get; set; }

        public string ProjectName { get; set; }

        public string CustomerCompanyName { get; set; }

        public string ExecutorCompanyName { get; set; }

        public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();

        public long ProjectManagerId { get; set; }

        public Employee ProjectManager { get; set; }

        public DateTime ProjectStart { get; set; }

        public DateTime ProjectEnd { get; set; }

        public int Priority { get; set; }

        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}