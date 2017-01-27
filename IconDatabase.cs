using System;
using System.Drawing;
using System.IO;
using RyzStudio.Data.SQLite;

namespace bzit.bomg
{
    public class IconDatabase : SQLiteDatabase
    {
        public IconDatabase()
        {
            this.requiredTableList = new string[] { "bzt_app_bomg_icons" };
        }

        protected override bool Prepare()
        {
            if (database == null)
            {
                return false;
            }
            
            return this.DoNonQuery(@"
                BEGIN TRANSACTION;
                CREATE TABLE bzt_app_bomg_icons 
                (
                    ico_id INTEGER PRIMARY KEY, 
                    ico_key TEXT,
                    ico_hash TEXT,
                    ico_content BLOB
                );
                COMMIT;
            ");
        }
        
#region public methods

        public bool HasIcon(string url)
        {
            return this.DoQueryExist("SELECT 1 FROM bzt_app_bomg_icons WHERE ico_key='" + escapeStringOut(url) + "'");
        }

        public bool AddIcon(string url, Image image)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (image == null)
            {
                return false;
            }
            
            if (this.HasIcon(url))
            {
                return this.DoNonQuery("UPDATE bzt_app_bomg_icons SET='" + imageToSQLString(image) + "' WHERE ico_key='" + escapeStringOut(url) + "';");
            }
            else
            {
                return this.DoNonQuery("INSERT INTO bzt_app_bomg_icons (ico_key, ico_content) VALUES ('" + escapeStringOut(url) + "', '" + imageToSQLString(image) + "');");
            }
        }

        public bool AddIcon(string url, byte[] image)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (image == null)
            {
                return false;
            }
            
            if (this.HasIcon(url))
            {
                return this.DoNonQuery("UPDATE bzt_app_bomg_icons SET='" + bytesToSQLString(image) + "' WHERE ico_key='" + escapeStringOut(url) + "';");
            }
            else
            {
                return this.DoNonQuery("INSERT INTO bzt_app_bomg_icons (ico_key, ico_content) VALUES ('" + escapeStringOut(url) + "', '" + bytesToSQLString(image) + "');");
            }
        }

        public Image GetIcon(string url)
        {
            if (!this.HasIcon(url))
            {
                return null;
            }
            
            string rs = this.DoQuerySingle("SELECT ico_content FROM bzt_app_bomg_icons WHERE ico_key='" + escapeStringOut(url) + "'");
            return sqlStringToImage(rs);
        }

        public void DeleteIcon(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }
            
            this.DoNonQuery("DELETE FROM bzt_app_bomg_icons WHERE ico_key='" + escapeStringOut(url) + "';");
        }
        
#endregion
        
        public static string escapeStringOut(string text)
        {
            return text.Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", " \\r").Replace("\n", "\\n");
        }

        public string imageToSQLString(Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, new System.Drawing.Imaging.ImageFormat(image.RawFormat.Guid));
            stream.Close();
            
            byte[] byteArray = stream.ToArray();
            
            return bytesToSQLString(byteArray);
        }

        public string bytesToSQLString(byte[] image)
        {
            return Convert.ToBase64String(image);
        }

        public Image sqlStringToImage(string base64_string)
        {
            byte[] byteArray2 = Convert.FromBase64String(base64_string);

            MemoryStream stream2 = new MemoryStream();
            stream2.Write(byteArray2, 0, byteArray2.Length);
            
            Image displayImage = Image.FromStream(stream2);
            return displayImage;
        }
    }
}