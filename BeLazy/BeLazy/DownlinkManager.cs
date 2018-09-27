using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class DownlinkManager
    {
        private Link link;

        public DownlinkManager(Link link)
        {
            this.link = link;
        }

        internal async Task<Project[]> GenerateAbstractProjectAsync()
        {
            Project[] projects;
            
            switch (link.DownlinkBTMSSystemName)
            {
                case "Transact":
                    TransactDownloadInterface tdi = new TransactDownloadInterface(link);
                    projects = await tdi.GetProjectsAsync();
                    break;
                default:
                    projects = null;
                    break;
            }
            
            return projects;
        }
    }
}