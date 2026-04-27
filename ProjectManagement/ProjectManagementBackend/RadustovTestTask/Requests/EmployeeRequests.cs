namespace RadustovTestTask.API.Requests
{
    using System.ComponentModel.DataAnnotations;

    public record CreateEmployeeRequest(
        [Required] string FirstName,
        [Required] string LastName,
        string? MiddleName,
        [Required][EmailAddress] string Email
    );

    public class CreateEmployeeWithPasswordRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public long Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Role { get; set; }
    }
}