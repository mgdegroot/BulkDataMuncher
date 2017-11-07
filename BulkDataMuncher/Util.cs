using System;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32.SafeHandles;

namespace BulkDataMuncher
{
    public class Util
    {
        public class ProgressInfo
        {
            private static int percentageDone = 0;
            public ProgressInfo()
            {
                TransferredFiles = new List<Util.FileSelection>();
                StepSize = 1;
            }
            public string CurrentFileSrc { get; set; }
            public string CurrentFileDst { get; set; }
            public List<Util.FileSelection> TransferredFiles { get; set; }

            public static int PercentageDone
            {
                get { return percentageDone; }
            }

            public static int StepSize { get; set; }
            public static void Increment()
            {
                Interlocked.Add(ref percentageDone, StepSize);
            }

        }


        public enum FileSelectionType
        {
            FILE = 0,
            DIRECTORY = 1,
        }

        public enum FileState
        {
            PENDING = 0,
            TRANSFERRED = 1,
            DUPLICATE = 2,
            ERROR = 3,
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

        public class ReturnStuffer
        {
            public ReturnStuffer()
            {
                ListOfStuff = new List<string>();
                Files = new List<FileSelection>();
            }
            public bool Result { get; set; }
            public string Msg { get; set; }
            public string ErrMsg { get; set; }
            public List<string> ListOfStuff { get; set; }
            public List<FileSelection> Files { get; set; }
        }

        public static void WriteCaseFileToOutput(CaseInfo theCase)
        {
            string p = theCase.CaseDirectory;
            string filename = $"_zaak_overzicht_{DateTime.Now.ToString("yyyy-MM-ddTHHmmss")}.txt";

            string text = $"zaak:\t{theCase.Name}\r\n" +
                          $"nummer:\t{theCase.Number}\r\n" +
                          $"eigenaar:\t{theCase.Owner}\r\n" +
                          $"datum:\t{theCase.Date.ToString("yyyy-MM-dd")}\r\n" +
                          $"datum laatst bijgewerkt:\t{DateTime.Now.ToString("yyyy-MM-dd")}\r\n" +
                          $"folder:\t{theCase.CaseDirectory}\r\n" +
                          $"aantal:\t{theCase.Files.Count}\r\n" +
                          $"\r\n\r\n"
                ;

            foreach (var fileSelection in theCase.Files)
            {
                text += fileSelection.State + " - " + fileSelection.Path + System.Environment.NewLine;
            }
            File.WriteAllText(Path.Combine(p, filename), text);


        }

        public static bool DirectoryExistst(string path)
        {
            if (ConfigHandler.UsernameSet)
            {
                return directoryExistsAsUser(path).Result;
            }
            else
            {
                return directoryExists(path).Result;
            }
        }


        public static bool CreateDirectory(string path)
        {
            ReturnStuffer result;
            if (ConfigHandler.UsernameSet)
            {
                result = createDirectoryAsUser(path);
            }
            else
            {
                result = createDirectory(path);
            }
            return result.Result;
        }


        public static FileSelection FileCopy(string srcFilename, string dstDirname, bool overwrite)
        {
            ReturnStuffer result;
            if (ConfigHandler.UsernameSet)
            {
                result = fileCopyAsUser(srcFilename, dstDirname, overwrite);
            }
            else
            {
                result = fileCopy(srcFilename, dstDirname, overwrite);
            }

            //return result.Result ? result.Files[0] : new FileSelection() {Path="", State=FileState.PENDING, Type= FileSelectionType.FILE };
            return result.Files[0];
        }


        public static List<FileSelection> DirectoryCopy(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            ReturnStuffer result;
            if (ConfigHandler.UsernameSet)
            {
                result = directoryCopyAsUser(srcDir, dstDir, recursive, overwrite);
            }
            else
            {
                result = directoryCopy(srcDir, dstDir, recursive, overwrite);
            }
            return result.Files;
        }


        private static ReturnStuffer directoryExistsAsUser(string path)
        {
            return ImpersonationHelper.ImpersonateWithReturnStuffer(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => directoryExists(path));
        }


        private static ReturnStuffer directoryExists(string path)
        {
            ReturnStuffer result = new ReturnStuffer()
            {
                Result = Directory.Exists(path),
            };
            return result;
        }


        private static ReturnStuffer createDirectoryAsUser(string path)
        {
            return ImpersonationHelper.ImpersonateWithReturnStuffer(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () =>createDirectory(path));
        }


        private static ReturnStuffer createDirectory(string path)
        {
            ReturnStuffer result = new ReturnStuffer()
            {
                Result = false,
            };
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return result;
        }


