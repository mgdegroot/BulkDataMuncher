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
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageSelectFiles.xaml
    /// </summary>
    public partial class PageSelectFiles : Page, IBasePage
    {
        private ObservableCollection<Util.FileSelection> fileSelection = new ObservableCollection<Util.FileSelection>();


        public PageSelectFiles(CaseInfo theCase)
        {
            InitializeComponent();
            this.Case = theCase;
            //fileSelection = new ObservableCollection<FileSelection>(this.Case.Files);
            dgSelectedFiles.ItemsSource = this.Case.Files;
        }

        public CaseInfo Case { get; set; }

        private void btnAddFiles_OnClick(object sender, RoutedEventArgs e)
        {
            handleChooseFiles();
        }

        private void btnAddDirectories_OnClick(object sender, RoutedEventArgs e)
        {
            handleChooseDirectories();

        }

        private void handleChooseFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = true,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var filename in ofd.FileNames)
                {
                    Case.Files.Add(new Util.FileSelection()
                    {
                        Type=Util.FileSelectionType.FILE,
                        Path=filename,
                    });
                }
            }
        }

        private void handleChooseDirectories()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Case.Files.Add(new Util.FileSelection()
                {
                    Type=Util.FileSelectionType.DIRECTORY,
                    Path=fbd.SelectedPath,
                });
            }
        }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            if (Case.Files.Count == 0)
            {
                MessageBox.Show("Geen bestanden of directories geselecteerd", "Geen data", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            else
            {
                PageSummaryBefore pageSummaryBefore = new PageSummaryBefore(Case);
                this.NavigationService.Navigate(pageSummaryBefore);
            }
        }

        private void btnOverwriteExisting_OnChecked(object sender, RoutedEventArgs e)
        {
            var r = this.FindResource("ImgOverwrite");
            Case.OverwriteExistingFiles = true;
            btnOverwriteExisting.Background = (ImageBrush) r;
            //tbOverwriteText.Text = "Wel\r\nOverschrijven";
            //btnOverwriteExisting.Content = "Wel overschrijven";

        }

        private void btnOverwriteExisting_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var r = this.FindResource("ImgNoOverwrite");
            Case.OverwriteExistingFiles = false;
            btnOverwriteExisting.Background = (ImageBrush) r;

            //tbOverwriteText.Text = "Niet\r\nOverschrijven";
            //btnOverwriteExisting.Content = "Niet overschrijven";
        }
    }
}
