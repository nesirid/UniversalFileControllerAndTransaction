using Microsoft.AspNetCore.Http;
using Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Service.DTOs.FileProcessing;


namespace Service.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsPath;

        public FileService(IConfiguration configuration)
        {
            _uploadsPath = configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";

            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fayl movcud deyil");

            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension
                (file.FileName)}{fileExtension}";
            string filePath = Path.Combine(_uploadsPath, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return uniqueFileName;
        }

        public Task<bool> DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Fayl adi movcud deyil");

            string filePath = Path.Combine(_uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<List<FileDto>> GetAllFilesAsync(int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 100;
            if (pageSize > 500) pageSize = 500;

            var files = Directory.GetFiles(_uploadsPath)
                .Select(filePath => new FileDto
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    FileSize = new FileInfo(filePath).Length,
                    CreatedDate = File.GetCreationTime(filePath)
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(files);
        }

        public async Task<bool> DeleteAllFilesAsync()
        {
            try
            {
                var files = Directory.GetFiles(_uploadsPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

    }
}

