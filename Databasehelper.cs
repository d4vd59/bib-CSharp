using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizduell
{
    public class Databasehelper
    {
        private string connection = "Data Source=Database/QuizApp.db";

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connection);
        }
        public DataTable GetCategories()
        {
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM category", conn);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}