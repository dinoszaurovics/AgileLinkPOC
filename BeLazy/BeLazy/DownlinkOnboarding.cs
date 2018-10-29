using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class DownlinkOnboarding
    {
        private Link link;

        public DownlinkOnboarding(Link link)
        {
            this.link = link;
        }

        internal async Task<AbstractProject[]> GetOnboardingReport()
        {
            AbstractProject[] abstractProjects;
            switch (link.DownlinkBTMSSystemName)
            {
                case "Transact":
                    TransactDownloadOnboarding tdob = new TransactDownloadOnboarding(link);
                    abstractProjects = await tdob.GenerateReport();
                    break;
                default:
                    abstractProjects = null;
                    break;
            }
            return abstractProjects;
        }
    }
}