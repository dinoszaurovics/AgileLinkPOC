using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class UplinkManager
    {
        private Link link;

        public UplinkManager(Link link)
        {
            this.link = link;
        }

        internal async Task<bool> GenerateUplinkProject(Project project)
        {
            Thread.Sleep(1000);
            Log.AddLog("Uploading project to Uplink server");
        }
    }
}