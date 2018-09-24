using System;

namespace AgileLink
{
    internal class DownlinkManager
    {
        private Link link;

        public DownlinkManager(Link link)
        {
            this.link = link;
        }

        internal Project GenerateAbstractProject()
        {
            Project project = new Project();
            Log.AddLog("Downloading project from Downlink server");
            return project;
        }
    }
}