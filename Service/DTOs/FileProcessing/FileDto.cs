using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs.FileProcessing
{
    public class FileDto
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
