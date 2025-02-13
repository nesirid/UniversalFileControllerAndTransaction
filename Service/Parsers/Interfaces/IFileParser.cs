using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Parsers.Interfaces
{
    public interface IFileParser
    {
        IEnumerable<Transaction> Parse(string fileContent);
        bool Supports(string fileType);
    }
}
