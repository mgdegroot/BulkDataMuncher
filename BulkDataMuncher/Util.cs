﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32.SafeHandles;

namespace BulkDataMuncher
{


    public class Util
    {
        public enum FileSelectionType
        {
            FILE = 0,
            DIRECTORY = 1,
        }

        public enum FileState
        {
            PENDING = 0,
            TRANSFERRED = 1,
            // TODO: more states

        }
        public class FileSelection
        {
            public FileSelection()
            {
                State = FileState.PENDING;
            }

            public FileSelectionType Type { get; set; }
            public FileState State { get; set; }
            public string Path { get; set; }
        }

        public static bool DirectoryExistst(string path)
        {
            if (ConfigHandler.UsernameSet)
            {
                return directoryExistsAsUser(path);
            }
            else
            {
                return directoryExists(path);
            }
        }


        public static bool CreateDirectory(string path)
        {
            bool result = false;
            if (ConfigHandler.UsernameSet)
            {
                result = createDirectoryAsUser(path);
            }
            else
            {
                result = createDirectory(path);
            }
            return result;
        }

        public static bool FileCopy(string srcFilename, string dstDirname, bool overwrite)
        {
            bool result = false;
            if (ConfigHandler.UsernameSet)
            {
                result = fileCopyAsUser(srcFilename, dstDirname, overwrite);
            }
            else
            {
                result = fileCopy(srcFilename, dstDirname, overwrite);
            }
            return result;
        }

        public static bool DirectoryCopy(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            bool result = false;
            if (ConfigHandler.UsernameSet)
            {
                result = directoryCopyAsUser(srcDir, dstDir, recursive, overwrite);
            }
            else
            {
                result = directoryCopy(srcDir, dstDir, recursive, overwrite);
            }
            return result;
        }

        private static bool directoryExistsAsUser(string path)
        {
            return ImpersonationHelper.ImpersonateWithBoolRet(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => directoryExists(path));
        }


        private static bool directoryExists(string path)
        {
            return Directory.Exists(path);
        }


        private static bool createDirectoryAsUser(string path)
        {
            return ImpersonationHelper.ImpersonateWithBoolRet(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () =>createDirectory(path));
        }

        private static bool createDirectory(string path)
        {
            bool result = true;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return result;
        }

        private static bool directoryCopyAsUser(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            return ImpersonationHelper.ImpersonateWithBoolRet(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => directoryCopy(srcDir, dstDir, recursive, overwrite));
        }

        private static bool directoryCopy(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            bool result = false;
            DirectoryInfo dir = new DirectoryInfo(srcDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory not found: " + srcDir);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create dst directories 
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            if (!Directory.Exists(dir.Name))
            {
                Directory.CreateDirectory(Path.Combine(dstDir, dir.Name));
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dstDir, dir.Name,file.Name);
                file.CopyTo(temppath, false);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(dstDir, dir.Name, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, recursive, overwrite);
                }
            }
            result = true;
            return result;
        }

        private static bool fileCopyAsUser(string srcFilename, string dstDirname, bool overwrite) => 
            ImpersonationHelper.ImpersonateWithBoolRet(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => fileCopy(srcFilename, dstDirname, overwrite));

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="srcFilename"></param>
        /// <param name="dstDirname"></param>
        /// <param name="overwrite"></param>
        private static bool fileCopy(string srcFilename, string dstDirname, bool overwrite)
        {
            bool result = false;
            FileInfo file = new FileInfo(srcFilename);

            if (!file.Exists)
            {
                throw new FileNotFoundException("Source file not found: " + srcFilename);
            }

            if (!Directory.Exists(dstDirname))
            {
                throw new DirectoryNotFoundException("Destination directory not found: " + dstDirname);
            }
            string dstFilepath = Path.Combine(dstDirname, file.Name);
            try
            {
                file.CopyTo(dstFilepath, overwrite);
                result = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                //MessageBox.Show($"Fout: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                result = false;
            }
            return result;
        }
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }

    public sealed class ImpersonationHelper
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            SafeTokenHandle safeTokenHandle;
            try
            {

                const int LOGON32_PROVIDER_DEFAULT = 0;
                //This parameter causes LogonUser to create a primary token.
                const int LOGON32_LOGON_INTERACTIVE = 2;

                // Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(userName, domainName, userPassword,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);
                //Facade.Instance.Trace("LogonUser called.");

                if (returnValue == false)
                {
                    int ret = Marshal.GetLastWin32Error();
                    //Facade.Instance.Trace($"LogonUser failed with error code : {ret}");

                    throw new System.ComponentModel.Win32Exception(ret);
                }

                using (safeTokenHandle)
                {
                    //Facade.Instance.Trace($"Value of Windows NT token: {safeTokenHandle}");
                    //Facade.Instance.Trace($"Before impersonation: {WindowsIdentity.GetCurrent().Name}");

                    // Use the token handle returned by LogonUser.
                    using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                    {
                        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
                        {
                            //Facade.Instance.Trace($"After impersonation: {WindowsIdentity.GetCurrent().Name}");
                            //Facade.Instance.Trace("Start executing an action");

                            actionToExecute();

                            //Facade.Instance.Trace("Finished executing an action");
                        }
                    }
                    //Facade.Instance.Trace($"After closing the context: {WindowsIdentity.GetCurrent().Name}");
                }

            }
            catch (Exception ex)
            {
                //Facade.Instance.Trace("Oh oh! Impersonate method failed.");
                //ex.HandleException();
                //Notify a caller about the issue
                throw;
            }
        }

        /// <summary>
        /// TODO: generic return type iso fixed bool return.
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <param name="actionToExecute"></param>
        /// <returns></returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static bool ImpersonateWithBoolRet(string domainName, string userName, string userPassword, Func<bool> actionToExecute)
        {
            SafeTokenHandle safeTokenHandle;
            bool retResultValue = false;
            try
            {

                const int LOGON32_PROVIDER_DEFAULT = 0;
                const int LOGON32_LOGON_INTERACTIVE = 2;

                bool returnValue = LogonUser(userName, domainName, userPassword,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);

                if (returnValue == false)
                {
                    int ret = Marshal.GetLastWin32Error();

                    throw new System.ComponentModel.Win32Exception(ret);
                }

                using (safeTokenHandle)
                {
                    using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                    {
                        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
                        {
                            retResultValue = actionToExecute();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //Facade.Instance.Trace("Oh oh! Impersonate method failed.");
                //ex.HandleException();
                //Notify a caller about the issue
                throw;
            }

            return retResultValue;
        }
    }
}
