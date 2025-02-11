using Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FileTypeRecognizer : IFileTypeRecognizer
    {
        public string RecognizeFileType(byte[] fileContent, string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower().Trim();

            return extension switch
            {
                ".xml" => "xml",
                ".csv" => "csv",
                ".txt" => "txt",
                _ => "unknown"
            };
        }
    }
}
