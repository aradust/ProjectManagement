namespace RadustovTestTask.BLL.Services
{
    using Microsoft.EntityFrameworkCore;
    using RadustovTestTask.BLL.Constants;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL;
    using RadustovTestTask.DAL.Entities;

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProjectMapper _projectMapper;
        private readonly IDocumentMapper _documentMapper;

        public ProjectService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            IProjectMapper projectMapper,
            IDocumentMapper documentMapper)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _projectMapper = projectMapper;
            _documentMapper = documentMapper;
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto dto, List<long> employeeIds)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();

            if (!_currentUserService.IsInRole(AppRoles.Chief) && !_currentUserService.IsInRole(AppRoles.Manager))
            {
                throw new UnauthorizedAccessException("Only Managers and Chiefs can create projects.");
            }

            Project project = _projectMapper.ToEntity(dto);

            foreach (long empId in employeeIds)
            {
                project.ProjectEmployees.Add(new ProjectEmployee
                {
                    EmployeeId = empId
                });
            }

            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            dto.Id = project.Id;
            return dto;
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(long id)
        {
            IQueryable<Project> query = _dbContext.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.ProjectEmployees)
                .AsQueryable();

            query = ApplyRoleFilter(query);

            Project? project = await query.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return null;
            }

            return _projectMapper.ToDto(project);
        }

        public async Task<ProjectDto?> UpdateProjectAsync(ProjectDto dto, bool updateTeamOnly = false)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            Project? project = await _dbContext.Projects
                .Include(p => p.ProjectEmployees)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project == null)
            {
                return null;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);

            if (!isChief && (!isManager || project.ProjectManagerId != currentUserId))
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this project.");
            }

            if (updateTeamOnly && !isChief)
            {
                List<long> existingEmployees = project.ProjectEmployees.Select(pe => pe.EmployeeId).ToList();
                List<long> newEmployees = dto.EmployeeIds ?? new List<long>();

                List<long> toAdd = newEmployees.Except(existingEmployees).ToList();
                List<long> toRemove = existingEmployees.Except(newEmployees).ToList();

                foreach (long empId in toAdd)
                {
                    _dbContext.ProjectEmployees.Add(new ProjectEmployee { ProjectId = project.Id, EmployeeId = empId });
                }

                foreach (long empId in toRemove)
                {
                    ProjectEmployee? relation = await _dbContext.ProjectEmployees
                        .FirstOrDefaultAsync(pe => pe.ProjectId == project.Id && pe.EmployeeId == empId);

                    if (relation != null)
                    {
                        _dbContext.ProjectEmployees.Remove(relation);
                    }
                }
            }
            else
            {
                _projectMapper.UpdateEntity(project, dto);

                if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
                {
                    List<long> existing = project.ProjectEmployees.Select(pe => pe.EmployeeId).ToList();
                    List<long> toAdd = dto.EmployeeIds.Except(existing).ToList();
                    List<long> toRemove = existing.Except(dto.EmployeeIds).ToList();

                    foreach (long empId in toAdd)
                    {
                        _dbContext.ProjectEmployees.Add(new ProjectEmployee { ProjectId = project.Id, EmployeeId = empId });
                    }

                    foreach (long empId in toRemove)
                    {
                        ProjectEmployee? relation = await _dbContext.ProjectEmployees
                            .FirstOrDefaultAsync(pe => pe.ProjectId == project.Id && pe.EmployeeId == empId);

                        if (relation != null)
                        {
                            _dbContext.ProjectEmployees.Remove(relation);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> RemoveProjectAsync(long id)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            Project? project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                return false;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);

            if (!isChief)
            {
                throw new UnauthorizedAccessException("Only Chief can delete projects.");
            }

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddEmployeeToProjectAsync(long projectId, long employeeId)
        {
            Project? project = await _dbContext.Projects.FindAsync(projectId);
            if (project == null)
            {
                return false;
            }

            long? currentUserId = _currentUserService.GetCurrentUserId();
            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);

            if (!isChief && (!isManager || project.ProjectManagerId != currentUserId))
            {
                throw new UnauthorizedAccessException("Only project manager or chief can assign employees.");
            }

            bool exists = await _dbContext.ProjectEmployees
                .AnyAsync(pe => pe.ProjectId == projectId && pe.EmployeeId == employeeId);

            if (exists)
            {
                return false;
            }

            await _dbContext.ProjectEmployees.AddAsync(new ProjectEmployee
            {
                ProjectId = projectId,
                EmployeeId = employeeId
            });

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveEmployeeFromProjectAsync(long projectId, long employeeId)
        {
            Project? project = await _dbContext.Projects.FindAsync(projectId);
            if (project == null)
            {
                return false;
            }

            long? currentUserId = _currentUserService.GetCurrentUserId();
            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);

            if (!isChief && (!isManager || project.ProjectManagerId != currentUserId))
            {
                throw new UnauthorizedAccessException("Only project manager or chief can remove employees.");
            }

            ProjectEmployee? relation = await _dbContext.ProjectEmployees
                .FirstOrDefaultAsync(pe => pe.ProjectId == projectId && pe.EmployeeId == employeeId);

            if (relation == null)
            {
                return false;
            }

            _dbContext.ProjectEmployees.Remove(relation);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<EmployeeDto>> GetProjectEmployeesAsync(long projectId)
        {
            IQueryable<Project> query = _dbContext.Projects.AsQueryable();
            query = ApplyRoleFilter(query);

            bool hasAccess = await query.AnyAsync(p => p.Id == projectId);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Access denied to this project.");
            }

            return await _dbContext.ProjectEmployees
                .Where(pe => pe.ProjectId == projectId)
                .Select(pe => new EmployeeDto
                {
                    Id = pe.Employee.Id,
                    FirstName = pe.Employee.FirstName,
                    MiddleName = pe.Employee.MiddleName,
                    LastName = pe.Employee.LastName,
                    Email = pe.Employee.Email
                })
                .ToListAsync();
        }

        public async Task<List<ProjectDto>> SearchProjectsAsync(
            string? query = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            int? priorityFrom = null,
            int? priorityTo = null,
            string? sortBy = null)
        {
            IQueryable<Project> dbQuery = _dbContext.Projects
                .Include(p => p.ProjectEmployees)
                .AsQueryable();

            dbQuery = ApplyRoleFilter(dbQuery);

            if (!string.IsNullOrWhiteSpace(query))
            {
                dbQuery = dbQuery.Where(p =>
                    p.ProjectName.Contains(query) ||
                    p.CustomerCompanyName.Contains(query) ||
                    p.ExecutorCompanyName.Contains(query));
            }

            if (startDateFrom.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.ProjectStart >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.ProjectStart <= startDateTo.Value);
            }

            if (priorityFrom.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Priority >= priorityFrom.Value);
            }

            if (priorityTo.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Priority <= priorityTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "priority_desc":
                        dbQuery = dbQuery.OrderByDescending(p => p.Priority);
                        break;
                    case "priority_asc":
                        dbQuery = dbQuery.OrderBy(p => p.Priority);
                        break;
                    case "name_desc":
                        dbQuery = dbQuery.OrderByDescending(p => p.ProjectName);
                        break;
                    case "name_asc":
                    default:
                        dbQuery = dbQuery.OrderBy(p => p.ProjectName);
                        break;
                }
            }
            else
            {
                dbQuery = dbQuery.OrderBy(p => p.ProjectName);
            }

            List<Project> projects = await dbQuery.ToListAsync();
            return projects.Select(_projectMapper.ToDto).ToList();
        }

        public async Task<bool> ProjectNameExistsAsync(string name, long? excludeProjectId)
        {
            IQueryable<Project> query = _dbContext.Projects.AsQueryable();

            if (excludeProjectId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProjectId.Value);
            }

            return await query.AnyAsync(p => p.ProjectName == name);
        }

        private IQueryable<Project> ApplyRoleFilter(IQueryable<Project> query)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);
            bool isEmployee = _currentUserService.IsInRole(AppRoles.Employee);

            if (!isChief && currentUserId.HasValue)
            {
                if (isManager)
                {
                    query = query.Where(p => p.ProjectManagerId == currentUserId.Value);
                }
                else if (isEmployee)
                {
                    query = query.Where(p => p.ProjectEmployees.Any(pe => pe.EmployeeId == currentUserId.Value));
                }
                else
                {
                    query = query.Where(p => false);
                }
            }

            return query;
        }
    }
}