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

        private void checkConfigPresent()
        {
            if (String.IsNullOrEmpty(ConfigHandler.DestinationBase))
            {
                if (MessageBox.Show("Configuratie niet aanwezig. Invullen voor gebruik", "Configuratie niet gevonden",
                        MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new PageConfig());
                }

            }
           }

        private void btnNew_OnClick(object sender, RoutedEventArgs e)
        {
            PageNewData pageNewData = new PageNewData();
            this.NavigationService.Navigate(pageNewData);
        }

        private void btnModify_OnClick(object sender, RoutedEventArgs e)
        {
            PageModifyExisting pageModifyExisting = new PageModifyExisting();
            this.NavigationService.Navigate(pageModifyExisting);
        }


        private void btnConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PageConfig pageConfig = new PageConfig();
            NavigationService.Navigate(pageConfig);
        }

        private void PageStart_OnLoaded(object sender, RoutedEventArgs e)
        {
            checkConfigPresent();
        }
    }
}
