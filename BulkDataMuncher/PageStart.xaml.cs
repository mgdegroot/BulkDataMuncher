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
    /// Interaction logic for PageStart.xaml
    /// </summary>
    public partial class PageStart : Page
    {
        public PageStart()
        {
            InitializeComponent();
        }

        public CaseInfo Case { get; set; }

        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {
            PageNewData pageNewData = new PageNewData();
            this.NavigationService.Navigate(pageNewData);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("test", "test", MessageBoxButton.OK);
        }

        
    }
}
