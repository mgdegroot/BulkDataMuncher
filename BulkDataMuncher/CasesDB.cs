using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataMuncher
{
    public static class CasesDB
    {
        private const string TABLE_CASE = "datamuncher_cases";
        private const string TABLE_CASE_CONTENT = "datamunchar_content";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string DATETIME_FORMAT = "yyyy-MM-dd hh:mm:ss";
        private const string DB_ALIAS = "CasesDB";

        public static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(ConfigHandler.DatabasePath);

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string qryCreateTableCase = $"CREATE TABLE {TABLE_CASE} (Number VARCHAR(30), Name VARCHAR(255), Owner VARCHAR(255), Date VARCHAR(50))";
                string qryCreateTableCaseContent = $"CREATE TABLE {TABLE_CASE_CONTENT} (CaseNumber VARCHAR(30), Path VARCHAR(255), FileType VARCHAR(4), ArchiveDate VARCHAR(50))";


                SQLiteCommand command = new SQLiteCommand(qryCreateTableCase, connection);
                
                command.ExecuteNonQuery();
                command = new SQLiteCommand(qryCreateTableCaseContent, connection);
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
                $"UPDATE {TABLE_CASE} SET Number = '{theCase.Number}', Name='{theCase.Name}', Owner='{theCase.Owner}', Date='{theCase.Date:DATE_FORMAT}'";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand command = new SQLiteCommand(qry, connection);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void AddFileToCaseDB(Util.FileSelection fileSelection, CaseInfo theCase)
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

        public static void AddTransferedFilesToCaseDB(CaseInfo theCase)
        {
            foreach (var fileSelection in theCase.Files)
            {
                if (fileSelection.State == Util.FileState.TRANSFERRED)
                {
                    AddFileToCaseDB(fileSelection, theCase);
                }
            }
            
        }

        public static CaseInfo GetCase(string caseNumber)
        {
            string qry = $"SELECT Number, Name, Owner, Date FROM {TABLE_CASE} WHERE Number = {caseNumber}";
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


        private static string ConnectionString =>
            $"Data Source={ConfigHandler.DatabasePath};Version=3;Pooling=True;Max Pool Size=100;";
    }
}
