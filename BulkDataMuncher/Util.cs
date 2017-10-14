using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataMuncher
{
    public class Util
    {
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
}
