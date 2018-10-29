using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransactJSON;

namespace BeLazy
{
    internal class TransactDownloadInterface
    {
        static HttpClient client = new HttpClient();
        private Link link;

        public TransactDownloadInterface(Link link)
        {
            this.link = link;
            client.BaseAddress = new Uri(link.DownlinkBTMSURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal async Task<AbstractProject[]> GetProjectsAsync()
        {
            TransactProjectRequest tpr = new TransactProjectRequest();
            tpr.method = "GetListActive";
            tpr.mode = "purchaseorder";
            tpr.token = link.DownlinkBTMSPassword;

            HttpResponseMessage response = await client.PostAsJsonAsync("", tpr);
            response.EnsureSuccessStatusCode();
            TransactProjectList tp = await response.Content.ReadAsAsync<TransactProjectList>();
            List<AbstractProject> projects = new List<AbstractProject>();

            MapProject(tp, ref projects);

            return projects.ToArray();

        }

        private void MapProject(TransactProjectList tp, ref List<AbstractProject> projects)
        {
            foreach (TransactProject transactProject in tp.data)
            {
                try
                {
                    // This is test line to filter for a single project

                    //if (transactProject.number == "TLR0148500/50/1")
                    //{

                    AbstractProject abstractProject = new AbstractProject();

                    if (!String.IsNullOrEmpty(transactProject.number))
                    {
                        abstractProject.ExternalProjectCode = transactProject.number;
                    }

                    if (!String.IsNullOrEmpty(transactProject.status))
                    {
                        switch (transactProject.status)
                        {
                            
                            case "Not confirmed":
                                abstractProject.Status = ProjectStatus.New;
                                break;
                            case "Confirmed":
                                abstractProject.Status = ProjectStatus.InProgress;
                                break;
                            case "Quality check started":
                                abstractProject.Status = ProjectStatus.QAinProgress;
                                break;
                            case "Invoiceable":
                                abstractProject.Status = ProjectStatus.Completed;
                                break;
                            case "Invoiced":
                                abstractProject.Status = ProjectStatus.Closed;
                                break;
                            case "Cancelled":
                                abstractProject.Status = ProjectStatus.Cancelled;
                                break;
                            default:
                                abstractProject.Status = ProjectStatus.Undefined;
                                break;
                        }
                    }


                    if (abstractProject.Status == ProjectStatus.InProgress)
                    {
                        if (!String.IsNullOrEmpty(transactProject.your_processing_number))
                        {
                            abstractProject.InternalProjectCode = transactProject.your_processing_number;
                        }

                        DateTime tempDT = DateTime.MinValue;

                        if (DateTime.TryParse(transactProject.date_ordered, out tempDT))
                        {
                            abstractProject.DateOrdered = tempDT;
                        }
                        else
                        {
                            abstractProject.DateOrdered = DateTime.Now;
                        }

                        if (DateTime.TryParse(transactProject.date_confirmed, out tempDT))
                        {
                            abstractProject.DateApproved = tempDT;
                        }
                        else
                        {
                            abstractProject.DateApproved = DateTime.Now;
                        }

                        if (DateTime.TryParse(transactProject.date_delivery, out tempDT))
                        {
                            abstractProject.Deadline = tempDT;
                        }
                        else
                        {
                            abstractProject.Deadline = DateTime.Now;
                            Log.AddLog("Deadline could not be parsed.", ErrorLevels.Error);
                        }

                        if (!String.IsNullOrEmpty(transactProject.project_coordinator))
                        {
                            abstractProject.ExternalProjectManagerName = transactProject.project_coordinator;
                        }

                        if (!String.IsNullOrEmpty(transactProject.project_coordinator_mail))
                        {
                            abstractProject.ExternalProjectManagerEmail = transactProject.project_coordinator_mail;
                        }

                        if (!String.IsNullOrEmpty(transactProject.project_coordinator_phone))
                        {
                            abstractProject.ExternalProjectManagerPhone = transactProject.project_coordinator_phone;
                        }

                        if (!String.IsNullOrEmpty(transactProject.end_customer))
                        {
                            abstractProject.EndCustomer = transactProject.end_customer;
                        }

                        if (!String.IsNullOrEmpty(transactProject.specialty))
                        {
                            try
                            {
                                abstractProject.SpecialityID = MappingManager.DoMappingToAbstract(MapType.Speciality, link.DownlinkBTMSSystemID, transactProject.specialty);
                            }
                            catch (Exception ex)
                            {
                                Log.AddLog("Transact speciality not mapped: " + transactProject.specialty + " - " + ex.Message, ErrorLevels.Warning);
                            }
                        }

                        if (!String.IsNullOrEmpty(transactProject.language_source))
                        {
                            try
                            {
                                abstractProject.SourceLanguageID = MappingManager.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_source);
                            }
                            catch (Exception ex)
                            {
                                Log.AddLog("Transact source language not mapped: " + transactProject.language_source + " - " + ex.Message, ErrorLevels.Warning);
                            }
                        }

                        if (!String.IsNullOrEmpty(transactProject.language_target))
                        {
                            try
                            {
                                abstractProject.TargetLanguageIDs.Add(MappingManager.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_target));
                            }
                            catch (Exception ex)
                            {
                                Log.AddLog("Transact target language not mapped: " + transactProject.language_target + " - " + ex.Message, ErrorLevels.Warning);
                            }
                        }

                        abstractProject.Workflow = "";
                        foreach (string to_do_item in transactProject.to_do)
                        {
                            if (abstractProject.Workflow == "")
                            {
                                abstractProject.Workflow += to_do_item;
                            }
                            else
                            {
                                abstractProject.Workflow += "¤" + to_do_item;
                            }
                        }

                        if (!String.IsNullOrEmpty(transactProject.system))
                        {
                            abstractProject.CATTool = transactProject.system;
                        }

                        Regex analysisLineMatcher = new Regex(@"^\s*(?<category>[\p{L}\d\s%-]+):\s*(?<wordcount>[\d,\.]+)\s*Words at\s*(?<weight>[\d\.]+)%;\s*$");

                        foreach (string analysisLine in transactProject.scaling)
                        {
                            if (analysisLineMatcher.IsMatch(analysisLine))
                            {
                                Match analysisLineParser = analysisLineMatcher.Match(analysisLine);
                                WordCountAnalysisItem wcai = new WordCountAnalysisItem();

                                wcai.Weight = Double.Parse(analysisLineParser.Groups["weight"].ToString());
                                wcai.WordCount = Double.Parse(analysisLineParser.Groups["wordcount"].ToString().Replace(",", ""));   // value can be 1,319 => 1319
                                string category = analysisLineParser.Groups["category"].ToString();

                                switch (category)
                                {
                                    case "Repetitions":
                                        wcai.StartPc = -1;
                                        wcai.EndPc = -1;
                                        break;
                                    case "100%":
                                        wcai.StartPc = 100;
                                        wcai.EndPc = 100;
                                        break;
                                    case "No Match":
                                        wcai.StartPc = 0;
                                        wcai.EndPc = 0;
                                        break;
                                    default:
                                        Regex pcCategoryMatcher = new Regex(@"^\s*(?<startPc>\d+)%\s*-\s*(?<endPc>\d+)%\s*$");
                                        if (pcCategoryMatcher.IsMatch(category))
                                        {
                                            Match pcParser = pcCategoryMatcher.Match(category);
                                            wcai.StartPc = int.Parse(pcParser.Groups["startPc"].ToString());
                                            wcai.EndPc = int.Parse(pcParser.Groups["endPc"].ToString());
                                        }
                                        break;
                                }

                                abstractProject.AnalysisCategories.Add(wcai);
                            }

                        }


                        double payableVolume = 0.0, priceTotal = 0.0, priceUnit = 0.0;

                        if (double.TryParse(transactProject.quantity, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out payableVolume))
                        {
                            abstractProject.PayableVolume = payableVolume;
                        }
                        else
                        {
                            abstractProject.PayableVolume = 0;
                        }

                        if (double.TryParse(transactProject.price_unit, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out priceUnit))
                        {
                            abstractProject.PriceUnit = priceUnit;
                        }
                        else
                        {
                            abstractProject.PriceUnit = 0;
                        }

                        if (double.TryParse(transactProject.prize_total, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out priceTotal))
                        {
                            abstractProject.PriceTotal = priceTotal;
                        }
                        else
                        {
                            abstractProject.PriceTotal = 0;
                        }

                        if (!String.IsNullOrEmpty(transactProject.quantity_unit))
                        {
                            try
                            {
                                abstractProject.PayableUnitID = MappingManager.DoMappingToAbstract(MapType.Unit, link.DownlinkBTMSSystemID, transactProject.quantity_unit);
                            }
                            catch (Exception ex)
                            {
                                Log.AddLog("Transact unit not mapped: " + transactProject.quantity_unit + " - " + ex.Message, ErrorLevels.Warning);
                            }
                        }

                        if (!String.IsNullOrEmpty(transactProject.instructions))
                        {
                            abstractProject.VendorNotes = transactProject.instructions;
                        }

                        if (!String.IsNullOrEmpty(transactProject.customer_check_criteria))
                        {
                            abstractProject.PMNotes = transactProject.customer_check_criteria;
                        }

                        if (transactProject.feedback_deliveries.Count > 0 && !String.IsNullOrEmpty(transactProject.feedback_deliveries[0].link_download))
                        {
                            abstractProject.ClientNotes = transactProject.feedback_deliveries[0].link_download;
                        }

                        projects.Add(abstractProject);
                    }
                }
                catch (Exception ex)
                {
                    Log.AddLog("Processing Transact project failed:" + ex.Message, ErrorLevels.Error);
                }
            }
        }
    }
}

