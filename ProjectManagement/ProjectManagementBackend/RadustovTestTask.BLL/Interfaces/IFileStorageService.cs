namespace RadustovTestTask.BLL.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string fileName, Stream fileStream);
        Task<byte[]> ReadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
        bool FileExists(string fileName);
    }
}