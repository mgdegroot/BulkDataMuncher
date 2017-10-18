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
    /// Interaction logic for PageModifyExisting.xaml
    /// </summary>
    public partial class PageModifyExisting : Page
    {
        public PageModifyExisting()
        {
            InitializeComponent();
        }

        public string CaseNumber
        {
            get { return Case.Number; }
            set
            {
                if (Case == null)
                {
                    Case = new CaseInfo();
                }
                Case.Number = value;
                txtZaaknummer.Text = Case.Number;
            }
            
        }

        public CaseInfo Case { get; set; }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            Case.Number = txtZaaknummer.Text;
            
            // TODO: search case in database and fill

            PageSelectFiles pageSelectFiles = new PageSelectFiles(Case);
            this.NavigationService.Navigate(pageSelectFiles);
        }
    }
}
