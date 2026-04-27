namespace RadustovTestTask.BLL.Mappers
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;
    using RadustovTestTask.DAL.Entities;

    public class DocumentMapper : IDocumentMapper
    {
        public DocumentDto ToDto(Document entity)
        {
            return new DocumentDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                OriginalName = entity.OriginalName,
                Size = entity.Size,
                UploadedAt = entity.UploadedAt
            };
        }

        public Document ToEntity(long projectId, string fileName, string originalName, long size)
        {
            return new Document
            {
                ProjectId = projectId,
                FileName = fileName,
                OriginalName = originalName,
                Size = size,
                UploadedAt = DateTime.UtcNow
            };
        }
    }
}