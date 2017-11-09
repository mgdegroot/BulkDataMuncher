using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace BulkDataMuncher
{

    public static class CasesDB {
        private const string TABLE_META = "meta";
        private const string TABLE_CASE = "caseinfo";
        private const string TABLE_CASE_CONTENT = "casecontent";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private static MySqlConnection connection;

        static CasesDB()
        {
            //string connStr = "server=192.168.20.60;user=<gebruiker>;database=datamuncher;port=3307;password=<wachtwoord>;convert zero datetime=True";
            Open();
        }

        public static bool IsOpen => connection != null && connection.State == ConnectionState.Open;

        public static bool Open()
        {
            bool result = false;

            if (!string.IsNullOrEmpty(ConfigHandler.ConnectionString))
            {
                connection = new MySqlConnection(ConfigHandler.ConnectionString);
                connection.Open();
                result = true;
            }

            return result;
        }


        public static bool Exists(string caseNumber)
        {
            bool result = false;
            string prc = "prc_case_exists";
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@number", caseNumber);
            object isThere = cmd.ExecuteScalar();

            result = (isThere != null && Convert.ToBoolean(isThere) );

            return result;
        }

        public static void AddCase(CaseInfo theCase)
        {
            string prc = "prc_case_add";
            
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@number", theCase.Number);
            cmd.Parameters.AddWithValue("@name", theCase.Name);
            cmd.Parameters.AddWithValue("@owner", theCase.Owner);
            cmd.Parameters.AddWithValue("@create_date", theCase.Date);
            cmd.ExecuteNonQuery();
        }

        public static void ModifyCase(CaseInfo theCase)
        {
            string prc = "prc_case_modify";
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@number", theCase.Number);
            cmd.Parameters.AddWithValue("@name", theCase.Name);
            cmd.Parameters.AddWithValue("@owner", theCase.Owner);
            cmd.Parameters.AddWithValue("@create_date", theCase.Date);
            cmd.ExecuteNonQuery();
        }

        public static void AddTransferredFileToCaseDB(Util.FileSelection fileSelection, CaseInfo theCase)
        {
            string prc = "prc_content_add";

            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@case_number", theCase.Number);
            cmd.Parameters.AddWithValue("@path", fileSelection.Path);
            cmd.Parameters.AddWithValue("@filetype", fileSelection.Type);
            //cmd.Parameters.AddWithValue("@archive_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@archive_date", "2017-12-12");
            cmd.ExecuteNonQuery();
        }

        public static void AddTransferedFilesToCaseDB(List<Util.FileSelection> fileSelections, CaseInfo theCase)
        {
            foreach (var fileSelection in fileSelections)
            {
                if (fileSelection.State == Util.FileState.TRANSFERRED)
                {
                    AddTransferredFileToCaseDB(fileSelection, theCase);
                }
            }

        }

        public static CaseInfo GetCase(string caseNumber)
        {
            //string qry = $"SELECT number, name, owner, create_date, last_modify_date FROM {TABLE_CASE} WHERE number = '{MySqlHelper.EscapeString(caseNumber)}'";
            CaseInfo retVal;
            string prc = "prc_case_get";
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@number", caseNumber);

            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                retVal = new CaseInfo()
                {
                    Number = reader[0] as string,
                    Name = reader[1] as string,
                    Owner = reader[2] as string,
                    Date = (DateTime)reader[3] ,
                    IsNew = false,
                };
            }
            else
            {
                retVal = null;
            }
            reader.Close();

            return retVal;
        }

        public static MySqlDataAdapter GetDataAdapterCases(string caseNumber = "0")
        {
            string prc = "prc_case_get";
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@number", caseNumber);

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

            return adapter;
        }

        public static MySqlDataAdapter GetDataAdapterCaseContent(string caseNumber)
        {
            string prc = "prc_content_get";
            MySqlCommand cmd = new MySqlCommand(prc, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            cmd.Parameters.AddWithValue("@case_number", caseNumber);

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            return adapter;
        }

        //private static MySqlConnection connection
        //{
        //    get
        //    {
        //        string connStr = "server=192.168.20.60;user=dr_db;database=datamuncher;port=3307;password=Welkom01";
        //        MySqlConnection conn = new MySqlConnection(connStr);
        //        //try
        //        //{
        //        //    Console.WriteLine("Connecting to MySQL...");
        //        //    conn.Open();
        //        //    // Perform database operations
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    Console.WriteLine(ex.ToString());
        //        //}
        //        return conn;
        //    }

        //}
    }

    public static class CasesDB_sqlite
    {
        private const string TABLE_META = "datamuncher_meta";
        private const string TABLE_CASE = "datamuncher_cases";
        private const string TABLE_CASE_CONTENT = "datamuncher_content";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private const string DB_ALIAS = "CasesDB";

        

        public static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(ConfigHandler.DatabasePath);

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string qryCreateTableCase = $"CREATE TABLE {TABLE_CASE} (Number VARCHAR(30), Name VARCHAR(255), Owner VARCHAR(255), Date VARCHAR(50))";
                string qryCreateTableCaseContent = $"CREATE TABLE {TABLE_CASE_CONTENT} (CaseNumber VARCHAR(30), Path VARCHAR(255), FileType VARCHAR(4), ArchiveDate VARCHAR(50))";
                string qryCreateTableMeta = $"CREATE TABLE {TABLE_META} (Key VARCHAR(255), Value VARCHAR(512))";

                SQLiteCommand command = new SQLiteCommand(qryCreateTableCase, connection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(qryCreateTableCaseContent, connection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(qryCreateTableMeta, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }

        }

        public static bool Exists(string caseNumber)
        {
            var caseExists = false;
            string qry = $"SELECT 1 FROM {TABLE_CASE} WHERE Number = '{caseNumber}'";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(qry, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                caseExists = reader.Read();
                reader.Close();
                connection.Close();
            }
            return caseExists;
        }

        public static void AddCase(CaseInfo theCase)
        {
            string qry = $"INSERT INTO {TABLE_CASE}(Number, Name, Owner, Date) VALUES('{theCase.Number}','{theCase.Name}', '{theCase.Owner}', '{theCase.Date.ToString(DATE_FORMAT)}')";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(qry, connection);

                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void ModifyCase(CaseInfo theCase)
        {
            
            string qry =
                $"UPDATE {TABLE_CASE} SET Number = '{theCase.Number}', Name='{theCase.Name}', Owner='{theCase.Owner}', Date='{theCase.Date.ToString(DATE_FORMAT)}' WHERE Number='{theCase.Number}'";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand command = new SQLiteCommand(qry, connection);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void AddTransferredFileToCaseDB(Util.FileSelection fileSelection, CaseInfo theCase)
        {
            string qry = $"INSERT OR REPLACE INTO {TABLE_CASE_CONTENT}(CaseNumber, Path, FileType, ArchiveDate) VALUES('{theCase.Number}', '{fileSelection.Path}', '{fileSelection.Type}', '{DateTime.Now.ToString(DATETIME_FORMAT)}')";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand command = new SQLiteCommand(qry, connection);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void AddTransferedFilesToCaseDB(List<Util.FileSelection> fileSelections, CaseInfo theCase)
        {
            foreach (var fileSelection in fileSelections)
            {
                if (fileSelection.State == Util.FileState.TRANSFERRED)
                {
                    AddTransferredFileToCaseDB(fileSelection, theCase);
                }
            }

        }

        //public static void AddTransferedFilesToCaseDB(CaseInfo theCase)
        //{
        //    foreach (var fileSelection in theCase.Files)
        //    {
        //        if (fileSelection.State == Util.FileState.TRANSFERRED)
        //        {
        //            AddTransferredFileToCaseDB(fileSelection, theCase);
        //        }
        //    }
            
        //}

        public static CaseInfo GetCase(string caseNumber)
        {
            string qry = $"SELECT Number, Name, Owner, Date FROM {TABLE_CASE} WHERE Number = '{caseNumber}'";
            CaseInfo retVal;
            
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(qry, connection);

                SQLiteDataReader reader = command.ExecuteReader();
                
                if (reader.Read())
                {
                    retVal = new CaseInfo()
                    {
                        Number = reader["Number"] as string,
                        Name = reader["Name"] as string,
                        Owner = reader["Owner"] as string,
                        Date = DateTime.Parse(reader["Date"] as string),
                        IsNew = false,

                    };
                }
                else
                {
                    retVal = null;
                }
                reader.Close();
                connection.Close();
            }

            return retVal;
        }

        public static SQLiteDataAdapter GetDataAdapterCases(string caseNumber="")
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            //connection.Open();
            connection.CreateCommand();
            string qry = $"SELECT Number, Name, Owner, Date FROM {TABLE_CASE}";
            
            if (!string.IsNullOrEmpty(caseNumber))
            {
                qry += $" WHERE Number = '{caseNumber}'";
            }

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry, connection);

            return adapter;

        }

        public static SQLiteDataAdapter GetDataAdapterCaseContent(string caseNumber)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            //connection.Open();
            connection.CreateCommand();
            string qry = $"SELECT CaseNumber, Path, FileType, ArchiveDate FROM {TABLE_CASE_CONTENT} WHERE CaseNumber = '{caseNumber}'";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry, connection);
            return adapter;
        }

        //private static void AttachDB(string fileDB, string aliasName, SQLiteConnection cn)
        //{
        //    string sqlText = string.Format("ATTACH '{0}' AS {1}", fileDB, aliasName);
        //    SQLiteCommand cmd = new SQLiteCommand(sqlText, cn);
        //    cmd.ExecuteNonQuery();
        //    /*
        //     using(SQLiteConnection cn = new SQLiteConnection(GetConnectionString()))
        //    {
        //        AttachDB(@"C:\SQLite\UserData.sqlite3", "UserData", cn);
        //        // logic ....
        //    } 
        //     */
        //}


        public static string ConnectionString =>
            $"Data Source={ConfigHandler.DatabasePath};Version=3;Pooling=True;Max Pool Size=100;";
    }
}
