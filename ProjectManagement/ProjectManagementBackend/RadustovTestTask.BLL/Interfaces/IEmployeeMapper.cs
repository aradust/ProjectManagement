namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.DAL.Entities;

    public interface IEmployeeMapper
    {
        Employee ToEntity(EmployeeDto dto);
        void UpdateEntity(Employee entity, EmployeeDto dto);
        EmployeeDto ToDto(Employee entity, string role);
        EmployeeDto ToDtoFromCreate(Employee entity, string role);
    }
}