namespace RadustovTestTask.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.Constants;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeApiMapper _employeeApiMapper;

        public EmployeesController(
            IEmployeeService employeeService,
            IEmployeeApiMapper employeeApiMapper)
        {
            _employeeService = employeeService;
            _employeeApiMapper = employeeApiMapper;
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Chief)]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeWithPasswordRequest request)
        {
            try
            {
                EmployeeDto result = await _employeeService.CreateEmployeeWithPasswordAsync(
                    request.FirstName,
                    request.LastName,
                    request.MiddleName,
                    request.Email,
                    request.Password,
                    request.Role);

                return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] string? searchTerm)
        {
            List<EmployeeDto> employees = string.IsNullOrEmpty(searchTerm)
                ? await _employeeService.GetAllEmployeesAsync()
                : await _employeeService.SearchEmployeesAsync(searchTerm);

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById([FromRoute] long id)
        {
            EmployeeDto? employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] long id, [FromBody] UpdateEmployeeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            EmployeeDto dto = _employeeApiMapper.ToUpdateDto(request);
            EmployeeDto? result = await _employeeService.UpdateEmployeeAsync(dto);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = AppRoles.Chief)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveEmployee([FromRoute] long id)
        {
            try
            {
                bool result = await _employeeService.RemoveEmployeeAsync(id);

                if (!result)
                {
                    return BadRequest(new { message = "Employee not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email, [FromQuery] long? excludeId)
        {
            bool exists = await _employeeService.EmailExistsAsync(email, excludeId);
            return Ok(new { exists });
        }
    }
}