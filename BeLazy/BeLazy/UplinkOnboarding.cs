using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class UplinkOnboarding
    {
        internal bool obSuccess;
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
                    obSuccess = tdob.obSuccess;
                    break;
                default:
                    break;
            }
        }
    }
}