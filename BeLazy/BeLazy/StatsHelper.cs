using System;

namespace BeLazy
{
    internal class StatsHelper
    {
       
        internal static void AddProjectStats(Project[] projects, Link link)
        {
            foreach (Project project in projects)
            {
                try
                {
                    DatabaseInterface.SaveProjectToDatabase(project, link);
                }
                catch (Exception ex)
                {
                    Log.AddLog("Saving project to database failed: " + ex.Message, ErrorLevels.Error);
                }
            }
        }
    }
}