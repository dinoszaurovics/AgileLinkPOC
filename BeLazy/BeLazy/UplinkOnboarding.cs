using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class UplinkOnboarding
    {
        private Link link;

        public UplinkOnboarding(Link link)
        {
            this.link = link;
        }

        internal async Task GetOnboardingReport(AbstractProject[] projects)
        {
            switch (link.UplinkBTMSSystemName)
            {
                case "XTRF":
                    XTRFUploadOnboarding tdob = new XTRFUploadOnboarding(link, projects);
                    await tdob.GenerateReport();
                    break;
                default:
                    break;
            }
        }
    }
}