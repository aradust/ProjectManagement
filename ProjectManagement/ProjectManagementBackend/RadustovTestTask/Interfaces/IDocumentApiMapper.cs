namespace RadustovTestTask.API.Interfaces
{
    using RadustovTestTask.BLL.DTO;

    public interface IDocumentApiMapper
    {
        DocumentDto ToDto(string fileName, string originalName, long size, long projectId);
    }
}