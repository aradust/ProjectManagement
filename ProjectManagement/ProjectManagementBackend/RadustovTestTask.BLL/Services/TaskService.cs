namespace RadustovTestTask.BLL.Services
{
    using Microsoft.EntityFrameworkCore;
    using RadustovTestTask.BLL.Constants;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL;
    using RadustovTestTask.DAL.Entities;

    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITaskMapper _taskMapper;

        public TaskService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            ITaskMapper taskMapper)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _taskMapper = taskMapper;
        }

        public async Task<TaskItemDto> CreateAsync(TaskItemDto dto)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (!_currentUserService.IsInRole(AppRoles.Chief))
            {
                throw new UnauthorizedAccessException("Only Managers and Chiefs can create tasks.");
            }

            if (_currentUserService.IsInRole(AppRoles.Manager))
            {
                Project? project = await _dbContext.Projects.FindAsync(dto.ProjectId);
                if (project == null || project.ProjectManagerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Managers can create tasks only in their own projects.");
                }
            }

            TaskItem entity = _taskMapper.ToEntity(dto, currentUserId.Value);

            await _dbContext.TaskItem.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            dto.Id = entity.Id;
            return dto;
        }

        public async Task<TaskItemDto?> GetByIdAsync(long id)
        {
            IQueryable<TaskItem> query = _dbContext.TaskItem
                .Include(t => t.Project)
                .Include(t => t.Author)
                .Include(t => t.Executor)
                .AsQueryable();

            query = ApplyRoleFilter(query);

            TaskItem? entity = await query.FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return null;
            }

            return _taskMapper.ToDto(entity);
        }

        public async Task<List<TaskItemDto>> GetAllAsync(long? projectId, int? status, string? sort)
        {
            IQueryable<TaskItem> query = _dbContext.TaskItem
                .Include(t => t.Project)
                .AsQueryable();

            query = ApplyRoleFilter(query);

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId);
            }

            if (status.HasValue)
            {
                TaskStatus dalStatus = (TaskStatus)status.Value;
                query = query.Where(t => t.Status == dalStatus);
            }

            query = sort switch
            {
                "priority" => query.OrderByDescending(t => t.Priority),
                "status" => query.OrderBy(t => t.Status),
                _ => query.OrderBy(t => t.Id)
            };

            List<TaskItem> entities = await query.ToListAsync();
            return entities.Select(_taskMapper.ToDto).ToList();
        }

        public async Task<TaskItemDto?> UpdateAsync(TaskItemDto dto)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException();
            }

            TaskItem? entity = await _dbContext.TaskItem.FindAsync(dto.Id);
            if (entity == null)
            {
                return null;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);
            bool isEmployee = _currentUserService.IsInRole(AppRoles.Employee);

            if (isEmployee)
            {
                if (entity.ExecutorId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Employees can only update their own tasks status.");
                }

                entity.Status = (DAL.Entities.TaskStatus)dto.Status;
            }
            else if (isManager)
            {
                Project? project = await _dbContext.Projects.FindAsync(entity.ProjectId);
                if (project == null || project.ProjectManagerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Managers can update tasks only in their own projects.");
                }

                entity.ExecutorId = dto.ExecutorId;
                entity.Status = (DAL.Entities.TaskStatus)dto.Status;
            }
            else if (isChief)
            {
                _taskMapper.UpdateEntity(entity, dto);
            }

            await _dbContext.SaveChangesAsync();
            return _taskMapper.ToDto(entity);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            TaskItem? entity = await _dbContext.TaskItem.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);

            if (!isChief)
            {
                if (!isManager)
                {
                    throw new UnauthorizedAccessException("Access denied.");
                }

                Project? project = await _dbContext.Projects.FindAsync(entity.ProjectId);
                if (project == null || project.ProjectManagerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Managers can delete tasks only in their own projects.");
                }
            }

            _dbContext.TaskItem.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private IQueryable<TaskItem> ApplyRoleFilter(IQueryable<TaskItem> query)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);
            bool isEmployee = _currentUserService.IsInRole(AppRoles.Employee);

            if (!isChief && currentUserId.HasValue)
            {
                if (isManager)
                {
                    query = query.Where(t => t.Project.ProjectManagerId == currentUserId.Value);
                }
                else if (isEmployee)
                {
                    query = query.Where(t => t.ExecutorId == currentUserId.Value);
                }
                else
                {
                    query = query.Where(t => false);
                }
            }

            return query;
        }

        public async Task<bool> TaskTitleExistsAsync(string title, long projectId, long? excludeTaskId)
        {
            IQueryable<TaskItem> query = _dbContext.TaskItem.Where(t => t.ProjectId == projectId && t.Title == title);
            if (excludeTaskId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTaskId.Value);
            }

            return await query.AnyAsync();
        }
    }
}