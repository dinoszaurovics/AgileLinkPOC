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

        internal async Task<bool> GenerateUplinkProjectAsync(Project[] projects)
        {
            await Task.Delay(2000);
            Log.AddLog("Uploaded project to Uplink server");
            return true;
        }
    }
}