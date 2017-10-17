using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.ConstrainedExecution;

namespace BulkDataMuncher
{
    public static class ConfigHandler
    {
        public const string KEY_DOMAIN = "DOMAIN";
        public const string KEY_USERNAME = "USERNAME";
        public const string KEY_PASSWORD = "PASSWORD";
        public const string KEY_DESTINATIONBASE = "DESTINATIONBASE";
        public const string VALUE_DEFAULT = "<NOT FOUND>";
        
        public static string Username
        {
            get => ConfigurationManager.AppSettings[KEY_USERNAME] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_USERNAME, value);
        }

        public static bool UsernameSet => String.IsNullOrEmpty(Username) == false;

        public static string Password
        {
            get => ConfigurationManager.AppSettings[KEY_PASSWORD] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_PASSWORD, value);
        }

        public static bool PasswordSet => String.IsNullOrEmpty(Password) == false;

        public static string DestinationBase
        {
            get => ConfigurationManager.AppSettings[KEY_DESTINATIONBASE] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_DESTINATIONBASE, value);
        }

        public static string Domain
        {
            get => ConfigurationManager.AppSettings[KEY_DOMAIN] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_DOMAIN, value);
        }

        private static bool writeSetting(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                return true;
            }
            catch (ConfigurationErrorsException cee)
            {
                return false;
            }
        }

        public static bool encryptConfig(string sectionKey)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionKey);
            if (section != null)
            {
                if (!section.SectionInformation.IsProtected)
                {
                    if (!section.ElementInformation.IsLocked)
                    {
                        section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        section.SectionInformation.ForceSave = true;
                        config.Save(ConfigurationSaveMode.Full);
                    }
                }
            }
            return true;
        }
    }


}
