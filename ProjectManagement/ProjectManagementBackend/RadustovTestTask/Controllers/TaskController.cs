namespace RadustovTestTask.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.API.Requests;
    using RadustovTestTask.BLL.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITaskApiMapper _taskApiMapper;
        private readonly ICurrentUserService _currentUserService;

        public TasksController(
            ITaskService taskService,
            ITaskApiMapper taskApiMapper,
            ICurrentUserService currentUserService)
        {
            _taskService = taskService;
            _taskApiMapper = taskApiMapper;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var dto = _taskApiMapper.ToCreateDto(request, currentUserId.Value);
            var result = await _taskService.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] long? projectId,
            [FromQuery] int? status,
            [FromQuery] string? sort)
        {
            var tasks = await _taskService.GetAllAsync(projectId, status, sort);
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateTaskRequest request)
        {
            var dto = _taskApiMapper.ToUpdateDto(request, id);
            var result = await _taskService.UpdateAsync(dto);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            bool result = await _taskService.DeleteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckTaskTitleExists(
            [FromQuery] string title,
            [FromQuery] long projectId,
            [FromQuery] long? excludeId)
        {
            bool exists = await _taskService.TaskTitleExistsAsync(title, projectId, excludeId);
            return Ok(new { exists });
        }
    }
}