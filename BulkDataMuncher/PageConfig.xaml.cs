﻿using System;
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
        public PageConfig()
        {
            InitializeComponent();
            init();

        }

        private void init()
        {
            txtDestination.Text = ConfigHandler.DestinationBase;
            txtDomain.Text = ConfigHandler.Domain;
            txtUsername.Text = ConfigHandler.Username;
            txtPassword.Password = ConfigHandler.Password;
            txtDatabase.Text = ConfigHandler.DatabasePath;
            btnEnableStuff.IsChecked = ConfigHandler.EnableWeirdo;
            btnEnableStuff.Content = ConfigHandler.EnableWeirdo ? "YEP" : "NOPE";
        }

        private void btnSave_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigHandler.DestinationBase = txtDestination.Text;

            ConfigHandler.DestinationBase = txtDestination.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDestination.Text;
            ConfigHandler.Domain = txtDomain.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDomain.Text;
            ConfigHandler.Username = txtUsername.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtUsername.Text;
            ConfigHandler.Password = txtPassword.Password;
            ConfigHandler.DatabasePath = txtDatabase.Text == ConfigHandler.VALUE_DEFAULT ? String.Empty : txtDatabase.Text;
            ConfigHandler.EnableWeirdo = (bool) btnEnableStuff.IsChecked;

            // TODO: UserContext -->
            if (!File.Exists(ConfigHandler.DatabasePath))
            {
                CasesDB.CreateDatabase();
            }

            if (NavigationService.CanGoBack)
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
