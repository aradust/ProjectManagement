namespace RadustovTestTask.BLL.Services
{
    using Microsoft.EntityFrameworkCore;
    using RadustovTestTask.BLL.Constants;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL;
    using RadustovTestTask.DAL.Entities;

    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDocumentMapper _documentMapper;
        private readonly IFileStorageService _fileStorage;

        public DocumentService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            IDocumentMapper documentMapper,
            IFileStorageService fileStorage)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _documentMapper = documentMapper;
            _fileStorage = fileStorage;
        }

        public async Task<List<DocumentDto>> GetProjectDocumentsAsync(long projectId)
        {
            bool hasAccess = await CheckProjectAccessAsync(projectId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Access denied to this project.");
            }

            List<Document> documents = await _dbContext.Documents
                .Where(d => d.ProjectId == projectId)
                .ToListAsync();

            return documents.Select(_documentMapper.ToDto).ToList();
        }

        public async Task<DocumentDto> AddDocumentAsync(long projectId, string originalFileName, Stream fileStream)
        {
            bool canModify = await CheckProjectModifyAccessAsync(projectId);
            if (!canModify)
            {
                throw new UnauthorizedAccessException("Only project manager or chief can add documents.");
            }

            string fileName = $"{projectId}_{Guid.NewGuid()}_{Path.GetFileName(originalFileName)}";

            try
            {
                await _fileStorage.SaveFileAsync(fileName, fileStream);

                Document document = _documentMapper.ToEntity(
                    projectId,
                    fileName,
                    originalFileName,
                    fileStream.Length
                );

                await _dbContext.Documents.AddAsync(document);
                await _dbContext.SaveChangesAsync();

                return _documentMapper.ToDto(document);
            }
            catch
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(fileName);
                }
                catch
                {
                }
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(long documentId)
        {
            Document? document = await _dbContext.Documents
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return false;
            }

            bool canModify = await CheckProjectModifyAccessAsync(document.ProjectId);
            if (!canModify)
            {
                throw new UnauthorizedAccessException("Only project manager or chief can delete documents.");
            }

            try
            {
                await _fileStorage.DeleteFileAsync(document.FileName);
            }
            catch
            {
            }

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<(byte[] FileContent, string ContentType, string FileName)> DownloadDocumentAsync(long documentId)
        {
            Document? document = await _dbContext.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new FileNotFoundException("Document not found");
            }

            bool hasAccess = await CheckProjectAccessAsync(document.ProjectId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Access denied to this document.");
            }

            byte[] fileContent = await _fileStorage.ReadFileAsync(document.FileName);

            string contentType = GetContentType(document.OriginalName);

            return (fileContent, contentType, document.OriginalName);
        }

        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private async Task<bool> CheckProjectAccessAsync(long projectId)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return false;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            if (isChief)
            {
                return true;
            }

            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);
            if (isManager)
            {
                return await _dbContext.Projects.AnyAsync(p => p.Id == projectId && p.ProjectManagerId == currentUserId);
            }

            bool isEmployee = _currentUserService.IsInRole(AppRoles.Employee);
            if (isEmployee)
            {
                return await _dbContext.ProjectEmployees.AnyAsync(pe => pe.ProjectId == projectId && pe.EmployeeId == currentUserId);
            }

            return false;
        }

        private async Task<bool> CheckProjectModifyAccessAsync(long projectId)
        {
            long? currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return false;
            }

            bool isChief = _currentUserService.IsInRole(AppRoles.Chief);
            if (isChief)
            {
                return true;
            }

            bool isManager = _currentUserService.IsInRole(AppRoles.Manager);
            if (isManager)
            {
                return await _dbContext.Projects.AnyAsync(p => p.Id == projectId && p.ProjectManagerId == currentUserId);
            }

            return false;
        }
    }
}