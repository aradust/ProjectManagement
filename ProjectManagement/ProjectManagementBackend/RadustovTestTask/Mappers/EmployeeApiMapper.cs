namespace RadustovTestTask.API.Mappers
{
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public class EmployeeApiMapper : IEmployeeApiMapper
    {
        public EmployeeDto ToUpdateDto(UpdateEmployeeRequest request)
        {
            return new EmployeeDto
            {
                Id = request.Id,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role
            };
        }
    }
}