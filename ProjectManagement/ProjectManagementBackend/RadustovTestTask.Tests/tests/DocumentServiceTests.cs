using Microsoft.EntityFrameworkCore;
using RadustovTestTask.BLL.Constants;
using RadustovTestTask.BLL.Interfaces;
using RadustovTestTask.BLL.Mappers;
using RadustovTestTask.BLL.Services;
using Xunit;

namespace RadustovTestTask.Tests.tests
{
    public class DocumentServiceTests : TestBase
    {
        private readonly IDocumentMapper _documentMapper = new DocumentMapper();

        private DocumentService CreateDocumentService()
        {
            return new DocumentService(
                DbContext,
                CurrentUserServiceMock.Object,
                _documentMapper,
                FileStorage
            );
        }

        [Fact]
        public async Task GetProjectDocumentsAsync_AsChief_ReturnsAllDocuments()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Doc Project", 10);

            var doc1 = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "file1.txt",
                OriginalName = "original1.txt",
                Size = 100
            };
            var doc2 = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "file2.txt",
                OriginalName = "original2.txt",
                Size = 200
            };
            await DbContext.Documents.AddRangeAsync(doc1, doc2);
            await DbContext.SaveChangesAsync();

            var result = await service.GetProjectDocumentsAsync(project.Id);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProjectDocumentsAsync_AsEmployee_WithAccess_ReturnsDocuments()
        {
            var employeeId = 50L;
            ResetCurrentUser();
            SetupCurrentUser(employeeId, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10, new List<long> { employeeId });

            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "file.txt",
                OriginalName = "original.txt",
                Size = 100
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            var result = await service.GetProjectDocumentsAsync(project.Id);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProjectDocumentsAsync_AsEmployee_WithoutAccess_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(50L, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.GetProjectDocumentsAsync(project.Id));
        }

        [Fact]
        public async Task AddDocumentAsync_AsChief_SavesFileAndCreatesRecord()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("DocProject", 10);

            var content = "Test content"u8.ToArray();
            var fileStream = new MemoryStream(content);
            string originalName = "test.txt";

            var result = await service.AddDocumentAsync(project.Id, originalName, fileStream);

            Assert.NotNull(result);
            Assert.Equal(originalName, result.OriginalName);
            Assert.Equal(content.Length, result.Size);
            Assert.True(FileStorage.FileExists(result.FileName));

            var docInDb = await DbContext.Documents.FirstOrDefaultAsync(d => d.Id == result.Id);
            Assert.NotNull(docInDb);
        }

        [Fact]
        public async Task AddDocumentAsync_AsProjectManager_SavesFile()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Manager Project", managerId);

            var fileStream = new MemoryStream("Data"u8.ToArray());

            var result = await service.AddDocumentAsync(project.Id, "file.txt", fileStream);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddDocumentAsync_AsEmployee_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10);
            var stream = new MemoryStream();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.AddDocumentAsync(project.Id, "file.txt", stream));
        }

        [Fact]
        public async Task DeleteDocumentAsync_AsChief_RemovesFileAndRecord()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10);

            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "stored.txt",
                OriginalName = "original.txt",
                Size = 100
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            await FileStorage.SaveFileAsync(doc.FileName, new MemoryStream("data"u8.ToArray()));

            var result = await service.DeleteDocumentAsync(doc.Id);

            Assert.True(result);
            Assert.False(FileStorage.FileExists(doc.FileName));

            var docInDb = await DbContext.Documents.FindAsync(doc.Id);
            Assert.Null(docInDb);
        }

        [Fact]
        public async Task DeleteDocumentAsync_AsProjectManager_RemovesDocument()
        {
            var managerId = 10L;
            ResetCurrentUser();
            SetupCurrentUser(managerId, AppRoles.Manager);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Manager Project", managerId);

            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "manager_file.txt",
                OriginalName = "original.txt",
                Size = 100
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            await FileStorage.SaveFileAsync(doc.FileName, new MemoryStream("data"u8.ToArray()));

            var result = await service.DeleteDocumentAsync(doc.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteDocumentAsync_AsEmployee_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(5L, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10);

            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "file.txt",
                OriginalName = "original.txt",
                Size = 100
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteDocumentAsync(doc.Id));
        }

        [Fact]
        public async Task DownloadDocumentAsync_AsProjectEmployee_ReturnsFileContent()
        {
            var employeeId = 50L;
            ResetCurrentUser();
            SetupCurrentUser(employeeId, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10, new List<long> { employeeId });

            var content = "Hello, world!"u8.ToArray();
            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "hello.txt",
                OriginalName = "greetings.txt",
                Size = content.Length
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            await FileStorage.SaveFileAsync(doc.FileName, new MemoryStream(content));

            var (fileContent, contentType, fileName) = await service.DownloadDocumentAsync(doc.Id);

            Assert.Equal(content, fileContent);
            Assert.Equal("text/plain", contentType);
            Assert.Equal(doc.OriginalName, fileName);
        }

        [Fact]
        public async Task DownloadDocumentAsync_NonExistentDocument_ThrowsFileNotFoundException()
        {
            ResetCurrentUser();
            SetupCurrentUser(1L, AppRoles.Chief);
            var service = CreateDocumentService();

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => service.DownloadDocumentAsync(999));
        }

        [Fact]
        public async Task DownloadDocumentAsync_AsEmployee_WithoutAccess_ThrowsUnauthorized()
        {
            ResetCurrentUser();
            SetupCurrentUser(50L, AppRoles.Employee);
            var service = CreateDocumentService();

            var project = await CreateProjectAsync("Project", 10);

            var doc = new RadustovTestTask.DAL.Entities.Document
            {
                ProjectId = project.Id,
                FileName = "secret.txt",
                OriginalName = "secret.txt",
                Size = 100
            };
            await DbContext.Documents.AddAsync(doc);
            await DbContext.SaveChangesAsync();

            await FileStorage.SaveFileAsync(doc.FileName, new MemoryStream("secret"u8.ToArray()));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DownloadDocumentAsync(doc.Id));
        }
    }
}