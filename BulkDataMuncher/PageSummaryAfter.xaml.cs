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
    /// Interaction logic for PageSummaryAfter.xaml
    /// </summary>
    public partial class PageSummaryAfter : Page, IBasePage
    {
        public PageSummaryAfter(CaseInfo theCase)
        {
            InitializeComponent();
            this.Case = theCase;

            populateFields();
        }

        public CaseInfo Case { get; set; }

        private void populateFields()
        {
            dgTransferedFiles.ItemsSource = Case.Files;

            txtZaaknaam.Text = Case.Name;
            txtZaaknummer.Text = Case.Number;
            txtEigenaar.Text = Case.Owner;
            txtDatum.Text = Case.Date.ToString("yyyy-MM-dd");
            txtOverwriteExisting.Text = Case.OverwriteExistingFiles ? "JA" : "NEE";

            txtDest.Text = Case.CaseDirectory;
        }

        private void btnOk_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageStart());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // State has changed, do not allow to go back to transfer page -->
            var entry = NavigationService.RemoveBackEntry();
            while (entry != null)
            {
                entry = NavigationService.RemoveBackEntry();
            }
        }
    }
}
