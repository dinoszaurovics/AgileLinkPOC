using System;
using System.Collections.Generic;

namespace BeLazy
{
    internal class Project
    {
        public string ExternalProjectCode { get; internal set; }
        public ProjectStatus Status { get; internal set; }
        public string InternalProjectCode { get; internal set; }
        public DateTime DateOrdered { get; internal set; }
        public DateTime DateApproved { get; internal set; }
        public DateTime Deadline { get; internal set; }
        public string ExternalProjectManagerName { get; internal set; }
        public string ExternalProjectManagerEmail { get; internal set; }
        public string ExternalProjectManagerPhone { get; internal set; }
        public string EndCustomer { get; internal set; }
        public int SpecialityID { get; internal set; }
        public int SourceLanguageID { get; internal set; }
        public List<int> TargetLanguageIDs { get; internal set; }
        public string Workflow { get; internal set; }
        public string CATTool { get; internal set; }
        public List<string> AnalysisResult { get; internal set; }
        public double PayableVolume { get; internal set; }
        public int PayableUnitID { get; internal set; }
        public string Instructions { get; internal set; }

        public Project()
        {
            TargetLanguageIDs = new List<int>();
        }
    }

    public enum ProjectStatus
    {
        Initiated, New, InProgress, Finished, Closed, Undefined
    }

}