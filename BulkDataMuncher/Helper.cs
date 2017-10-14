using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataMuncher
{
    public class Helper
    {
    }

    public enum FileSelectionType
    {
        FILE = 0,
        DIRECTORY = 1,
    }
    public class FileSelection
    {

        public FileSelectionType Type { get; set; }
        public string Path { get; set; }
    }
}
