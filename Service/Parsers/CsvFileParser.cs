using Domain.Entities;
using Service.Parsers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Parsers
{
    public class CsvFileParser : IFileParser
    {
        public IEnumerable<Transaction> Parse(string fileContent)
        {
            var transactions = new List<Transaction>();
            var lines = fileContent.Split(Environment.NewLine);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');

                transactions.Add(new Transaction
                {
                    TransactionID = int.Parse(columns[0]),
                    AccountNumber = columns[1],
                    Amount = decimal.Parse(columns[2]),
                    Currency = columns[3],
                    TransactionType = columns[4],
                    Date = DateTime.Parse(columns[5])
                });
            }

            return transactions;
        }

        public bool Supports(string fileType)
        => fileType.Equals("csv", StringComparison.OrdinalIgnoreCase);
    }
}
