using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataMuncher
{
    public class CaseInfo
    {
        public CaseInfo()
        {
            Files = new ObservableCollection<FileSelection>();
        }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Owner { get; set; }
        public DateTime Date { get; set; }
        public ObservableCollection<FileSelection> Files { get; set; }
    }
}
