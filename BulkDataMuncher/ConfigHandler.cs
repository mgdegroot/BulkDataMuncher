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
        public const string KEY_ENABLE_WEIRDO = "ENABLE_WEIRDO";
        public const string KEY_DOMAIN = "DOMAIN";
        public const string KEY_USERNAME = "USERNAME";
        public const string KEY_PASSWORD = "PASSWORD";
        public const string KEY_DESTINATIONBASE = "DESTINATIONBASE";
        public const string KEY_DB_PATH = "DB_PATH";
        public const string VALUE_DEFAULT = "";
        public const string KEY_CONFIG_PASSWORD = "CONFIG_PASSWORD";
        public const string KEY_CONNECTIONSTRING = "CONNECTIONSTRING";

        public static string Username
        {
            get => ConfigurationManager.AppSettings[KEY_USERNAME] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_USERNAME, value);
        }

        public static bool UsernameSet => String.IsNullOrEmpty(Username) == false;

        public static string Password
        {
            get => ConfigurationManager.AppSettings[KEY_PASSWORD] ?? string.Empty;
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

        public static string DatabasePath
        {
            get
            {
                //string test = ConfigurationManager.AppSettings[KEY_DB_PATH];
                return ConfigurationManager.AppSettings[KEY_DB_PATH] ?? VALUE_DEFAULT;
            }
            set => writeSetting(KEY_DB_PATH, value);
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings[KEY_CONNECTIONSTRING] ?? VALUE_DEFAULT;
            }
            set => writeSetting(KEY_CONNECTIONSTRING, value);
        }

        public static string ConfigPassword
        {
            get => ConfigurationManager.AppSettings[KEY_CONFIG_PASSWORD] ?? VALUE_DEFAULT;
            set => writeSetting(KEY_CONFIG_PASSWORD, value);
        }

        public static bool EnableWeirdo
        {
            get => Convert.ToBoolean(ConfigurationManager.AppSettings[KEY_ENABLE_WEIRDO] ?? "True");
            set => writeSetting(KEY_ENABLE_WEIRDO, Convert.ToString(value));
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
