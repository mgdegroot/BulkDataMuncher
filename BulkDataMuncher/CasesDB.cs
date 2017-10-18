using System;
using System.Collections.Generic;
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

        static CasesDB()
        {
            if (File.Exists(ConfigHandler.DatabasePath))
            {
                Connection = new SQLiteConnection($"Data Source={ConfigHandler.DatabasePath};Version=3;");
            }
        }

        public static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(ConfigHandler.DatabasePath);

            Connection = new SQLiteConnection($"Data Source={ConfigHandler.DatabasePath};Version=3;");

            Open();
            string qryCreatetable = $"CREATE TABLE {TABLE_CASE} (Number VARCHAR(30), Name VARCHAR(255), Owner VARCHAR(255), Date VARCHAR(50))";

            SQLiteCommand command = new SQLiteCommand(qryCreatetable, Connection);

            command.ExecuteNonQuery();
            Close();
        }

        public static bool Open()
        {
            if (Connection == null)
            {
                Connection = new SQLiteConnection($"Data Source={ConfigHandler.DatabasePath};Version=3;");
            }
            Connection.Open();

            return true;
        }

        public static bool Close()
        {
            Connection?.Close();

            return true;
        }

        public static void AddCase(CaseInfo theCase)
        {
            Open();
            string qry = $"INSERT INTO {TABLE_CASE}(Number, Name, Owner, Date) VALUES('{theCase.Number}','{theCase.Name}', '{theCase.Owner}', '{theCase.Date:YYYY-mm-DD}')";
            SQLiteCommand command = new SQLiteCommand(qry, Connection);

            command.ExecuteNonQuery();
            Close();
        }

        public static void ModifyCase(CaseInfo theCase)
        {
            Open();
            string qry =
                $"UPDATE {TABLE_CASE} SET Number = '{theCase.Number}', Name='{theCase.Name}', Owner='{theCase.Owner}', Date='{theCase.Date:YYYY-mm-DD}'";
            SQLiteCommand command = new SQLiteCommand(qry, Connection);

            command.ExecuteNonQuery();
            Close();
        }

        public static CaseInfo GetCase(string caseNumber)
        {
            Open();
            string qry = $"SELECT Number, Name, Owner, Data FROM {TABLE_CASE} WHERE Number = {caseNumber}";
            SQLiteCommand command = new SQLiteCommand(qry, Connection);

            SQLiteDataReader reader = command.ExecuteReader();
            CaseInfo retVal;
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
            Close();
            return retVal;
        }

        public static SQLiteConnection Connection { get; private set; }
    }
}
