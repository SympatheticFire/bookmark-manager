using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

namespace RyzStudio.IO
{
    public abstract class SessionFileFormatBase
    {
        protected string CONST_KEYPASS = "";
        protected string CONST_PRODUCT = "";
        protected string CONST_STREAM_FILE_NAME = "";

        protected bool enableErrorReporting = false;
        protected string lastUsedFileName = "";

        #region public properties

        public string passkey
        {
            get { return CONST_KEYPASS; }
            set { CONST_KEYPASS = value; }
        }

        /*public string LastFilename
        {
            get { return lastUsedFileName; }
            set { lastUsedFileName = value; }
        }*/
        
        #endregion

        #region public methods

/*        public bool loadFromRyz()
        {
            return loadFromRyz(lastUsedFileName);
        }*/

        public bool loadFromRyz(string file_name)
        {
            lastUsedFileName = file_name;
            if (!File.Exists(file_name))
            {
                return false;
            }
            
            bool rv = false;
            
            try
            {
                ZipInputStream zipIn = new ZipInputStream(File.OpenRead(file_name));
                zipIn.Password = CONST_KEYPASS;
                ZipEntry theEntry = null;
                
                while ((theEntry = zipIn.GetNextEntry()) != null)
                {
                    string streamFileName = Path.GetFileName(theEntry.Name);
                    if (streamFileName.Equals(CONST_STREAM_FILE_NAME))
                    {
                        MemoryStream oxStream = new MemoryStream();
                        StreamWriter streamWriter = new StreamWriter(oxStream);
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = zipIn.Read(data, 0, data.Length);
                            if (size <= 0) break;
                            streamWriter.BaseStream.Write(data, 0, size);
                        }

                        oxStream.Position = 0;

                        StreamReader sr2 = new StreamReader(oxStream, Encoding.UTF8);
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(sr2.ReadToEnd());
                        
                        loadFromXmlDocument(ref xDoc);
                    }
                }

                zipIn.Close();
                
                rv = true;
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            
            return rv;
        }

        public bool loadFromXml()
        {
            return loadFromXml(lastUsedFileName);
        }

        public bool loadFromXml(string file_name)
        {
            lastUsedFileName = file_name;
            if (!File.Exists(file_name))
            {
                return false;
            }
            
            bool rv = false;
            
            try
            {
                StreamReader sr2 = new StreamReader(file_name, Encoding.UTF8);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(sr2.ReadToEnd());
                
                loadFromXmlDocument(ref xDoc);
                sr2.Close();
                
                rv = true;
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            
            return rv;
        }

/*        public bool saveToRyz()
        {
            return saveToRyz(lastUsedFileName);
        }
*/

        public bool saveToRyz(string file_name)
        {
            bool rv = false;
            lastUsedFileName = file_name;
            byte[] buffer = new byte[4096];

            try
            {
                File.Delete(file_name);
                FileInfo fileinfo1 = new FileInfo(file_name);
                if (!Directory.Exists(fileinfo1.DirectoryName))
                {
                    Directory.CreateDirectory(fileinfo1.DirectoryName);
                }
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }

            try
            {
                ZipOutputStream zipOutStream1 = new ZipOutputStream(File.Create(file_name));
                zipOutStream1.SetLevel(9);
                zipOutStream1.Password = CONST_KEYPASS;

                MemoryStream oxIndexStream = new MemoryStream();
                XmlTextWriter oxTW = new XmlTextWriter(oxIndexStream, Encoding.UTF8);
                saveToXmlTextWriter(ref oxTW);
                oxTW.Flush();

                // write to file
                zipOutStream1.PutNextEntry(new ZipEntry(CONST_STREAM_FILE_NAME));
                oxIndexStream.Position = 0;
                StreamReader sr2 = new StreamReader(oxIndexStream, Encoding.UTF8);
                int sourceBytes;
                do
                {
                    sourceBytes = sr2.BaseStream.Read(buffer, 0, buffer.Length);
                    zipOutStream1.Write(buffer, 0, sourceBytes);
                }
                while (sourceBytes > 0);

                sr2.Close();
                oxTW.Close();
                zipOutStream1.Finish();
                zipOutStream1.Close();
                
                rv = true;
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            
            return rv;
        }

        public bool saveToXml()
        {
            return saveToXml(lastUsedFileName);
        }

        public bool saveToXml(string file_name)
        {
            bool rv = false;
            
            lastUsedFileName = file_name;
            try
            {
                File.Delete(file_name);
                FileInfo fileinfo1 = new FileInfo(file_name);
                if (!Directory.Exists(fileinfo1.DirectoryName))
                {
                    Directory.CreateDirectory(fileinfo1.DirectoryName);
                }
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }

            try
            {
                XmlTextWriter oxTW = new XmlTextWriter(file_name, Encoding.UTF8);
                saveToXmlTextWriter(ref oxTW);
                oxTW.Flush();
                oxTW.Close();
                
                rv = true;
            }
            catch (Exception exc)
            {
                if (enableErrorReporting)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            
            return rv;
        }
        
        #endregion
        
        protected virtual void loadFromXmlDocument(ref XmlDocument xml_doc) { }
        protected virtual void saveToXmlTextWriter(ref XmlTextWriter writer) { }

        #region public methods (conversions)

        public int[] convIntArrayString(string s1, char c)        
        {
            string[] sarr = s1.Split(c);
            int[] iarr = new int[sarr.Length];
            for (int i = 0; i < sarr.Length; i++)
            {
                iarr[i] = Int32.Parse(sarr[i]);
            }

            return iarr;
        }

        public string convStringIntArray(int[] r, char s)
        {
            string t = null;
            for (int i = 0; i < r.Length; i++)
            {
                if (i != 0)
                {
                    t += s.ToString();
                }

                t += r[i].ToString();
            }

            return t;
        }

#endregion
    }
}