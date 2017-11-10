using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Page
    {
        private bool freshStart = false;
        public PageConfig(bool freshStart=false)
        {
            InitializeComponent();
            this.freshStart = freshStart;
            init();

        }

        private void init()
        {
            txtDestination.Text = ConfigHandler.DestinationBase;
            txtDomain.Text = ConfigHandler.Domain;
            txtUsername.Text = ConfigHandler.Username;
            txtPassword.Password = ConfigHandler.Password;

            //txtDatabase.Text = ConfigHandler.DatabasePath;
            // do not fill config password here...

            btnEnableStuff.IsChecked = ConfigHandler.EnableWeirdo;
            btnEnableStuff.Content = ConfigHandler.EnableWeirdo ? "YEP" : "NOPE";
        }

        private bool checkPassworld()
        {
            // TODO: implement hashing / salting stuff (overkill but still...)-->
            if (txtCurrentConfigPassword.Password == ConfigHandler.ConfigPassword)
            {
                return true;
            }
            else
            {
                return false;
            }

            //Store a password hash:
            //Util.PasswordHash hash = new Util.PasswordHash("password");
            //byte[] hashBytes = hash.ToArray();

            //Check password against a stored hash
            //byte[] hashBytes = 
            //Util.PasswordHash hash = new Util.PasswordHash(hashBytes);
            //if (!hash.Verify("newly entered password"))
            //{
            //    throw new System.UnauthorizedAccessException();
            //}
        }

        private void btnSave_OnClick(object sender, RoutedEventArgs e)
        {

            if (!checkPassworld())
            {
                MessageBox.Show("Fout wachtwoord.", "FOEI", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            ConfigHandler.DestinationBase = txtDestination.Text;

            ConfigHandler.DestinationBase = txtDestination.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDestination.Text;
            ConfigHandler.Domain = txtDomain.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDomain.Text;
            ConfigHandler.Username = txtUsername.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtUsername.Text;
            ConfigHandler.Password = txtPassword.Password;
            ConfigHandler.ConnectionString = txtConnectionString.Text;
            //ConfigHandler.DatabasePath = txtDatabase.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDatabase.Text;
            ConfigHandler.EnableWeirdo = (bool) btnEnableStuff.IsChecked;
            if (!String.IsNullOrEmpty(txtConfigPassword.Password))
            {
                ConfigHandler.ConfigPassword = txtConfigPassword.Password;
            }

            //// TODO: UserContext -->
            //if (!File.Exists(ConfigHandler.DatabasePath))
            //{
            //    CasesDB.CreateDatabase();
            //}

            if (freshStart)
            {
                // due to a bug in reading the config after freshly writing a new config we need to restart the application. Just exit with message here -->
                if (MessageBox.Show("Nieuwe config geschreven. De applicatie sluit nu om deze actief te maken...",
                        "Herstart nodig", MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown(0);
                }
            }
            else if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }

        }

        private void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigHandler.DestinationBase = string.Empty;
            ConfigHandler.Domain = string.Empty;
            ConfigHandler.Username = string.Empty;
            ConfigHandler.Password = string.Empty;
            init();
        }

        private void btnEnableStuff_OnChecked(object sender, RoutedEventArgs e)
        {
            btnEnableStuff.Content = "YEP";
        }

        private void btnEnableStuff_OnUnchecked(object sender, RoutedEventArgs e)
        {
            btnEnableStuff.Content = "NOPE";
        }

        private void btnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