namespace TransactJSON
{
    public class TransactProjectList
    {
        public string code { get; set; }
        public string message { get; set; }
        public bool has_error { get; set; }
        public bool encoded { get; set; }
        public List<TransactProject> data { get; set; }

        public TransactProjectList()
        {
            data = new List<TransactProject>();
        }
    }


    public class AcrossTask
    {
        public string project_id { get; set; }
        public string project_name { get; set; }
        public string document_name { get; set; }
        public string language_source { get; set; }
        public string language_target { get; set; }
        public string task { get; set; }
        public string status { get; set; }
        public string assigned_user { get; set; }
        public string progress { get; set; }
    }

    public class TransactProject
    {
        public string number { get; set; }
        public string status { get; set; }
        public string your_processing_number { get; set; }
        public string belongs_to { get; set; }
        public string date_ordered { get; set; }
        public string date_confirmed { get; set; }
        public string date_delivery { get; set; }
        public string supplier { get; set; }
        public string project_coordinator { get; set; }
        public string project_coordinator_mail { get; set; }
        public string project_coordinator_phone { get; set; }
        public string end_customer { get; set; }
        public string specialty { get; set; }
        public string language_source { get; set; }
        public string language_target { get; set; }
        public List<string> to_do { get; set; }
        public string system { get; set; }
        public string grid_server { get; set; }
        public List<string> scaling { get; set; }
        public string quantity { get; set; }
        public string quantity_unit { get; set; }
        public string price_unit { get; set; }
        public string prize_total { get; set; }
        public List<TransactFile> files { get; set; }
        public string instructions { get; set; }
        public string customer_check_criteria { get; set; }
        public List<AcrossTask> across_tasks { get; set; }
        public List<FeedbackDelivery> feedback_deliveries { get; set; }
        public List<string> delivery_format { get; set; }

        public TransactProject()
        {
            to_do = new List<string>();
            scaling = new List<string>();
            files = new List<TransactFile>();
            feedback_deliveries = new List<FeedbackDelivery>();
            delivery_format = new List<string>();
            across_tasks = new List<AcrossTask>();
        }
    }

    public class TransactFile
    {
        public string file_name { get; set; }
        public string date_created { get; set; }
        public string date_expiration { get; set; }
        public string size { get; set; }
        public int download_count { get; set; }
        public bool download_required { get; set; }
        public string link { get; set; }
    }

    public class FeedbackDelivery
    {
        public int id { get; set; }
        public string date_expected { get; set; }
        public string date_delivered { get; set; }
        public bool forced_delivery { get; set; }
        public string comment { get; set; }
        public string link_formalcheck { get; set; }
        public string link_errorspy { get; set; }
        public string link_download { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
    }

    public class TransactProjectRequest
    {
        public string token { get; set; }
        public string mode { get; set; }
        public string method { get; set; }
    }

}

