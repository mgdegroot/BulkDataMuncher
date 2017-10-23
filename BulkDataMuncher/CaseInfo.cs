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
        public CaseInfo(bool isNew=true)
        {
            Files = new ObservableCollection<Util.FileSelection>();
            IsNew = isNew;
        }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Owner { get; set; }
        public DateTime Date { get; set; }
        public ObservableCollection<Util.FileSelection> Files { get; set; }
        public string CaseDirectory => Path.Combine(ConfigHandler.DestinationBase, this.Number);
        public bool IsNew { get; set; }
        public bool OverwriteExistingFiles { get; set; }
    }
}
