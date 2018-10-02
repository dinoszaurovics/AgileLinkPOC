using System;

namespace BeLazy
{
    internal class StatsHelper
    {
       
        internal static void AddProjectStats(Project[] projects, Link link)
        {
            foreach (Project project in projects)
            {
                DatabaseInterface.SaveProjectToDatabase(project, link);
            }
        }
    }
}