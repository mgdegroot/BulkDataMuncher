using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageTransfer.xaml
    /// </summary>
    public partial class PageTransfer : Page, IBasePage
    {
        public PageTransfer(CaseInfo theCase)
        {
            InitializeComponent();
            Case = theCase;
            doTransfer();
        }

        public CaseInfo Case { get; set; }

        private void bntCancel_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void doTransfer()
        {
            var fileCnt = Case.Files.Count;

            foreach (FileSelection fileSelection in Case.Files)
            {
                switch (fileSelection.Type)
                {
                    case FileSelectionType.DIRECTORY:
                        // copy dir
                        
                        if (ConfigHandler.UsernameSet)
                        {
                            Util.DirectoryCopyAsUser(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, fileSelection.Path, "", recursive: true, overwrite: false);
                        }
                        else
                        {
                            Util.DirectoryCopy(fileSelection.Path, "", recursive: true, overwrite: false);
                        }
                        
                        break;
                    case FileSelectionType.FILE:
                        // copy file
                        if (ConfigHandler.UsernameSet)
                        {
                            Util.FileCopyAsUser(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, fileSelection.Path, "", overwrite: false);
                        }
                        else
                        {
                            Util.FileCopy(fileSelection.Path, "", overwrite: false);
                        }
                        break;
                }
            }
        }
    }
}
