namespace RadustovTestTask.BLL.Services
{
    using Microsoft.Extensions.Configuration;
    using RadustovTestTask.BLL.Interfaces;
    using System.IO;

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            string configuredPath = configuration["FileStorage:UploadPath"];

            if (!string.IsNullOrEmpty(configuredPath))
            {
                if (Path.IsPathRooted(configuredPath))
                {
                    _uploadPath = configuredPath;
                }
                else
                {
                    string baseDirectory = Directory.GetCurrentDirectory();
                    _uploadPath = Path.GetFullPath(Path.Combine(baseDirectory, configuredPath));
                }
            }
            else
            {
                _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            }

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> SaveFileAsync(string fileName, Stream fileStream)
        {
            string filePath = Path.Combine(_uploadPath, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            return filePath;
        }

        public async Task<byte[]> ReadFileAsync(string fileName)
        {
            string filePath = Path.Combine(_uploadPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {fileName} not found");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public Task DeleteFileAsync(string fileName)
        {
            string filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public bool FileExists(string fileName)
        {
            string filePath = Path.Combine(_uploadPath, fileName);
            return File.Exists(filePath);
        }
    }
}