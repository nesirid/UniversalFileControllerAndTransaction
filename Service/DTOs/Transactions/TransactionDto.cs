using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs.Transactions
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int TransactionID { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
    }
}
