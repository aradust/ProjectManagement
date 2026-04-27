namespace RadustovTestTask.BLL.Interfaces
{
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.DAL.Entities;

    public interface IDocumentMapper
    {
        DocumentDto ToDto(Document entity);
        Document ToEntity(long projectId, string fileName, string originalName, long size);
    }
}