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
            //if (Case == null)
            //{
            //    if (MessageBox.Show("Zaak details ophalen?", "Ophalen gegevens", MessageBoxButton.YesNo,
            //            MessageBoxImage.Question) == MessageBoxResult.Yes)
            //    {
            //        fetchCase();
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

            bool caseDatabaseRowExists = CasesDB.Exists(Case.Number);
            bool caseDirectoryExists = Util.DirectoryExistst(Case.CaseDirectory);

            if (caseDirectoryExists && caseDatabaseRowExists)
            {
                Case = CasesDB.GetCase(Case.Number);
                PageSelectFiles pageSelectFiles = new PageSelectFiles(Case);
                this.NavigationService.Navigate(pageSelectFiles);
            }
            else if (caseDirectoryExists && !caseDatabaseRowExists)
            {
                MessageBox.Show("Zaak bestaat op opslag maar niet in database. Waarschuw beheerder",
                    "Inconsistentie gedetecteerd", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else if (caseDatabaseRowExists && !caseDirectoryExists)
            {
                MessageBox.Show("Zaak bestaat in database maar niet in opslag. Waarschuw beheerder",
                    "Inconsistentie gedetecteerd", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                if (MessageBox.Show("Zaak niet gevonden. Nieuwe zaak toevoegen in plaats van bestaande wijzigen?",
                        "Zaak niet gevonden", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    NavigationService.Navigate(new PageNewData());
                }
            }

        }

        private void btnFetchCase_OnClick(object sender, RoutedEventArgs e)
        {
            fetchCase();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(txtZaaknummer);
        }

        private bool fetchCase()
        {
            bool result = false;

            txtZaaknaam.Text = string.Empty;
            txtZaakEigenaar.Text = string.Empty;
            dpCase.DisplayDate = DateTime.MinValue;
            dpCase.Text = DateTime.MinValue.ToString();


            if (CasesDB.Exists(txtZaaknummer.Text))
            {
                Case = CasesDB.GetCase(txtZaaknummer.Text);

                txtZaaknaam.Text = Case.Name;
                txtZaakEigenaar.Text = Case.Owner;
                dpCase.DisplayDate = Case.Date;
                dpCase.Text = Case.Date.ToString();

                btnNext.IsEnabled = true;
                result = true;
            }
            else
            {
                Case = null;
                btnNext.IsEnabled = false;
                result = false;
                MessageBox.Show($"Zaak {txtZaaknummer.Text} niet gevonden", "Niet gevonden", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                
            }
            return result;
        }
    }
}
