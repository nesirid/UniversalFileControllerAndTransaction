using Microsoft.AspNetCore.Mvc;
using Repository.Data;
using Service.Services;
using Service.Parsers;
using Service.Services.Interfaces;
using System.Text;

namespace App.Controllers
{
    public class FileController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IFileTypeRecognizer _fileTypeRecognizer;
        private readonly IFileService _fileService;
        private readonly AppDbContext _dbContext;
        private readonly IEnumerable<IFileParser> _fileParsers;

        public FileController(ITransactionService transactionService,
                              IFileTypeRecognizer fileTypeRecognizer,
                              IFileService fileService,
                              AppDbContext dbContext,
                              IEnumerable<IFileParser> fileParsers)
        {
            _transactionService = transactionService;
            _fileTypeRecognizer = fileTypeRecognizer;
            _fileService = fileService;
            _dbContext = dbContext;
            _fileParsers = fileParsers;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles(IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return BadRequest("Fayl yoxdur");
            if (files.Length > 5)
                return BadRequest("Eyni anda en cox 5 fayl yuklemek olar");

            var uploadedFiles = new List<object>();
            foreach (var file in files)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    var fileType = _fileTypeRecognizer.RecognizeFileType(fileBytes, file.FileName);
                    Console.WriteLine($"Tanınmış fayl növü {fileType} adi {file.FileName}");

                    if (fileType == "unknown")
                    {
                        uploadedFiles.Add(new
                        {
                            OriginalFileName = file.FileName,
                            Message = "Fayl formati desteklenmir"
                        });
                        continue;
                    }

                    var savedFileName = await _fileService.SaveFileAsync(file);
                    var parser = _fileParsers.FirstOrDefault(p => p.Supports(fileType));
                    Console.WriteLine($"{fileType} üçün təhlilçi axtarır");

                    if (parser == null)
                    {
                        uploadedFiles.Add(new
                        {
                            OriginalFileName = file.FileName,
                            SavedFileName = savedFileName,
                            FileType = fileType,
                            Message = "Fayl ugurla elave olundu, amma icindeki melumatlar parslanmir"
                        });
                        continue;
                    }

                    var fileContent = Encoding.UTF8.GetString(fileBytes);
                    var transactions = parser.Parse(fileContent);

                    if (transactions.Any())
                    {
                        await _dbContext.Transactions.AddRangeAsync(transactions);
                        await _dbContext.SaveChangesAsync();
                    }

                    uploadedFiles.Add(new
                    {
                        OriginalFileName = file.FileName,
                        SavedFileName = savedFileName,
                        FileType = fileType,
                        Message = "Fayl ugurla elave olundu ve melumatlar bazaya daxil edildi"
                    });
                }
                catch (Exception ex)
                {
                    uploadedFiles.Add(new
                    {
                        OriginalFileName = file.FileName,
                        Message = $"{ex.Message}"
                    });
                }
            }
            return Ok(uploadedFiles);
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetAllFiles([FromQuery] int pageNumber = 1,
                                                     [FromQuery] int pageSize = 100)
        {
            var files = await _fileService.GetAllFilesAsync(pageNumber, pageSize);
            return Ok(files);
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var result = await _fileService.DeleteFileAsync(fileName);

            if (!result)
            return NotFound(new { Message = "Fayl tapilmadi" });
            return Ok(new { Message = "Fayl ugurla silindi" });
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            var result = await _fileService.DeleteAllFilesAsync();
            if (result)
                return Ok(new { Message = "Bütün fayllar uğurla silindi" });

            return StatusCode(500, new { Message = "Faylları silmək mümkün olmadı" });
        }
    }
}
