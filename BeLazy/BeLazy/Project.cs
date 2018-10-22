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
        public double PayableVolume { get; internal set; }
        public int PayableUnitID { get; internal set; }
        public double PriceUnit { get; internal set; }
        public double PriceTotal { get; internal set; }
        public string PMNotes { get; internal set; }
        public string VendorNotes { get; internal set; }
        public string ClientNotes { get; internal set; }
        public List<WordCountAnalysisItem> AnalysisCategories { get; internal set; }

        public string SourceLanguageISO2
        {
            get
            {
                return DatabaseInterface.GetLanguage(SourceLanguageID).ISO2;
            }
        }

        public string TargetLanguageISO2
        {
            get
            {
                string result = "";

                foreach(var targetLanguageID in TargetLanguageIDs)
                {
                    string langCode = DatabaseInterface.GetLanguage(targetLanguageID).ISO2;
                    result += (result == "") ? langCode : ("-" + langCode);
                }

                return result;
            }
        }

        public Project()
        {
            TargetLanguageIDs = new List<int>();
            AnalysisCategories = new List<WordCountAnalysisItem>();
        }
    }

    public enum ProjectStatus
    {
        Initiated, New, InProgress, Finished, Closed, Undefined
    }

    public class WordCountAnalysisItem
    {
        // Super ICE Match: 102 -> 102
        // ICE match: 101 -> 101
        // Reps:  -1 -> -1
        // 100%: 100 -> 100
        // No match: 0 -> 0

        public int StartPc { get; internal set; }
        public int EndPc { get; internal set; }
        public double WordCount { get; internal set; }
        public double CharacterCount { get; internal set; }
        public double SegmentCount { get; internal set; }
        public double PlaceholderCount { get; internal set; }

        public double Weight { get; internal set; }

        public double WeightedWordCount
        {
            get
            {
                return WordCount * Weight / 100;
            }
        }
    }
}