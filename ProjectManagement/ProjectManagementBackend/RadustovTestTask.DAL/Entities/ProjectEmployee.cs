namespace RadustovTestTask.DAL.Entities
{
    public class ProjectEmployee
    {
        public long ProjectId { get; set; }
        public Project Project { get; set; }

        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}