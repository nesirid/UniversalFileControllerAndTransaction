using Domain.Entities;
using Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service.Parsers
{
    public class XmlFileParser : IFileParser
    {
        public IEnumerable<Transaction> Parse(string fileContent)
        {
            var transactions = new List<Transaction>();

            var document = XDocument.Parse(fileContent);

            var transactionElements = document.Descendants("Transaction");

            foreach (var element in transactionElements)
            {
                transactions.Add(new Transaction
                {
                    TransactionID = int.Parse(element.Element("TransactionID")?.Value ?? "0"),
                    AccountNumber = element.Element("AccountNumber")?.Value,
                    Amount = decimal.Parse(element.Element("Amount")?.Value ?? "0"),
                    Currency = element.Element("Currency")?.Value,
                    TransactionType = element.Element("TransactionType")?.Value,
                    Date = DateTime.Parse(element.Element("Date")?.Value ?? DateTime.MinValue.ToString())
                });
            }

            return transactions;
        }

        public bool Supports(string fileType) => fileType.Equals("xml", StringComparison.OrdinalIgnoreCase);
    }
}
