using Microsoft.AspNetCore.Mvc;
using Repository.Data;
using Service.Services.Interfaces;
using System.Text;

namespace App.Controllers
{
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IFileTypeRecognizer _fileTypeRecognizer;
        private readonly IFileService _fileService;
        private readonly AppDbContext _dbContext;
        private readonly IEnumerable<IFileParser> _fileParsers;


        public TransactionsController(ITransactionService transactionService,
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

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int pageNumber = 1,
                                                         [FromQuery] int pageSize = 10)
        {
            var transactions = await _transactionService.GetTransactionDtosAsync(pageNumber, pageSize);
            return Ok(transactions);
        }

        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> DeleteFile(int Id)
        {
            var result = await _transactionService.DeleteTransactionAsync(Id);
            if (!result) return NotFound(new { Message = "Tranzaksiya tapilmadi" });
            return Ok(new { Message = "Transaksiya ugurla silindi" });
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllTransactions()
        {
            var result = await _transactionService.DeleteAllTransactionsAsync();
            if (!result) return StatusCode(500, "Xeta bas verdi");

            return Ok(new { Message = "Butun tranzaksiyalar silindi" });
        }
    }
}
