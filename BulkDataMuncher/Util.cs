using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace BulkDataMuncher
{
    public class Util
    {

        public static void DirectoryCopyAsUser(string domain, string username, string password, string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            ImpersonationHelper.Impersonate(domain, username, password, delegate
            {
                DirectoryCopy(srcDir, dstDir, recursive, overwrite);
            });


        }

        public static void DirectoryCopy(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
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

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dstDir, file.Name);
                file.CopyTo(temppath, false);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(dstDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, recursive, overwrite);
                }
            }
        }

        public static void FileCopyAsUser(string domain, string username, string password, string srcFilename, string dstDirname, bool overwrite)
        {
            ImpersonationHelper.Impersonate(domain, username, password, delegate
            {
                FileCopy(srcFilename, dstDirname, overwrite);
            });
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="srcFilename"></param>
        /// <param name="dstDirname"></param>
        /// <param name="overwrite"></param>
        public static void FileCopy(string srcFilename, string dstDirname, bool overwrite)
        {
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
            file.CopyTo(dstFilepath, overwrite);

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
                //Facade.Instance.Trace("Oh no! Impersonate method failed.");
                //ex.HandleException();
                //On purpose: we want to notify a caller about the issue /Pavel Kovalev 9/16/2016 2:15:23 PM)/
                throw;
            }
        }
    }
}
