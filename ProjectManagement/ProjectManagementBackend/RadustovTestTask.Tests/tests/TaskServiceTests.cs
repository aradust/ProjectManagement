using Moq;
using RadustovTestTask.BLL.Constants;
using RadustovTestTask.BLL.DTO;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.BLL.Mappers;
using RadustovTestTask.BLL.Services;
using Xunit;

namespace RadustovTestTask.Tests.tests
{
    public class TaskServiceTests : TestBase
    {
        private readonly ITaskMapper _taskMapper = new TaskMapper();

        private TaskService CreateTaskService()
        {

            var mock = CurrentUserServiceMock.Object;
            System.Diagnostics.Debug.WriteLine($"Mock in factory - UserId: {mock.GetCurrentUserId()}, IsChief: {mock.IsInRole(AppRoles.Chief)}");
            return new TaskService(
                DbContext,
                CurrentUserServiceMock.Object,
                _taskMapper
            );
        }

        [Fact]
        public async Task CreateAsync_AsManager_ForOtherProject_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(10L, AppRoles.Manager);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Other Project", managerId: 99);

            var dto = new TaskItemDto { Title = "Task", ProjectId = project.Id };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_AsEmployee_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", managerId: 10);
            var dto = new TaskItemDto { Title = "Task", ProjectId = project.Id };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task GetAllAsync_AsChief_ReturnsAllTasks()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project1 = await CreateProjectAsync("Project 1", 10);
            var project2 = await CreateProjectAsync("Project 2", 20);

            await CreateTaskAsync("Task 1", project1.Id, authorId: 10);
            await CreateTaskAsync("Task 2", project2.Id, authorId: 20);

            var result = await service.GetAllAsync(null, null, null);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_AsManager_ReturnsOnlyOwnProjectsTasks()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateTaskService();

            var ownProject = await CreateProjectAsync("Own Project", managerId);
            var otherProject = await CreateProjectAsync("Other Project", 99);

            await CreateTaskAsync("Own Task", ownProject.Id, authorId: managerId);
            await CreateTaskAsync("Other Task", otherProject.Id, authorId: 99);

            var result = await service.GetAllAsync(null, null, null);

            Assert.Single(result);
            Assert.Equal("Own Task", result[0].Title);
        }

        [Fact]
        public async Task GetAllAsync_AsEmployee_ReturnsOnlyAssignedTasks()
        {
            var employeeId = 50L;
            ResetCurrentUser();
            SetupCurrentUser(employeeId, AppRoles.Employee);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);

            await CreateTaskAsync("Assigned Task", project.Id, authorId: 10, executorId: employeeId);
            await CreateTaskAsync("Unassigned Task", project.Id, authorId: 10, executorId: 99);

            var result = await service.GetAllAsync(null, null, null);

            Assert.Single(result);
            Assert.Equal("Assigned Task", result[0].Title);
        }

        [Fact]
        public async Task GetAllAsync_WithProjectFilter_ReturnsFilteredTasks()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project1 = await CreateProjectAsync("Project 1", 10);
            var project2 = await CreateProjectAsync("Project 2", 20);

            await CreateTaskAsync("Task 1", project1.Id, authorId: 10);
            await CreateTaskAsync("Task 2", project2.Id, authorId: 20);

            var result = await service.GetAllAsync(project1.Id, null, null);

            Assert.Single(result);
            Assert.Equal("Task 1", result[0].Title);
        }

        [Fact]
        public async Task UpdateAsync_AsChief_UpdatesAllFields()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            var task = await CreateTaskAsync("Old Title", project.Id, authorId: 10);

            var updateDto = new TaskItemDto
            {
                Id = task.Id,
                Title = "New Title",
                Comment = "New Comment",
                Priority = 8,
                Status = TaskStatusDto.InProgress,
                ExecutorId = 50
            };

            var result = await service.UpdateAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("New Title", result.Title);
            Assert.Equal(TaskStatusDto.InProgress, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_AsManager_ForOwnProject_UpdatesExecutorAndStatus()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", managerId);
            var task = await CreateTaskAsync("Task", project.Id, authorId: managerId);

            var updateDto = new TaskItemDto
            {
                Id = task.Id,
                Title = "Changed Title",
                Status = TaskStatusDto.Done,
                ExecutorId = 50
            };

            var result = await service.UpdateAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("Task", result.Title);
            Assert.Equal(TaskStatusDto.Done, result.Status);
            Assert.Equal(50, result.ExecutorId);
        }

        [Fact]
        public async Task UpdateAsync_AsEmployee_UpdatesOnlyOwnTaskStatus()
        {
            var employeeId = 5L;
            ResetCurrentUser();
            SetupCurrentUser(employeeId, AppRoles.Employee);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            var task = await CreateTaskAsync("Employee Task", project.Id, authorId: 10, executorId: employeeId);

            var updateDto = new TaskItemDto
            {
                Id = task.Id,
                Title = "New Title",
                Status = TaskStatusDto.InProgress
            };

            var result = await service.UpdateAsync(updateDto);

            Assert.Equal(TaskStatusDto.InProgress, result.Status);
            Assert.Equal("Employee Task", result.Title);
        }

        [Fact]
        public async Task UpdateAsync_AsEmployee_ForOtherTask_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            var task = await CreateTaskAsync("Task", project.Id, authorId: 10, executorId: 99);

            var updateDto = new TaskItemDto { Id = task.Id, Status = TaskStatusDto.Done };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_AsChief_DeletesAnyTask()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            var task = await CreateTaskAsync("To Delete", project.Id, authorId: 10);

            var result = await service.DeleteAsync(task.Id);

            Assert.True(result);
            var deleted = await DbContext.TaskItem.FindAsync(task.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_AsManager_ForOwnProject_DeletesTask()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", managerId);
            var task = await CreateTaskAsync("Manager Task", project.Id, authorId: managerId);

            var result = await service.DeleteAsync(task.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_AsManager_ForOtherProject_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(10L, AppRoles.Manager);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Other Project", 99);
            var task = await CreateTaskAsync("Task", project.Id, authorId: 99);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(task.Id));
        }

        [Fact]
        public async Task DeleteAsync_AsEmployee_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            var task = await CreateTaskAsync("Task", project.Id, authorId: 10, executorId: 5);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(task.Id));
        }

        [Fact]
        public async Task TaskTitleExistsAsync_SameTitleInProject_ReturnsTrue()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project = await CreateProjectAsync("Project", 10);
            await CreateTaskAsync("Unique Task", project.Id, authorId: 10);

            var result = await service.TaskTitleExistsAsync("Unique Task", project.Id, null);

            Assert.True(result);
        }

        [Fact]
        public async Task TaskTitleExistsAsync_DifferentProject_ReturnsFalse()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateTaskService();

            var project1 = await CreateProjectAsync("Project 1", 10);
            var project2 = await CreateProjectAsync("Project 2", 20);

            await CreateTaskAsync("Same Title", project1.Id, authorId: 10);

            var result = await service.TaskTitleExistsAsync("Same Title", project2.Id, null);

            Assert.False(result);
        }
    }
}