        private static ReturnStuffer directoryCopy(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            ReturnStuffer result = new ReturnStuffer();

            // Directories keep their base -->
            dstDir = Path.Combine(dstDir, new DirectoryInfo(srcDir).Name);

            //Create all of the directories -->
            foreach (string dirPath in Directory.GetDirectories(srcDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(srcDir, dstDir));
                result.Files.Add(new FileSelection()
                {
                    Path = dirPath.Replace(srcDir, dstDir),
                    State = FileState.TRANSFERRED,
                    Type = FileSelectionType.DIRECTORY,
                });
            }


            // TODO: get total file count via other means if this is performance hit.... -->
            int fileCnt = Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories).Length;

            Util.ProgressInfo.StepSize = 100 / fileCnt;

            //Copy all the files & Replaces any files with the same name -->
            foreach (string newPath in Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(srcDir, dstDir), overwrite);
                result.Files.Add(new FileSelection() { Path = newPath.Replace(srcDir, dstDir), State = FileState.TRANSFERRED, Type = FileSelectionType.FILE });

                Util.ProgressInfo.Increment();
            }

            return result;
        }


        private static ReturnStuffer directoryCopyAsUser(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        {
            return ImpersonationHelper.ImpersonateWithReturnStuffer(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => directoryCopy(srcDir, dstDir, recursive, overwrite));
        }

        //private static ReturnStuffer directoryCopy_old(string srcDir, string dstDir, bool recursive = true, bool overwrite = false)
        //{
        //    ReturnStuffer result = new ReturnStuffer()
        //    {
        //        Result = false,
        //    };
        //    DirectoryInfo dir = new DirectoryInfo(srcDir);

        //    if (!dir.Exists)
        //    {
        //        throw new DirectoryNotFoundException("Source directory not found: " + srcDir);
        //    }

        //    DirectoryInfo[] dirs = dir.GetDirectories();

        //    // Create dst directories 
        //    if (!Directory.Exists(dstDir))
        //    {
        //        Directory.CreateDirectory(dstDir);
        //    }

        //    if (!Directory.Exists(dir.Name))
        //    {
        //        Directory.CreateDirectory(Path.Combine(dstDir, dir.Name));
        //    }

        //    result.Files.Add(new FileSelection()
        //    {
        //        Path = Path.Combine(dstDir, dir.Name),
        //        State = FileState.TRANSFERRED,
        //        Type = FileSelectionType.DIRECTORY,
        //    });


        //    FileInfo[] files = dir.GetFiles();
        //    foreach (FileInfo file in files)
        //    {
        //        string temppath = Path.Combine(dstDir, dir.Name,file.Name);
        //        file.CopyTo(temppath, false);
        //        result.Files.Add(new FileSelection() {Path=temppath, State=FileState.TRANSFERRED, Type=FileSelectionType.FILE});
        //    }

        //    if (recursive)
        //    {
        //        foreach (DirectoryInfo subdir in dirs)
        //        {
        //            string temppath = Path.Combine(dstDir, dir.Name, subdir.Name);
        //            ReturnStuffer subresult = directoryCopy(subdir.FullName, temppath, recursive, overwrite);
        //            result.Files.AddRange(subresult.Files);
        //        }
        //    }
        //    result.Result = true;
        //    return result;
        //}


        private static ReturnStuffer fileCopyAsUser(string srcFilename, string dstDirname, bool overwrite) => 
            ImpersonationHelper.ImpersonateWithReturnStuffer(ConfigHandler.Domain, ConfigHandler.Username, ConfigHandler.Password, () => fileCopy(srcFilename, dstDirname, overwrite));


        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="srcFilename"></param>
        /// <param name="dstDirname"></param>
        /// <param name="overwrite"></param>
        private static Util.ReturnStuffer fileCopy(string srcFilename, string dstDirname, bool overwrite)
        {
            ReturnStuffer returnStuffer = new ReturnStuffer()
            {
                Result = false,
            };

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
                returnStuffer.Files.Add(new FileSelection() { Path=dstFilepath, State = FileState.TRANSFERRED, Type = FileSelectionType.FILE});
                Util.ProgressInfo.Increment();
                returnStuffer.Result = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    returnStuffer.Files.Add(new FileSelection()
                    {
                        Path = dstFilepath,
                        State = FileState.DUPLICATE,
                        Type = FileSelectionType.FILE
                    });
                    returnStuffer.ErrMsg = ex.Message;
                    returnStuffer.Result = false;
                }
                else
                {
                    returnStuffer.Files.Add(new FileSelection()
                    {
                        Path = dstFilepath,
                        State = FileState.ERROR,
                        Type = FileSelectionType.FILE
                    });
                    returnStuffer.ErrMsg = ex.Message;
                    returnStuffer.Result = false;
                }
            }
            return returnStuffer;
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
        public static Util.ReturnStuffer ImpersonateWithReturnStuffer(string domainName, string userName, string userPassword, Func<Util.ReturnStuffer> actionToExecute)
        {
            SafeTokenHandle safeTokenHandle;
            Util.ReturnStuffer returnStuffer = new Util.ReturnStuffer();
            returnStuffer.Result = false;
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
                            returnStuffer = actionToExecute();
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

            return returnStuffer;
        }
    }
}
