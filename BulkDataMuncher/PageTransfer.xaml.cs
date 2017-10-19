using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageTransfer.xaml
    /// </summary>
    public partial class PageTransfer : Page, IBasePage
    {
        private BackgroundWorker bgWorker = new BackgroundWorker();

        public PageTransfer(CaseInfo theCase)
        {
            InitializeComponent();
            Case = theCase;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += doTransfer;
            bgWorker.ProgressChanged += transfer_ProgressChanged;
            bgWorker.RunWorkerCompleted += transfer_Completed;
        }

        public CaseInfo Case { get; set; }

        private void bntCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Annulleer kopie", "Annuleer kopie", MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                if (bgWorker.WorkerSupportsCancellation)
                {
                    bgWorker.CancelAsync();
                }
            }
        }

        private void PageTransfer_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Add case to database -->
            if (Case.IsNew)
            {
                CasesDB.AddCase(Case);
            }
            else
            {
                CasesDB.ModifyCase(Case);
            }

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(this.Case);
            }
        }


        private void doTransfer(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            CaseInfo workerCase = (CaseInfo) e.Argument;

            var progressTick = 100 / workerCase.Files.Count;
            var currCnt = 0;

            Util.CreateDirectory(workerCase.CaseDirectory);

            
            foreach (FileSelection fileSelection in workerCase.Files)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                //lblSource.Content = Path.GetFileName(fileSelection.Path);
                //lblDest.Content = Path.Combine(Case.CaseDirectory, fileSelection.Path);

                switch (fileSelection.Type)
                {
                    case FileSelectionType.DIRECTORY:
                        Util.DirectoryCopy(fileSelection.Path, Case.CaseDirectory, recursive: true, overwrite: false);
                        break;
                    case FileSelectionType.FILE:
                        Util.FileCopy(fileSelection.Path, Case.CaseDirectory, overwrite: false);
                        break;
                }
                currCnt++;
                worker.ReportProgress(progressTick * currCnt);
                //System.Threading.Thread.Sleep(2000);
            }
        }

        private void transfer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
        }

        private void transfer_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Cancelled", "Cancelled", MessageBoxButton.OK);
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error", e.Error.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (MessageBox.Show("Completed", "Completed", MessageBoxButton.OK, MessageBoxImage.Information) ==
                    MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new PageStart());
                }
            }
        }
    }
}
