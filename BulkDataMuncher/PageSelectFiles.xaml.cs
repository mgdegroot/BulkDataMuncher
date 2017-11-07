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
using FolderSelect;
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
            // NATIVE -->
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.ShowNewFolderButton = false;

            //if (fbd.ShowDialog() == DialogResult.OK)
            //{
            //    Case.Files.Add(new Util.FileSelection()
            //    {
            //        Type=Util.FileSelectionType.DIRECTORY,
            //        Path=fbd.SelectedPath,
            //    });
            //}
            // <-- END NATIVE

            // EX -->
            //var dlg1 = new FolderBrowserDialogEx();
            //dlg1.Description = "Kies een directory:";
            //dlg1.ShowNewFolderButton = true;
            //dlg1.ShowEditBox = true;
            ////dlg1.ShowBothFilesAndFolders = true;
            ////dlg1.NewStyle = false;
            //dlg1.ShowFullPathInEditBox = true;
            //dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;

            //DialogResult result = dlg1.ShowDialog();
            //if (result == DialogResult.OK)
            //{
            //    Case.Files.Add(new Util.FileSelection()
            //        {
            //            Type=Util.FileSelectionType.DIRECTORY,
            //            Path= dlg1.SelectedPath,
            //        });
            //}
            // <--END EX

            var fsd = new FolderSelectDialog();
            fsd.Title = "Kies directory";
            //fsd.InitialDirectory = @"c:\";
            
            if (fsd.ShowDialog(IntPtr.Zero))
            {
                Case.Files.Add(new Util.FileSelection()
                {
                    Type = Util.FileSelectionType.DIRECTORY,
                    Path = fsd.FileName,
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

            rectOverwriteExisting.Fill = (ImageBrush) r;
            tbOverwrite.Text = "Wel\r\nOverschrijven";
        }

        private void btnOverwriteExisting_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var r = this.FindResource("ImgNoOverwrite");
            Case.OverwriteExistingFiles = false;
            rectOverwriteExisting.Fill= (ImageBrush) r;

            tbOverwrite.Text = "Niet\r\nOverschrijven";
        }
    }
}
