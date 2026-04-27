
namespace RadustovTestTask.BLL.DTO
{
    public class ProjectDto
    {
        public long Id { get; set; }

        public string ProjectName { get; set; }

        public string CustomerCompanyName { get; set; }

        public string ExecutorCompanyName { get; set; }

        public long ProjectManagerId { get; set; }

        public DateTime ProjectStart { get; set; }

        public DateTime ProjectEnd { get; set; }

        public int Priority { get; set; }

        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();

        public List<long> EmployeeIds { get; set; } = new List<long>();
    }
}
