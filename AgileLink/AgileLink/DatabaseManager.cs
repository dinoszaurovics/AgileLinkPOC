using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace AgileLink
{
    internal class DatabaseManager
    {
        private static ConnectionStringSettings getConnectionString()
        {
            ConnectionStringSettings connString = new ConnectionStringSettings();
            connString = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (connString == null)
            {
                return null;
            }
            return connString;
        }

        internal static void ExecuteSQLUpdate(string sqlCommand)
        {
            ConnectionStringSettings connString = getConnectionString();
            using (SqlConnection con = new SqlConnection(connString.ToString()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sqlCommand, con))
                    command.ExecuteNonQuery();
            }

        }

        internal static int ExecuteSQLInsert(string sqlCommand)
        {
            int i = 0;
            ConnectionStringSettings connString = getConnectionString();
            using (SqlConnection con = new SqlConnection(connString.ToString()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(sqlCommand, con))
                    i = Convert.ToInt32(command.ExecuteScalar());
            }
            return i;
        }

        internal static DataTable ExecuteSQLSelect(string sqlCommand)
        {
            DataTable dt = new DataTable();
            ConnectionStringSettings connString = getConnectionString();

            using (SqlConnection con = new SqlConnection(connString.ToString()))
            {
                con.Open();
                using (SqlCommand command =
                    new SqlCommand(sqlCommand, con))
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    dt.Load(dr);
                }
            }

            return dt;
        }

    }
}