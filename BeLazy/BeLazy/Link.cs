namespace BeLazy
{
    internal class Link
    {
        public int linkID;
        public bool isValid;

        public int UplinkUserID { get; set; }
        public int DownlinkUserID { get; set; }
        public int UplinkBTMSSystemID { get; set; }
        public int UplinkCTMSSystemID { get; set; }
        public int DownlinkBTMSSystemID { get; set; }
        public int DownlinkCTMSSystemID { get; set; }

        public string DownlinkCTMSPassword { get; set; }
        public string DownlinkCTMSURL { get; set; }
        public string DownlinkCTMSUsername { get; set; }
        public string DownlinkCTMSSystemName { get; set; }
        public string DownlinkCTMSSystemVersion { get; set; }

        public string DownlinkBTMSPassword { get; set; }
        public string DownlinkBTMSURL { get; set; }
        public string DownlinkBTMSUsername { get; set; }
        public string DownlinkBTMSSystemName { get; set; }
        public string DownlinkBTMSSystemVersion { get; set; }

        public string UplinkCTMSPassword { get; set; }
        public string UplinkCTMSURL { get; set; }
        public string UplinkCTMSUsername { get; set; }
        public string UplinkCTMSSystemName { get; set; }
        public string UplinkCTMSSystemVersion { get; set; }

        public string UplinkBTMSPassword { get; set; }
        public string UplinkBTMSURL { get; set; }
        public string UplinkBTMSUsername { get; set; }
        public string UplinkBTMSSystemName { get; set; }
        public string UplinkBTMSSystemVersion { get; set; }

        public string ClientIDForUplinkProject { get; set; }

        public Link(int linkID)
        {
            this.linkID = linkID;
            DatabaseInterface.GetLink(this);
        }
    }
}