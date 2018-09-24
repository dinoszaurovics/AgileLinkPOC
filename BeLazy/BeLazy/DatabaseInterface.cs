using System;
using System.Collections.Generic;
using System.Data;

namespace BeLazy
{
    internal class DatabaseInterface
    {
        internal static List<int> GetLinksToSync()
        {
            List<int> result = new List<int>();
            string SQLcommand = "SELECT * from tLinks";
            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            foreach (DataRow dr in dt.Rows)
            {
                DateTime lastSync;
                if (!DateTime.TryParse(dr["LastSync"].ToString(), out lastSync))
                {
                    lastSync = DateTime.MinValue;
                }

                int syncInterval;
                if(Int32.TryParse(dr["SyncInterval"].ToString(), out syncInterval))
                {
                    syncInterval = 1;
                }

                if (lastSync + new TimeSpan(0, syncInterval, 0) < DateTime.Now)
                {
                    result.Add( (Int32)dr["LinkID"] );
                }
            }
            return result;

        }
    }
}