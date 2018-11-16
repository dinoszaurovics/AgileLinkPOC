using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class OnboardingCheck
    {
        private int linkID;
        public bool obSuccess;
        public DateTime obFinished;
        internal Link link;

        public OnboardingCheck(int linkID)
        {
            this.linkID = linkID;
        }

        internal async Task<bool> ProcessOBCheckAsync()
        {
            try
            {
                link = new Link(linkID);
                DownlinkOnboarding dlob = new DownlinkOnboarding(link);
                UplinkOnboarding ulob = new UplinkOnboarding(link);

                AbstractProject[] abstractProjects = await dlob.GetOnboardingReport();
                await ulob.GetOnboardingReport(abstractProjects);

                obSuccess = dlob.obSuccess && ulob.obSuccess;
                obFinished = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Log.AddLog("Onboarding check error: " + ex.Message, ErrorLevels.Error);
                return false;
            }
        }

     
    }
}