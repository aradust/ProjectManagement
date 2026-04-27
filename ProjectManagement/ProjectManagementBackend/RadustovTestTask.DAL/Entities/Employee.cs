namespace RadustovTestTask.DAL.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class Employee : IdentityUser<long>
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();
    }
}