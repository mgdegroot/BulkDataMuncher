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
using System.Windows.Threading;
using Path = System.IO.Path;

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageTransfer.xaml
    /// </summary>
    public partial class PageTransfer : Page, IBasePage
    {
        // actual filetransfer is done in background -->
        private BackgroundWorker bgWorker = new BackgroundWorker();

        // seperate thread for progress notification -->
        DispatcherTimer timer = new DispatcherTimer();

        public PageTransfer(CaseInfo theCase)
        {
            InitializeComponent();
            Case = theCase;

            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += doTransfer;
            bgWorker.ProgressChanged += transfer_ProgressChanged;
            bgWorker.RunWorkerCompleted += transfer_Completed;
            
            // update progressbar thread -->
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            pbProgress.Value = Util.ProgressInfo.PercentageDone;
        }


        public CaseInfo Case { get; set; }


        private void bntCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Annuleer kopieeren. Zeker weten?", "Annuleer kopie", MessageBoxButton.OKCancel,
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
            Util.ProgressInfo.StepSize = 100 / Case.Files.Count;

            if (ConfigHandler.EnableWeirdo)
            {
                backgroundMusicElement.Play();
            }
            else
            {
                backgroundMusicElement.Stop();
            }
        }


        private void doTransfer(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            CaseInfo workerCase = (CaseInfo) e.Argument;

            Util.CreateDirectory(workerCase.CaseDirectory);

            Util.ProgressInfo pi = new Util.ProgressInfo();
            List<Util.FileSelection> transferList = new List<Util.FileSelection>();

            // DEBUG
            //System.Threading.Thread.Sleep(60000);


            foreach (Util.FileSelection fileSelection in workerCase.Files)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                pi.CurrentFileSrc = fileSelection.Path;
                pi.CurrentFileDst = Case.CaseDirectory;
                
                worker.ReportProgress(Util.ProgressInfo.PercentageDone, pi);
                bool copyResult = false;
                switch (fileSelection.Type)
                {
                    case Util.FileSelectionType.DIRECTORY:
                        List<Util.FileSelection>fileList = Util.DirectoryCopy(fileSelection.Path, Case.CaseDirectory, recursive: true, overwrite: Case.OverwriteExistingFiles);
                        lock (pi.TransferredFiles)
                        {
                            pi.TransferredFiles = fileList;
                            
                        }
                        transferList.AddRange(fileList);
                        break;
                    case Util.FileSelectionType.FILE:
                        Util.FileSelection file = Util.FileCopy(fileSelection.Path, Case.CaseDirectory, overwrite: Case.OverwriteExistingFiles);

                        lock (pi.TransferredFiles)
                        {
                            pi.TransferredFiles = new List<Util.FileSelection>(1);
                            pi.TransferredFiles.Add(file);
                        }
                        transferList.Add(file);
                        break;
                }
                fileSelection.State = Util.FileState.TRANSFERRED;
            }
            pi.TransferredFiles = transferList;
            e.Result = pi;
        }

        private void transfer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
            Util.ProgressInfo pi = (Util.ProgressInfo) e.UserState;
            lblSource.Content = pi.CurrentFileSrc;
            lblDest.Content = pi.CurrentFileDst;
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
                Util.ProgressInfo pi = (Util.ProgressInfo) e.Result;

                Case.Files.Clear();
                // TODO: AddRange for ObservableCollection -->
                foreach (var piTransferredFile in pi.TransferredFiles)
                {
                    Case.Files.Add(piTransferredFile);
                }

                CasesDB.AddTransferedFilesToCaseDB(pi.TransferredFiles, Case);
                Util.WriteCaseFileToOutput(this.Case);

                string failedFiles = pi.TransferredFiles.Where(
                    fileSel => fileSel.State == Util.FileState.ERROR || fileSel.State == Util.FileState.DUPLICATE
                    ).Aggregate(string.Empty, (current, fileSel) => current + (fileSel.Path + System.Environment.NewLine));


                

                if (string.IsNullOrEmpty(failedFiles))
                {
                    failedFiles = "Alle bestanden gekopieerd!";
                }

                if (MessageBox.Show(failedFiles, "Completed", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new PageSummaryAfter(this.Case));
                }
            }
        }
    }
}
