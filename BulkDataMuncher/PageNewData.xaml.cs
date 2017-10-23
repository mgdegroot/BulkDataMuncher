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
    public partial class PageNewData : Page, IBasePage
    {
        public PageNewData()
        {
            InitializeComponent();

        }

        public CaseInfo Case { get; set; }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            Case = fetchAndValidateInput();
            if (Case != null)
            {
                bool caseDatabaseRowExists = CasesDB.Exists(Case.Number);
                bool caseDirectoryExists = Util.DirectoryExistst(Case.CaseDirectory);
                if (caseDirectoryExists || caseDatabaseRowExists)
                {
                    // Verify consistency -->
                    if (caseDirectoryExists && !caseDatabaseRowExists)
                    {
                        MessageBox.Show("Zaak bestaat op opslag maar niet in database. Waarschuw beheerder",
                            "Inconsistentie gedetecteerd", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    else if (caseDatabaseRowExists && !caseDirectoryExists)
                    {
                        MessageBox.Show("Zaak bestaat in database maar niet in opslag. Waarschuw beheerder",
                            "Inconsistentie gedetecteerd", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    else if (MessageBox.Show($"Zaak met nummer {Case.Number} bestaat al.\r\nToevoegen aan bestaande zaak?",
                            "Zaaknummer bestaat al",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        NavigationService.Navigate(new PageModifyExisting() {CaseNumber = Case.Number});
                    }
                }
                else
                {
                    PageSelectFiles pageSelectFiles = new PageSelectFiles(this.Case);
                    this.NavigationService.Navigate(pageSelectFiles);
                }
            }
        }

        string validate()
        {
            string retVal = string.Empty;

            if (dpCase.SelectedDate == null)
            {
                retVal += "DPCASEDATE" + "|";
            }
            if (string.IsNullOrEmpty(txtZaaknummer.Text))
            {
                retVal += "TXTZAAKNUMMER" + "|";
            }
            if (string.IsNullOrEmpty(txtZaaknaam.Text))
            {
                retVal += "TXTZAAKNAAM" + "|";
            }
            if (string.IsNullOrEmpty(txtZaakEigenaar.Text))
            {
                retVal += "TXTZAAKEIGENAAR" + "|";
            }

            return retVal;
        }

        CaseInfo fetchAndValidateInput()
        {
            CaseInfo result = null;
            string validateResult = validate();

            if (validateResult != string.Empty)
            {
                string[] errors = validateResult.Split('|');
                string errMessage = string.Empty;
                foreach (var error in errors)
                {
                    switch (error)
                    {
                        case "":
                            break;
                        case "DPCASEDATE":
                            errMessage += "Kies een datum\r\n";
                            break;
                        case "TXTZAAKNUMMER":
                            errMessage += "Vul zaaknummer in\r\n";
                            break;
                        case "TXTZAAKNAAM":
                            errMessage += "Vul zaaknaam in\r\n";
                            break;
                        case "TXTZAAKEIGENAAR":
                            errMessage += "Vul zaak eigenaar in\r\n";
                            break;
                    }
                }


                MessageBox.Show(errMessage, "Corrigeer Fout(en)", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(txtZaaknaam);
        }
    }
}
