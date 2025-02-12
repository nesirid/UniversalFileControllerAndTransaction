using Microsoft.AspNetCore.Http;
using Service.DTOs.FileProcessing;

namespace Service.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
        Task<List<FileDto>> GetAllFilesAsync(int pageNumber, int pageSize);
        Task<bool> DeleteFileAsync(string fileName);
        Task<bool> DeleteAllFilesAsync();



    }
}