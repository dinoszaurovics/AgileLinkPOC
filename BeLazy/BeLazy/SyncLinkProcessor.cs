using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class SyncLinkProcessor
    {
        List<Task<bool>> syncTasks = new List<Task<bool>>();
        List<SyncAction> syncActions = new List<SyncAction>();

        public SyncLinkProcessor()
        {
            List<int> linkIDs = DatabaseInterface.GetLinksToSync();
            foreach (int linkID in linkIDs)
            {
                try
                {
                    SyncAction sa = new SyncAction(linkID);
                    syncActions.Add(sa);
                    Task<bool> syncTask = sa.ProcessSyncAsync();
                    syncTasks.Add(syncTask);
                }
                catch (Exception ex)
                {
                    Log.AddLog("SyncAction error: " + ex.Message, ErrorLevels.Critical);
                }
            }
        }

        internal void ManageSyncTasks()
        {
            Task.WaitAll(syncTasks.ToArray());
            foreach (SyncAction syncAction in syncActions)
            {
                Log.AddLog(syncAction.syncSuccess ? "Sync succesful" : "Sync failed");
            }
        }
    }
}