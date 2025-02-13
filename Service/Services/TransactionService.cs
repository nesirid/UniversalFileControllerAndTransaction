using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Service.DTOs.Transactions;
using Service.Parsers.Interfaces;
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

            _dbContext.ChangeTracker.Clear();
        }

        public async Task<List<TransactionDto>> GetTransactionDtosAsync(int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            return await _dbContext.Transactions
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    TransactionID = t.TransactionID,
                    AccountNumber = t.AccountNumber,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    TransactionType = t.TransactionType,
                    Date = t.Date
                })
                .AsSplitQuery() 
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteTransactionAsync(int id)
        {
            var transaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (transaction == null) return false;

            transaction.IsDeleted = true;
            transaction.DeletedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                bool exists = await _dbContext.Transactions.AnyAsync(t => t.Id == id);
                if (!exists) return false;

                int affectedRows = await _dbContext.Transactions
                    .Where(t => t.Id == id)
                    .ExecuteDeleteAsync();
                return affectedRows > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"[ERROR] Concurrency issue while deleting: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error while deleting: {ex.Message}");
                return false;
            }
            finally
            {
                _dbContext.ChangeTracker.Clear();
            }
        }

        public async Task<bool> DeleteAllTransactionsAsync()
        {
            try
            {
                int affectedRows = await _dbContext.Transactions.ExecuteDeleteAsync();
                _dbContext.ChangeTracker.Clear();
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to delete transactions: {ex.Message}");
                return false;
            }
        }
    }
}
