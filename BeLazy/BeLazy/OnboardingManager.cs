using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class OnboardingManager
    {
        List<Task<bool>> onboardingTasks = new List<Task<bool>>();
        List<OnboardingCheck> onboardingChecks = new List<OnboardingCheck>();

        public OnboardingManager()
        {
            List<int> linkIDs = DatabaseInterface.GetLinksToSync();
            foreach (int linkID in linkIDs)
            {
                try
                {
                    OnboardingCheck obc = new OnboardingCheck(linkID);
                    onboardingChecks.Add(obc);
                    Task<bool> onboardingTask = obc.ProcessOBCheckAsync();
                    onboardingTasks.Add(onboardingTask);
                }
                catch (Exception ex)
                {
                    Log.AddLog("SyncAction error: " + ex.Message, ErrorLevels.Critical);
                }
            }

        }

        internal void ManageOBTasks()
        {
            Task.WaitAll(onboardingTasks.ToArray());
            foreach (OnboardingCheck obc in onboardingChecks)
            {
                Log.AddLog(obc.obSuccess ? "Onboarding check succesful" : "Onboarding check failed");
            }
        }
    }
}