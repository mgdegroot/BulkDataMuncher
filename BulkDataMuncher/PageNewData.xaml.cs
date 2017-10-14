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
    /// Interaction logic for PageNewData.xaml
    /// </summary>
    public partial class PageNewData : Page
    {
        public PageNewData()
        {
            InitializeComponent();

        }

        public CaseInfo Case { get; set; }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            Case = fetchInfo();
            if (Case != null)
            {
                PageSelectFiles pageSelectFiles = new PageSelectFiles(this.Case);
                this.NavigationService.Navigate(pageSelectFiles);
            }
        }

        string validate()
        {
            string retVal = string.Empty;

            if (dpCase.SelectedDate == null)
            {
                retVal = "DPCASEDATE";
            }

            return retVal;
        }

        CaseInfo fetchInfo()
        {
            CaseInfo result = null;
            string validateResult = validate();

            if (validateResult != string.Empty)
            {
                string errMessage = string.Empty;

                switch (validateResult)
                {
                    case "":
                        break;
                    case "DPCASEDATE":
                        errMessage = "Kies een datum";
                        break;
                }

                MessageBox.Show(errMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                result = new CaseInfo();
                result.Name = txtZaaknaam.Text;
                result.Number = txtZaaknummer.Text;
                result.Owner = txtZaakEigenaar.Text;
                result.Date = dpCase.SelectedDate.Value;
            }
            return result;
        }
    }
}
