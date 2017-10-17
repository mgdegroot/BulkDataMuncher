using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for PageSummaryBefore.xaml
    /// </summary>
    public partial class PageSummaryBefore : Page, IBasePage
    {
        public PageSummaryBefore(CaseInfo theCase)
        {
            InitializeComponent();
            this.Case = theCase;

            populateFields();
        }

        public CaseInfo Case { get; set; }

        private void populateFields()
        {
            dgSelectedFiles.ItemsSource = Case.Files;

            txtZaaknaam.Text = Case.Name;
            txtZaaknummer.Text = Case.Number;
            txtEigenaar.Text = Case.Owner;
            txtDatum.Text = Case.Date.ToString("yyyy-MM-dd");

            //string destDir =
//            string destDir = System.IO.Path.Combine(ConfigHandler.DestinationBase, Case.Number);
            txtDest.Text = Case.CaseDirectory;

            txtZaaknaam.IsEnabled = false;
            txtZaaknummer.IsEnabled = false;
            txtEigenaar.IsEnabled = false;
            txtDatum.IsEnabled = false;


        }
    }
}
