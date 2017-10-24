using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
            SQLiteDataAdapter adapter = CasesDB.GetDataAdapterCases();
            DataTable dt = new DataTable("datamuncher_cases");
            adapter.Fill(dt);
            dgCases.ItemsSource = dt.DefaultView;

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
            string caseNumber = "";
            SQLiteDataAdapter adapter = CasesDB.GetDataAdapterCaseContent(caseNumber);

            DataTable dt = new DataTable("datamuncher_cases");
            adapter.Fill(dt);
            dgContentCase.ItemsSource = dt.DefaultView;

            MessageBox.Show("Double", "Click", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
    }
}
