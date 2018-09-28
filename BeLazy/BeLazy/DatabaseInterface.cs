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
                if (Int32.TryParse(dr["SyncInterval"].ToString(), out syncInterval))
                {
                    syncInterval = 1;
                }

                if (lastSync + new TimeSpan(0, syncInterval, 0) < DateTime.Now)
                {
                    result.Add((Int32)dr["LinkID"]);
                }
            }
            return result;

        }

        internal static void GetLink(Link link)
        {
            string SQLcommand = "SELECT * from vLinks Where linkID = " + link.linkID.ToString();
            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                link.isValid = false;
                return;
            }
            else
            {
                DataRow dr = dt.Rows[0];

                link.UplinkUserID = (Int32)dr["uplinkUserID"];
                link.DownlinkUserID = (Int32)dr["downlinkUserID"];
                link.UplinkBTMSSystemID = (Int32)dr["UplinkBTMSSystemID"];
                link.UplinkCTMSSystemID = (Int32)dr["UplinkCTMSSystemID"];
                link.DownlinkBTMSSystemID = (Int32)dr["DownlinkBTMSSystemID"];
                link.DownlinkCTMSSystemID = (Int32)dr["DownlinkCTMSSystemID"];

                link.DownlinkCTMSPassword = dr["dcPassword"].ToString();
                link.DownlinkCTMSURL = dr["dcURL"].ToString();
                link.DownlinkCTMSUsername = dr["dcUserName"].ToString();
                link.DownlinkCTMSSystemName = dr["dcTMSName"].ToString();
                link.DownlinkCTMSSystemVersion = dr["dcTMSVersion"].ToString();

                link.DownlinkBTMSPassword = dr["dbPassword"].ToString();
                link.DownlinkBTMSURL = dr["dbURL"].ToString();
                link.DownlinkBTMSUsername = dr["dbUserName"].ToString();
                link.DownlinkBTMSSystemName = dr["dbTMSName"].ToString();
                link.DownlinkBTMSSystemVersion = dr["dbTMSVersion"].ToString();

                link.UplinkCTMSPassword = dr["ucPassword"].ToString();
                link.UplinkCTMSURL = dr["ucURL"].ToString();
                link.UplinkCTMSUsername = dr["ucUserName"].ToString();
                link.UplinkCTMSSystemName = dr["ucTMSName"].ToString();
                link.UplinkCTMSSystemVersion = dr["ucTMSVersion"].ToString();

                link.UplinkBTMSPassword = dr["ubPassword"].ToString();
                link.UplinkBTMSURL = dr["ubURL"].ToString();
                link.UplinkBTMSUsername = dr["ubUserName"].ToString();
                link.UplinkBTMSSystemName = dr["ubTMSName"].ToString();
                link.UplinkBTMSSystemVersion = dr["ubTMSVersion"].ToString();


            }

        }

        internal static int GetMappingToAbstractValue(string idToReturn, string table, int tMSSystemID, string searchField, string itemName)
        {
            string SQLcommand = String.Format("SELECT {0} FROM {1} WHERE TMSSystemTypeID = {2} AND {3} = '{4}' ",
               idToReturn, table, tMSSystemID, searchField, itemName);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                throw new Exception("Mapping value is not found: " + tMSSystemID + " - " + searchField + " - " + itemName);
            }
            else
            {
                DataRow dr = dt.Rows[0];
                return Convert.ToInt32(dr[idToReturn]);
            }
        }
    }
}