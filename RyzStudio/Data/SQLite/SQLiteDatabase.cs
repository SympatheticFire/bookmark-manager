using System;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace RyzStudio.Data.SQLite
{
    public class SQLiteDatabase
    {
        #region static methods

        public static string escapeSQL(string query)
        {
            return query.Replace("'", "''").Trim();
        }

        public static string escapeValue(string text)
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
                rv = rv.Replace("[^" + (i + 1).ToString() + "]", escapeSQL(arguments[i]));
            }
            
            return rv;
        }
        
        #endregion
        
        protected SQLiteWrapper.SQLiteBase database = null;
        protected string databaseLocation = ":memory:";
        protected string lastError = "";
        
        protected string[] requiredTableList = new string[0];
        
        protected const string tableNameConfig = "ryz_app_xxxx_config";

        #region public properties

        [Browsable(false)]
        public SQLiteWrapper.SQLiteBase Database
        {
            get { return database; }
            set { database = value; }
        }

        [Browsable(false)]
        public string DatabaseLocation
        {
            get { return databaseLocation; }
            set { databaseLocation = value; }
        }

        [Browsable(false)]
        public string LastError
        {
            get { return lastError; }
        }

        [Browsable(false)]
        public int LastInsertID
        {
            get
            {
                if (database == null)
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

        public bool Create(bool use_memory = true)
        {
            lastError = string.Empty;

            if (string.IsNullOrEmpty(databaseLocation))
            {
                return false;
            }
            
            try
            {
                if (database != null)
                {
                    database.CloseDatabase();
                }

                database = new SQLiteWrapper.SQLiteBase(((use_memory) ? ":memory:" : databaseLocation));
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }
            
            bool rv = Prepare();
            if (!rv)
            {
                return false;
            }
            
            return CheckRequiredTables();
        }

        public bool Create(string filename, bool override_file = false)
        {
            lastError = string.Empty;
            databaseLocation = filename;

            if (string.IsNullOrEmpty(databaseLocation))
            {
                return false;
            }

            if (File.Exists(databaseLocation) && override_file)
            {
                try
                {
                    File.Delete(databaseLocation);
                }
                catch (Exception xc)
                {
                    lastError = xc.Message;
                    return false;
                }
            }
            
            try
            {
                if (database != null)
                {
                    database.CloseDatabase();
                }

                database = new SQLiteWrapper.SQLiteBase(databaseLocation);
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }
            
            bool rv = CheckRequiredTables();
            if (!rv)
            {
                Prepare();
            }
            
            return CheckRequiredTables();
        }

        public bool Load(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            
            lastError = string.Empty;
            databaseLocation = filename;
            
            try
            {
                if (database != null)
                {
                    database.CloseDatabase();
                }

                database = new SQLiteWrapper.SQLiteBase(databaseLocation);
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }
            
            return CheckRequiredTables();
        }

        public void Close()
        {
            if (database != null)
            {
                database.CloseDatabase();
            }
        }


        public DataTable DoQuery(string query)
        {
            lastError = string.Empty;
            if (database == null)
            {
                return null;
            }
            
            try
            {
                return database.ExecuteQuery(query);
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return null;
            }
        }

        public DataTable DoQuery(string query, params string[] args)
        {
            string sql = SQLiteDatabase.PrepareQuery(query, args);
            
            return DoQuery(sql);
        }

        public bool DoNonQuery(string query)
        {
            lastError = string.Empty;
            if (database == null)
            {
                return false;
            }
            
            try
            {
                database.ExecuteNonQuery(query);
                return true;
            }
            catch (Exception exc)
            {
                lastError = exc.Message;
                return false;
            }
        }

        public bool DoNonQuery(string query, params string[] args)
        {
            string sql = SQLiteDatabase.PrepareQuery(query, args);
            
            return DoNonQuery(sql);
        }

        public string DoQuerySingle(string query)
        {
            lastError = string.Empty;
            if (database == null)
            {
                return string.Empty;
            }
            
            DataTable tbl = DoQuery(query);

            if (tbl == null)
            {
                return string.Empty;
            }

            if (tbl.Columns.Count <= 0)
            {
                return string.Empty;
            }

            if (tbl.Rows.Count <= 0)
            {
                return string.Empty;
            }
            
            return tbl.Rows[0][0].ToString();
        }

        public string DoQuerySingle(string query, params string[] args)
        {
            string sql = SQLiteDatabase.PrepareQuery(query, args);
            
            return DoQuerySingle(sql);
        }

        public int DoQueryCount(string query)
        {
            if (database == null)
            {
                return -1;
            }
            
            DataTable tbl = DoQuery(query);

            if (tbl == null)
            {
                return -1;
            }

            if (tbl.Rows.Count <= 0)
            {
                return 0;
            }
            
            return tbl.Rows.Count;
        }

        public int DoQueryCount(string query, params string[] args)
        {
            string sql = SQLiteDatabase.PrepareQuery(query, args);
            
            return DoQueryCount(sql);
        }

        public bool DoQueryExist(string query)
        {
            int rv = DoQueryCount(query);
            
            return (rv > 0);
        }

        public bool DoQueryExist(string query, params string[] args)
        {
            string sql = SQLiteDatabase.PrepareQuery(query, args);
            
            return DoQueryExist(sql);
        }

        public bool HasTable(string table_name)
        {
            lastError = string.Empty;
            if (database == null)
            {
                return false;
            }
            
            int rv = this.DoQueryCount("SELECT 1 FROM sqlite_master WHERE type='table' AND name='" + escapeSQL(table_name) + "'");            

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


        protected bool PrepareConfig()
        {
            if (HasTable(tableNameConfig))
            {
                return true;
            }
            
            bool rv = this.DoNonQuery(@"
                BEGIN TRANSACTION;
                CREATE TABLE " + tableNameConfig + @" (cfg_name TEXT, cfg_value TEXT);
                COMMIT;
            ");

            return rv;
        }

        public bool SetConfig(string name, string value)
        {
            if (!PrepareConfig())
            {
                return false;
            }
            
            string sql = string.Empty;
            int rv = this.DoQueryCount("SELECT 1 FROM " + tableNameConfig + " WHERE cfg_name='" + escapeSQL(name) + "'");
            if (rv <= 0)
            {
                sql = "INSERT INTO " + tableNameConfig + " (cfg_name, cfg_value) VALUES ('[^1]', '[^2]');";
            }
            else
            {
                sql = "UPDATE " + tableNameConfig + " SET cfg_value='[^2]' WHERE cfg_name='[^1]';";
            }

            sql = PrepareQuery(sql, new string[] { name, value });
            
            return this.DoNonQuery(sql);
        }

        public string GetConfig(string name, string default_value = "")
        {
            if (!PrepareConfig())
            {
                return default_value;
            }
            
            bool rv = this.DoQueryExist("SELECT 1 FROM " + tableNameConfig + " WHERE cfg_name='" + escapeSQL(name) + "'");
            if (!rv)
            {
                return default_value;
            }
            
            return this.DoQuerySingle("SELECT cfg_value FROM " + tableNameConfig + " WHERE cfg_name='" + escapeSQL(name) + "'");
        }

        #endregion

        protected virtual bool Prepare()
        {
            return true;
        }
    }
}