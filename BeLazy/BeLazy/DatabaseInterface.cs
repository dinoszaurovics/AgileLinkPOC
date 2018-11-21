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
            string SQLcommand = "SELECT * from tLinks WHERE IsActive = 1";
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

                link.UplinkUserID = GetIntValue(dr["uplinkUserID"].ToString());
                link.DownlinkUserID = GetIntValue(dr["downlinkUserID"].ToString());
                link.UplinkBTMSSystemID = GetIntValue(dr["UplinkBTMSSystemID"].ToString());
                link.UplinkCTMSSystemID = GetIntValue(dr["UplinkCTMSSystemID"].ToString());
                link.UplinkBTMSSystemTypeID = GetIntValue(dr["ubTMSSystemTypeID"].ToString());
                link.UplinkCTMSSystemTypeID = GetIntValue(dr["ucTMSSystemTypeID"].ToString());

                link.DownlinkBTMSSystemID = GetIntValue(dr["DownlinkBTMSSystemID"].ToString());
                link.DownlinkCTMSSystemID = GetIntValue(dr["DownlinkCTMSSystemID"].ToString());
                link.DownlinkBTMSSystemTypeID = GetIntValue(dr["dbTMSSystemTypeID"].ToString());
                link.DownlinkCTMSSystemTypeID = GetIntValue(dr["dcTMSSystemTypeID"].ToString());
                link.ClientIDForUplinkProject = dr["ClientIDForUplinkProject"].ToString();

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

        internal static void ClearLog()
        {
            string SQLcommand = String.Format("DELETE FROM tLogMessages");
            DatabaseManager.ExecuteSQLUpdate(SQLcommand);
        }

        internal static bool ProjectExists(string externalProjectCode, int linkID)
        {
           string SQLcommand = String.Format("SELECT * FROM tProject WHERE ExternalProjectCode = '{0}' AND LinkID = {1}", externalProjectCode, linkID);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            return dt.Rows.Count > 0;
        }

        internal static Language GetLanguage(int languageID)
        {
            string SQLcommand = String.Format("SELECT FullName, ISO2, ISO3 FROM tLanguages WHERE LanguageID = {0}",
            languageID);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                throw new Exception("Language not found: " + languageID);
            }
            else
            {
                DataRow dr = dt.Rows[0];
                Language language = new Language()
                {
                    LanguageID = languageID,
                    FullName = dr["FullName"].ToString(),
                    ISO2 = dr["ISO2"].ToString(),
                    ISO3 = dr["ISO3"].ToString()
                };

                return language;
            }
        }

        internal static string GetMappingScript(int linkID, MapType mapType)
        {
            string SQLcommand = String.Format("SELECT ScriptExpression FROM tMappingScripts WHERE MapType = '{0}' AND LinkID = {1} ",
              mapType, linkID);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                throw new Exception("Error in script mapping: " + linkID + " - " + mapType);
            }
            else
            {
                DataRow dr = dt.Rows[0];
                return dr["ScriptExpression"].ToString();
            }
        }

        private static int GetIntValue(string value)
        {
            int result;
            if (Int32.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

        internal static int GetMappingForGeneralValues(string idToReturn, string table, int tMSSystemTypeID, string searchField, string itemName)
        {
            string SQLcommand = String.Format("SELECT {0} FROM {1} WHERE TMSSystemTypeID = {2} AND {3} = '{4}' ",
               idToReturn, table, tMSSystemTypeID, searchField, itemName);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                throw new Exception("Mapping value is not valid: " + tMSSystemTypeID + " - " + searchField + " - " + itemName);
            }
            else
            {
                DataRow dr = dt.Rows[0];
                return Convert.ToInt32(dr[idToReturn]);
            }
        }

        internal static string GetMappingToUplinkValue(MapType mapType, Link link, string projectValue)
        {
            string SQLcommand = String.Format("SELECT MappedValue FROM tUplinkValueMapper WHERE LinkID = {0} AND WorkflowType = '{1}' AND ProjectValue = '{2}'",
            link.linkID, mapType.ToString(), projectValue);

            DataTable dt = DatabaseManager.ExecuteSQLSelect(SQLcommand);
            if (dt.Rows.Count != 1)
            {
                throw new Exception("Mapping value is not found: " + projectValue + " - " + link.UplinkBTMSSystemName + " - " + mapType);
            }
            else
            {
                DataRow dr = dt.Rows[0];
                return dr["MappedValue"].ToString();
            }
        }

        internal static void SaveProjectToDatabase(AbstractProject project, Link link)
        {
            string SQLcommand = "INSERT INTO tProject (" +
                "[LinkID], " +
                "[ExternalProjectCode], " +
                "[Status], " +
                "[InternalProjectCode], " +
                "[DateOrdered], " +
                "[DateApproved], " +
                "[Deadline], " +
                "[ExternalProjectManagerName], " +
                "[ExternalProjectManagerEmail], " +
                "[ExternalProjectManagerPhone], " +
                "[EndCustomer], " +
                "[SpecialityID], " +
                "[SourceLanguageID], " +
                "[CATTool], " +
                "[PayableVolume], " +
                "[PayableUnitID], " +
                "[PriceUnit], " +
                "[PriceTotal], " +
                "[PMNotes], " +
                "[VendorNotes], " +
                "[ClientNotes] " +
                ") VALUES (" +
                (link.linkID.ToString() ?? "null") + ", " +
                SQLEscape(project.ExternalProjectCode) + ", " +
                SQLEscape(project.Status.ToString()) + ", " +
                SQLEscape(project.InternalProjectCode) + ", " +
                SqlDateTime(project.DateOrdered) + ", " +
                SqlDateTime(project.DateApproved) + ", " +
                SqlDateTime(project.Deadline) + ", " +
                SQLEscape(project.ExternalProjectManagerName) + ", " +
                SQLEscape(project.ExternalProjectManagerEmail) + ", " +
                SQLEscape(project.ExternalProjectManagerPhone) + ", " +
                SQLEscape(project.EndCustomer) + ", " +
                (project.SpecialityID.ToString() ?? "null") + ", " +
                (project.SourceLanguageID.ToString() ?? "null") + ", " +
                SQLEscape(project.CATTool) + ", " +
                (project.PayableVolume.ToString() ?? "null") + ", " +
                (project.PayableUnitID.ToString() ?? "null") + ", " +
                (project.PriceUnit.ToString() ?? "null") + ", " +
                (project.PriceTotal.ToString() ?? "null") + ", " +
                SQLEscape(project.PMNotes) + ", " +
                SQLEscape(project.VendorNotes) + ", " +
                SQLEscape(project.ClientNotes)
                + "); SELECT @@IDENTITY"
                ;

            int projectID = DatabaseManager.ExecuteSQLInsert(SQLcommand);

            foreach (int langID in project.TargetLanguageIDs)
            {
                SQLcommand = "INSERT INTO tProjectTargetLanguages (ProjectID, LanguageID) VALUES (" +
                projectID.ToString() + ", " +
                langID.ToString() +
                "); SELECT @@IDENTITY";

                DatabaseManager.ExecuteSQLInsert(SQLcommand);
            }

            foreach (var analysisCategory in project.AnalysisCategories)
            {
                SQLcommand = "INSERT INTO tProjectAnalysisCategories " +
                    "(ProjectID, StartPC, EndPC, WordCount, CharacterCount, SegmentCount, PlaceholderCount, Weight)" +
                    " VALUES (" +
                projectID.ToString() + ", " +
                (analysisCategory.StartPc.ToString() ?? "null") + ", " +
                (analysisCategory.EndPc.ToString() ?? "null") + ", " +
                (analysisCategory.WordCount.ToString() ?? "null") + ", " +
                (analysisCategory.CharacterCount.ToString() ?? "null") + ", " +
                (analysisCategory.SegmentCount.ToString() ?? "null") + ", " +
                (analysisCategory.PlaceholderCount.ToString() ?? "null") + ", " +
                (analysisCategory.Weight.ToString() ?? "null") +
                "); SELECT @@IDENTITY";

                DatabaseManager.ExecuteSQLInsert(SQLcommand);
            }

        }

        internal static void SaveLogMessageToDatabase(string message, string errorLevel)
        {
            string SQLcommand = "INSERT INTO tLogMessages " +
                    "(ErrorLevel, ErrorMessage)" +
                    " VALUES (" +
                    SQLEscape(errorLevel) + ", " +
                    SQLEscape(message) +
                    "); SELECT @@IDENTITY";
            DatabaseManager.ExecuteSQLInsert(SQLcommand);
        }

        private static string SqlDateTime(DateTime? value)
        {
            if (value != null)
            {
                string result = "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                return result;
            }
            else
                return "null";
        }

        private static string SQLEscape(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "null";
            }

            string result = "'" + value.Replace("'", "''") + "'";
            return result;
        }
    }

    class Language
    {
        public int LanguageID;
        public string FullName;
        public string ISO2;
        public string ISO3;
    }

}