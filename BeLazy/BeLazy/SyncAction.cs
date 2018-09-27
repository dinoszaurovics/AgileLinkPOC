using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class SyncAction
    {
        private int linkID;
        public bool syncSuccess;
        public DateTime syncFinished;

        public SyncAction(int linkID)
        {
            this.linkID = linkID;
        }

        internal async Task<bool> ProcessSyncAsync()
        {
            Link link = new Link(linkID);
            DownlinkManager dlm = new DownlinkManager(link);
            UplinkManager ulm = new UplinkManager(link);

            await ProjectTransferAsync(dlm, ulm);

            syncSuccess = true;
            syncFinished = DateTime.Now;
            return true;
        }

        private async Task<bool> ProjectTransferAsync(DownlinkManager dlm, UplinkManager ulm)
        {
            Project[] projects = await dlm.GenerateAbstractProjectAsync();
            if (projects.Length != 0)
            {
                await ulm.GenerateUplinkProjectAsync(projects);
                return true;
            }
            else
            {
                Log.AddLog("Project could not be created", ErrorLevels.Critical);
                return false;
            }
        }
    }
}