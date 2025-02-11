using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Service.DTOs.Transactions;
using Service.Services.Interfaces;
using System.Text;
using System.Xml.Serialization;


namespace Service.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEnumerable<IFileParser> _fileParsers;
        private readonly IFileTypeRecognizer _fileTypeRecognizer;

        public TransactionService(AppDbContext dbContext,
                                  IEnumerable<IFileParser> fileParsers,
                                  IFileTypeRecognizer fileTypeRecognizer)
        {
            _dbContext = dbContext;
            _fileParsers = fileParsers;
            _fileTypeRecognizer = fileTypeRecognizer;
        }

        public async Task ProcessFileAsync(string fileContent, string fileName)
        {
            var fileType = _fileTypeRecognizer.RecognizeFileType(Encoding.UTF8.GetBytes(fileContent), fileName);

            var parser = _fileParsers.FirstOrDefault(p => p.Supports(fileType));
            if (parser == null)
                throw new NotSupportedException($"Bu tipli {fileType} fayl destek olunmur");

            var transactions = parser.Parse(fileContent)
                .Where(t => t != null && !string.IsNullOrWhiteSpace(t.AccountNumber))
                .ToList();

            if (!transactions.Any())
                throw new InvalidOperationException("Duzgun melumat tapilmadi");

            await _dbContext.Transactions.AddRangeAsync(transactions);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TransactionDto>> GetTransactionDtosAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var transactions = await _dbContext.Transactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionID = t.TransactionID,
                AccountNumber = t.AccountNumber,
                Amount = t.Amount,
                Currency = t.Currency,
                TransactionType = t.TransactionType,
                Date = t.Date
            }).ToList();
        }

        public async Task<bool> DeleteTransactionAsync(int Id)
        {
            var transaction = await _dbContext.Transactions.FindAsync(Id);
            if (transaction == null) return false;

            _dbContext.Transactions.Remove(transaction);
            await _dbContext.SaveChangesAsync();
            return true;
        }

    }
}
