using System;
using System.Drawing;
using System.IO;
using RyzStudio.Data.SQLite;

namespace bzit.bomg
{
    public class IconDatabase : SQLiteDatabase2
    {
        public IconDatabase()
        {
            this.requiredTableList = new string[] { "bzt_app_bomg_icons" };
        }

        protected override bool Prepare()
        {
            if (dbConnection == null)
            {
                return false;
            }
            
            return this.DoNonQuery(@"                
                CREATE TABLE bzt_app_bomg_icons 
                (
                    ico_id INTEGER PRIMARY KEY, 
                    ico_key TEXT,
                    ico_hash TEXT,
                    ico_content BLOB
                )                
            ") >= 0;
        }
        
#region public methods

        public bool HasIcon(string url)
        {
            return this.DoQueryExist("SELECT 1 FROM bzt_app_bomg_icons WHERE ico_key='" + SQLiteDatabase2.EscapeValue(url) + "'");
        }

/*        public bool AddIcon(string url, Image image)
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
                return this.DoNonQuery("UPDATE bzt_app_bomg_icons SET='" + imageToSQLString(image) + "' WHERE ico_key='" + SQLiteDatabase2.escapeValue(url) + "';") >= 0;
            }
            else
            {
                return this.DoNonQuery("INSERT INTO bzt_app_bomg_icons (ico_key, ico_content) VALUES ('" + SQLiteDatabase2.escapeValue(url) + "', '" + imageToSQLString(image) + "');") >= 0;
            }
        }*/

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
                return this.DoNonQuery("UPDATE bzt_app_bomg_icons SET='" + bytesToSQLString(image) + "' WHERE ico_key='" + SQLiteDatabase2.EscapeValue(url) + "';") >= 0;
            }
            else
            {
                return this.DoNonQuery("INSERT INTO bzt_app_bomg_icons (ico_key, ico_content) VALUES ('" + SQLiteDatabase2.EscapeValue(url) + "', '" + bytesToSQLString(image) + "');") >= 0;
            }
        }

        public Image GetIcon(string url)
        {
            if (!this.HasIcon(url))
            {
                return null;
            }
            
            string rs = this.DoQuerySingle("SELECT ico_content FROM bzt_app_bomg_icons WHERE ico_key='" + SQLiteDatabase2.EscapeValue(url) + "'");
            return sqlStringToImage(rs);
        }

        public void DeleteIcon(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }
            
            this.DoNonQuery("DELETE FROM bzt_app_bomg_icons WHERE ico_key='" + SQLiteDatabase2.EscapeValue(url) + "';");
        }
        
#endregion
        
        protected string imageToSQLString(Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, new System.Drawing.Imaging.ImageFormat(image.RawFormat.Guid));
            stream.Close();
            
            byte[] byteArray = stream.ToArray();
            
            return bytesToSQLString(byteArray);
        }

        protected string bytesToSQLString(byte[] image)
        {
            return Convert.ToBase64String(image);
        }

        protected Image sqlStringToImage(string base64_string)
        {            
            byte[] byteArray2 = Convert.FromBase64String(base64_string);

            MemoryStream stream2 = new MemoryStream();
            stream2.Write(byteArray2, 0, byteArray2.Length);
            
            Image displayImage = Image.FromStream(stream2);
            return displayImage;
        }
    }
}