using System;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class SyncAction
    {
        private int linkID;
        public bool synxSuccess;
        public DateTime syncFinished;

        public SyncAction(int linkID)
        {
            this.linkID = linkID;
        }

        internal Task<bool> ProcessSyncAsync()
        {
            var result = SyncService();
            return result;
        }

        private async Task<bool> SyncService()
        {
            Link link = new Link(linkID);
            DownlinkManager dlm = new DownlinkManager(link);
            UplinkManager ulm = new UplinkManager(link);

            await ProjectTransferAsync(dlm, ulm);

            synxSuccess = true;
            syncFinished = DateTime.Now;
            return true;
        }

        private async Task<bool> ProjectTransferAsync(DownlinkManager dlm, UplinkManager ulm)
        {
            Project project = dlm.GenerateAbstractProject();
            await ulm.GenerateUplinkProject(project);
            return true;
        }
    }
}