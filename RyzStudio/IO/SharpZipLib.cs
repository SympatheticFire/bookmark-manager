using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace RyzStudio.IO
{
    public class SharpZipLib
    {
        /// <summary>
        /// Is zip file encrypted
        /// </summary>
        /// <param name="fn">Filename</param>
        /// <returns>Is encrypted</returns>
        public static bool IsZipEncrypted(string fn)
        {
            bool ret_val = false;
            try
            {
                ZipInputStream zipIn = new ZipInputStream(System.IO.File.OpenRead(fn));
                ZipEntry theEntry = null;
                while ((theEntry = zipIn.GetNextEntry()) != null)
                {
                    if (theEntry.IsCrypted)
                    {
                        ret_val = true;
                    }

                    break;
                }

                zipIn.Close();
            }
            catch
            {   
                // do nothing
            }

            return ret_val;
        }

        public static void AddFile(ZipOutputStream zipstream, string filename, string prefixpath = null)
        {
            byte[] buffer = new byte[4096];
            
            string f1 = "";
            if (prefixpath != null)
            {
                f1 = Path.GetDirectoryName(filename).TrimEnd('\\') + "\\";
                f1 = f1.Replace(prefixpath, "").TrimEnd('\\') + "\\";
                f1 = f1 + Path.GetFileName(filename);
                f1 = f1.TrimStart('\\');
            }
            
            ZipEntry entry = new ZipEntry(f1);
            entry.DateTime = DateTime.Now;
            zipstream.PutNextEntry(entry);

            FileStream fs = File.OpenRead(filename);
            int sourceBytes;
            do
            {
                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                zipstream.Write(buffer, 0, sourceBytes);
            }
            while (sourceBytes > 0);
        }

        public static void AddFolder(ZipOutputStream zipstream, string folderpath, string prefixpath = null)
        {
            foreach (string fn in Directory.GetFiles(folderpath, "*.*", System.IO.SearchOption.AllDirectories))
            {
                AddFile(zipstream, fn, prefixpath);
            }
        }
    }
}