using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public int TransactionID { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
    }
}
