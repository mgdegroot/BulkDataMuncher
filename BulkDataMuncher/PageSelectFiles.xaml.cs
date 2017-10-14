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

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageSelectFiles.xaml
    /// </summary>
    public partial class PageSelectFiles : Page
    {
        private ObservableCollection<FileSelection> fileSelection = new ObservableCollection<FileSelection>();


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
                    Case.Files.Add(new FileSelection()
                    {
                        Type=FileSelectionType.FILE,
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
                Case.Files.Add(new FileSelection()
                {
                    Type=FileSelectionType.DIRECTORY,
                    Path=fbd.SelectedPath,
                });
            }
        }

        private void btnNext_OnClick(object sender, RoutedEventArgs e)
        {
            PageSummaryBefore pageSummaryBefore = new PageSummaryBefore(Case);
            this.NavigationService.Navigate(pageSummaryBefore);
        }
    }
}
