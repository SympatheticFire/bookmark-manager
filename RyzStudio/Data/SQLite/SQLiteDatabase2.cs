using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace RyzStudio.Data.SQLite
{
    public class SQLiteDatabase2
    {
        #region static methods

        public static string EscapeSQL(string query)
        {
            return query.Replace("'", "''").Trim();
        }

        public static string EscapeValue(string text)
        {
            return text.Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", " \\r").Replace("\n", "\\n");
        }

        public static string PrepareQuery(string query, params string[] arguments)
        {
            string rv = query;

            if (string.IsNullOrEmpty(rv))
            {
                return string.Empty;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                rv = rv.Replace("[^" + (i + 1).ToString() + "]", EscapeSQL(arguments[i]));
            }

            return rv;
        }

        public static string Encode64(string text) { return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)); }

        #endregion

        protected SQLiteConnection dbConnection = null;
        protected string dbLocation = string.Empty;
        protected string lastError = "";

        protected string[] requiredTableList = new string[0];

        protected const string tableNameConfig = "ryz_app_xxxx_config";

        #region public properties

        [Browsable(false)]
        public SQLiteConnection DBConnection
        {
            get { return dbConnection; }
            set { dbConnection = value; }
        }

        [Browsable(false)]
        public string DBLocation
        {
            get { return dbLocation; }
            set { dbLocation = value; }
        }

        public string LastError { get { return lastError; } }

        public int LastInsertID
        {
            get
            {
                if (dbConnection == null)
                {
                    return 0;
                }

                DataTable dt = this.DoQuery("SELECT last_insert_rowid() AS ccc;");
                if (dt == null)
                {
                    return 0;
                }

                if (dt.Rows.Count <= 0)
                {
                    return 0;
                }

                return int.Parse(dt.Rows[0]["ccc"].ToString());
            }
        }

        #endregion

        #region public methods

        public bool CreateMemory()
        {
            lastError = string.Empty;

            dbLocation = ":memory:";

            try
            {
                dbConnection = new SQLiteConnection(string.Concat("Data Source=\"", dbLocation, "\";Version=3;UTF8Encoding=True;"));
                dbConnection.Open();
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }

            return true;
        }

        public bool CreateSpecial(string filename, bool overwriteFile = false, string password = null)
        {
            bool rs = this.CreateFile(filename, overwriteFile, password);
            if (!rs)
            {
                return false;
            }

            rs = this.Prepare();
            if (!rs)
            {
                return false;
            }

            return this.CheckRequiredTables();
        }

        public bool CreateFile(string filename, bool overwriteFile = false, string password = null)
        {
            if (File.Exists(filename))
            {
                if (overwriteFile)
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            try
            {
                SQLiteConnection.CreateFile(filename);
            }
            catch
            {
                return false;
            }

            return this.LoadFile(filename, password);
        }

        public bool LoadFile(string filename, string password = null)
        {
            lastError = string.Empty;

            if (!File.Exists(filename))
            {
                return false;
            }

            dbLocation = filename;

            try
            {
                dbConnection = new SQLiteConnection(string.Concat("Data Source=\"", filename, "\";Version=3;UTF8Encoding=True;", (password == null) ? string.Empty : string.Concat("Password=", Encode64(password), ";")));
                dbConnection.Open();
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }

            return true;
        }

        public void Close()
        {
            if (dbConnection != null)
            {
                try
                {
                    dbConnection.Close();
                }
                catch
                {
                    // do nothing
                }
            }
        }

        #region query

        public DataTable DoQuery(string query)
        {
            lastError = string.Empty;

            if (dbConnection == null)
            {
                return null;
            }

            try
            {
                SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                SQLiteDataReader dr = command.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dr);

                return dt;
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return null;
            }
        }

        public DataTable DoQuery(string query, params string[] args)
        {
            string sql = SQLiteDatabase2.PrepareQuery(query, args);

            return DoQuery(sql);
        }

        public int DoQueryCount(string query)
        {
            lastError = string.Empty;

            if (dbConnection == null)
            {
                return -1;
            }

            DataTable dt = this.DoQuery(query);
            if (dt == null)
            {
                return -1;
            }

            return dt.Rows.Count;
        }

        public int DoQueryCount(string query, params string[] args)
        {
            string sql = SQLiteDatabase2.PrepareQuery(query, args);

            return this.DoQueryCount(sql);
        }

        public bool DoQueryExist(string query)
        {
            int rv = this.DoQueryCount(query);

            return (rv > 0);
        }

        public bool DoQueryExist(string query, params string[] args)
        {
            string sql = SQLiteDatabase2.PrepareQuery(query, args);

            return this.DoQueryExist(sql);
        }

        public string DoQuerySingle(string query)
        {
            lastError = string.Empty;

            if (dbConnection == null)
            {
                return string.Empty;
            }

            DataTable dt = this.DoQuery(query);

            if (dt == null)
            {
                return string.Empty;
            }

            if (dt.Columns.Count <= 0)
            {
                return string.Empty;
            }

            if (dt.Rows.Count <= 0)
            {
                return string.Empty;
            }

            if (dt.Rows[0][0] is byte[])
            {
                return Encoding.UTF8.GetString(dt.Rows[0][0] as byte[]);
            }
            else
            {
                return dt.Rows[0][0].ToString();
            }          
        }

        public string DoQuerySingle(string query, params string[] args)
        {
            string sql = SQLiteDatabase2.PrepareQuery(query, args);

            return this.DoQuerySingle(sql);
        }

        public int DoNonQuery(string query)
        {
            lastError = string.Empty;

            if (dbConnection == null)
            {
                return -1;
            }

            int rv = 0;

            try
            {
                SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                rv = command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                rv = -1;
            }

            return rv;
        }

        public int DoNonQuery(string query, params string[] args)
        {
            string sql = SQLiteDatabase2.PrepareQuery(query, args);

            return this.DoNonQuery(sql);
        }

        public bool HasTable(string tableName)
        {
            lastError = string.Empty;

            if (dbConnection == null)
            {
                return false;
            }

            int rv = this.DoQueryCount("SELECT 1 FROM sqlite_master WHERE type='table' AND name='" + EscapeSQL(tableName) + "'");

            return (rv > 0);
        }

        public bool CheckRequiredTables()
        {
            bool rv = true;
            foreach (string tbl in requiredTableList)
            {
                if (string.IsNullOrEmpty(tbl))
                {
                    continue;
                }

                if (!this.HasTable(tbl))
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        #endregion

        public bool PrepareConfig()
        {
            if (this.HasTable(tableNameConfig))
            {
                return true;
            }

            int rv = this.DoNonQuery("CREATE TABLE " + tableNameConfig + " (cfg_name TEXT, cfg_value TEXT)");

            return rv > 0;
        }

        public bool SetConfig(string name, string value)
        {
            this.PrepareConfig();

            string sql = string.Empty;
            int rv = this.DoQueryCount("SELECT 1 FROM " + tableNameConfig + " WHERE cfg_name='" + EscapeSQL(name) + "'");
            if (rv <= 0)
            {
                sql = "INSERT INTO " + tableNameConfig + " (cfg_name, cfg_value) VALUES ('[^1]', '[^2]');";
            }
            else
            {
                sql = "UPDATE " + tableNameConfig + " SET cfg_value='[^2]' WHERE cfg_name='[^1]';";
            }

            sql = PrepareQuery(sql, new string[] { name, value });

            return this.DoNonQuery(sql) > 0;
        }

        public string GetConfig(string name, string defaultValue = "")
        {
            this.PrepareConfig();

            bool rv = this.DoQueryExist("SELECT 1 FROM " + tableNameConfig + " WHERE cfg_name='" + EscapeSQL(name) + "';");
            if (!rv)
            {
                return defaultValue;
            }

            return this.DoQuerySingle("SELECT cfg_value FROM " + tableNameConfig + " WHERE cfg_name='" + EscapeSQL(name) + "';");
        }

        #endregion

        protected virtual bool Prepare()
        {
            return true;
        }
    }
}
