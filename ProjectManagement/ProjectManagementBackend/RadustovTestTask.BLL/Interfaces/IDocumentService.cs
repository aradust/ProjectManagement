namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetProjectDocumentsAsync(long projectId);
        Task<DocumentDto> AddDocumentAsync(long projectId, string originalFileName, Stream fileStream);
        Task<bool> DeleteDocumentAsync(long documentId);
        Task<(byte[] FileContent, string ContentType, string FileName)> DownloadDocumentAsync(long documentId);
    }
}