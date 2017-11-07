using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BulkDataMuncher
{
    /// <summary>
    /// Interaction logic for PageReporting.xaml
    /// </summary>
    public partial class PageReporting : Page
    {
        public PageReporting()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SQLiteDataAdapter adapterCase = CasesDB.GetDataAdapterCases();
            DataTable dtCase = new DataTable("datamuncher_case");
            adapterCase.Fill(dtCase);
            dgCases.ItemsSource = dtCase.DefaultView;

            /*
        using (dt = new DataTable())
        {
            sqlda.Fill(dt);
            dataGridView1.DataSource = dt;
        }      
    */
        }

        private void dgCases_row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            
            string caseNumber = ((System.Data.DataRowView)row.Item)[0].ToString();
            SQLiteDataAdapter adapter = CasesDB.GetDataAdapterCaseContent(caseNumber);

            DataTable dt = new DataTable("datamuncher_case_content");
            adapter.Fill(dt);
            dgContentCase.ItemsSource = dt.DefaultView;

            
        }

        private void btnExportSelected_OnClick(object sender, RoutedEventArgs e)
        {

            var selection = dgCases.SelectedItem;
            string caseNumber = ((System.Data.DataRowView)dgCases.SelectedItem)[0].ToString();

            export(caseNumber);

        }

        private void export(string caseNumber = "")
        {
            SQLiteDataAdapter adapterCase = CasesDB.GetDataAdapterCases(caseNumber);


            DataTable dtCase = new DataTable("zaken");

            adapterCase.Fill(dtCase);
            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(dtCase);
            if (chkIncFilenames.IsChecked.HasValue && chkIncFilenames.IsChecked.Value)
            {
                foreach (DataRow row in dtCase.Rows)
                {
                    string tmpCase = row[0].ToString();
                    SQLiteDataAdapter adapterCaseContent = CasesDB.GetDataAdapterCaseContent(tmpCase);

                    DataTable dtCaseContent = new DataTable($"zaak_{tmpCase}");
                    adapterCaseContent.Fill(dtCaseContent);

                    workbook.Worksheets.Add(dtCaseContent);
                }
            }
            
            SaveFileDialog ofd = new SaveFileDialog()
            {
                RestoreDirectory = true,
                DefaultExt = "xlsx",
                Filter = "Excel Files Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                workbook.SaveAs(ofd.FileName);
            }

        }

        private void btnExportAll_OnClick(object sender, RoutedEventArgs e)
        {
            export();
        }
    }
}
