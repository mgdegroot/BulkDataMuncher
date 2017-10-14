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

        public CaseInfo Case { get; set; }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            PageSelectFiles pageSelectFiles = new PageSelectFiles(Case);
            this.NavigationService.Navigate(pageSelectFiles);
        }
    }
}
