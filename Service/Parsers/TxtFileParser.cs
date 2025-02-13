using Domain.Entities;
using Service.Parsers.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Service.Parsers
{
    public class TxtFileParser : IFileParser
    {
        public IEnumerable<Transaction> Parse(string fileContent)
        {
            var transactions = new List<Transaction>();
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2) return transactions;

            var delimiter = "|";
            foreach (var line in lines.Skip(1))
            {
                var columns = line.Split(new[] { delimiter }, StringSplitOptions.TrimEntries);

                if (columns.Length != 6) continue;

                if (int.TryParse(columns[0], out int transactionId) &&
                    decimal.TryParse(columns[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount) &&
                    DateTime.TryParse(columns[5], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    transactions.Add(new Transaction
                    {
                        TransactionID = transactionId,
                        AccountNumber = columns[1],
                        Amount = amount,
                        Currency = columns[3],
                        TransactionType = columns[4],
                        Date = date
                    });
                }
            }

            return transactions;
        }

        public bool Supports(string fileType) => fileType.Equals("txt", StringComparison.OrdinalIgnoreCase);
    }
}
