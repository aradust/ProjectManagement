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
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IProjectApiMapper _projectApiMapper;
        private readonly ICurrentUserService _currentUserService;

        public ProjectsController(
            IProjectService projectService,
            IProjectApiMapper projectApiMapper,
            ICurrentUserService currentUserService)
        {
            _projectService = projectService;
            _projectApiMapper = projectApiMapper;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = $"{AppRoles.Chief},{AppRoles.Manager}")]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            ProjectDto dto = _projectApiMapper.ToCreateDto(request);
            ProjectDto created = await _projectService.CreateProjectAsync(dto, request.EmployeeIds);
            return CreatedAtAction(nameof(GetProjectById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> SearchProjects(
            [FromQuery] string? search,
            [FromQuery] DateTime? startDateFrom,
            [FromQuery] DateTime? startDateTo,
            [FromQuery] int? priorityFrom,
            [FromQuery] int? priorityTo,
            [FromQuery] string? sortBy)
        {
            List<ProjectDto> projects = await _projectService.SearchProjectsAsync(
                search, startDateFrom, startDateTo, priorityFrom, priorityTo, sortBy);

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById([FromRoute] long id)
        {
            ProjectDto? project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] long id, [FromBody] UpdateProjectRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            ProjectDto dto = _projectApiMapper.ToUpdateDto(request);
            ProjectDto? updated = await _projectService.UpdateProjectAsync(dto, request.UpdateTeamOnly);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveProject([FromRoute] long id)
        {
            bool result = await _projectService.RemoveProjectAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{projectId}/employees/{employeeId}")]
        public async Task<IActionResult> AddEmployeeToProject([FromRoute] long projectId, [FromRoute] long employeeId)
        {
            bool result = await _projectService.AddEmployeeToProjectAsync(projectId, employeeId);

            if (!result)
            {
                return BadRequest("Employee already assigned or invalid data");
            }

            return NoContent();
        }

        [HttpDelete("{projectId}/employees/{employeeId}")]
        public async Task<IActionResult> RemoveEmployeeFromProject([FromRoute] long projectId, [FromRoute] long employeeId)
        {
            bool result = await _projectService.RemoveEmployeeFromProjectAsync(projectId, employeeId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{projectId}/employees")]
        public async Task<IActionResult> GetProjectEmployees([FromRoute] long projectId)
        {
            List<EmployeeDto> employees = await _projectService.GetProjectEmployeesAsync(projectId);
            return Ok(employees);
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckProjectNameExists([FromQuery] string name, [FromQuery] long? excludeId)
        {
            bool exists = await _projectService.ProjectNameExistsAsync(name, excludeId);
            return Ok(new { exists });
        }
    }
}