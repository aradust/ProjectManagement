namespace RadustovTestTask.API.Mappers
{
    using RadustovTestTask.API.Interfaces;
    using RadustovTestTask.BLL.DTO;

    public class DocumentApiMapper : IDocumentApiMapper
    {
        public DocumentDto ToDto(string fileName, string originalName, long size, long projectId)
        {
            return new DocumentDto
            {
                FileName = fileName,
                OriginalName = originalName,
                Size = size,
                UploadedAt = DateTime.UtcNow
            };
        }
    }
}