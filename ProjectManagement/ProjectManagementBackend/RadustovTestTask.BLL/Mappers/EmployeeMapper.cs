namespace RadustovTestTask.BLL.Mappers
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;

    public class EmployeeMapper : IEmployeeMapper
    {
        public Employee ToEntity(EmployeeDto dto)
        {
            return new Employee
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email
            };
        }

        public void UpdateEntity(Employee entity, EmployeeDto dto)
        {
            entity.FirstName = dto.FirstName;
            entity.MiddleName = dto.MiddleName;
            entity.LastName = dto.LastName;
            entity.Email = dto.Email;
            entity.UserName = dto.Email;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        public EmployeeDto ToDto(Employee entity, string role)
        {
            return new EmployeeDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                LastName = entity.LastName,
                Email = entity.Email,
                Role = role
            };
        }

        public EmployeeDto ToDtoFromCreate(Employee entity, string role)
        {
            return new EmployeeDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                LastName = entity.LastName,
                Email = entity.Email,
                Role = role
            };
        }
    }
}