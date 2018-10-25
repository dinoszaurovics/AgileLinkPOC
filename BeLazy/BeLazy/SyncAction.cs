using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class SyncAction
    {
        private int linkID;
        public bool syncSuccess;
        public DateTime syncFinished;
        internal Link link;

        public SyncAction(int linkID)
        {
            this.linkID = linkID;
        }

        internal async Task<bool> ProcessSyncAsync()
        {
            try
            {
                link = new Link(linkID);
                DownlinkManager dlm = new DownlinkManager(link);
                UplinkManager ulm = new UplinkManager(link);

                await ProjectTransferAsync(dlm, ulm);

                syncSuccess = true;
                syncFinished = DateTime.Now;
                return true;
            }
            catch(Exception ex)
            {
                Log.AddLog("Synchronization error: " + ex.Message, ErrorLevels.Error);
                return false;
            }
        }

        private async Task<bool> ProjectTransferAsync(DownlinkManager dlm, UplinkManager ulm)
        {
            AbstractProject[] projects = await dlm.GenerateAbstractProjectAsync();
            if (projects.Length != 0)
            {
                StatsHelper.AddProjectStats(projects, link);
                await ulm.GenerateUplinkProjectAsync(projects);
                return true;
            }
            else
            {
                Log.AddLog("No project found.", ErrorLevels.Information);
                return false;
            }
        }
    }
}