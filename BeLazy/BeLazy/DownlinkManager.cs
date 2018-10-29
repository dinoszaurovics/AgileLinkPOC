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

        internal async Task<AbstractProject[]> GenerateAbstractProjectAsync()
        {
            AbstractProject[] abstractProjects;
            
            switch (link.DownlinkBTMSSystemName)
            {
                case "Transact":
                    TransactDownloadInterface tdi = new TransactDownloadInterface(link);
                    abstractProjects = await tdi.GetProjectsAsync();
                    break;
                default:
                    abstractProjects = null;
                    break;
            }
            
            return abstractProjects;
        }
    }
}