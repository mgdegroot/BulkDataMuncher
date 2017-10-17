using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BulkDataMuncher
{
    public interface IBasePage
    {

        CaseInfo Case { get; set; }
    }
}
