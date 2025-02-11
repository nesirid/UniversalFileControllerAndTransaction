using Domain.Entities;
using Service.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Interfaces
{
    public interface ITransactionService
    {
        Task ProcessFileAsync(string fileContent, string fileType);
        Task<List<TransactionDto>> GetTransactionDtosAsync(int pageNumber, int pageSize);
        Task<bool> DeleteTransactionAsync(int Id);
    }
}
