using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeLazy
{
    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                Options.Initialize();
                if (args.Length > 0 && args[0] == "Onboarding")
                {
                    Log.AddLog("Onboarding check starting", ErrorLevels.Information);
                    OnboardingManager om = new OnboardingManager();
                    Log.AddLog("Thread returned to Main", ErrorLevels.Information);
                    om.ManageOBTasks();
                    Log.AddLog("Running finished", ErrorLevels.Information);
                }
                else if(args.Length > 0 && args[0] == "Sync")
                {
                    Log.AddLog("Data transfer started", ErrorLevels.Information);
                    SyncLinkProcessor slp = new SyncLinkProcessor();
                    Log.AddLog("Thread returned to Main", ErrorLevels.Information);
                    slp.ManageSyncTasks();
                    Log.AddLog("Running finished", ErrorLevels.Information);
                }
                else if (args.Length > 0 && args[0] == "ClearLog")
                {
                    DatabaseInterface.ClearLog();
                }
            }
            catch (Exception ex)
            {
                Log.AddLog("Unhandled exception: " + ex.Message, ErrorLevels.Fatal);
                Console.ReadKey();
            }
        }
    }
}
