using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
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
        public string CaseDirectory => Path.Combine(ConfigHandler.DestinationBase, this.Number);
    }
}
