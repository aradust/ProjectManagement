namespace RadustovTestTask.API.Interfaces
{
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.DTO;

    public interface IEmployeeApiMapper
    {
        EmployeeDto ToUpdateDto(UpdateEmployeeRequest request);
    }
}