using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileLink
{
    class Program
    {
        
        static void Main(string[] args)
        {
            try
            {
                Options.Initialize();

//                while (true)
                {
                    SyncLinkProcessor slp = new SyncLinkProcessor();
                    Log.AddLog("Thread returned to Main", ErrorLevels.Information);
                    slp.ManageSyncTasks();
                    Log.AddLog("Running finished" , ErrorLevels.Information);
                    //Thread.Sleep(60000);
                }

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.AddLog("Unhandled exception: " + ex.Message, ErrorLevels.Fatal);
                Console.ReadKey();
            }
        }
    }
}
