using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
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

        internal async Task<Project[]> GetProjectsAsync()
        {
            TransactProjectRequest tpr = new TransactProjectRequest();
            tpr.method = "GetListActive";
            tpr.mode = "purchaseorder";
            tpr.token = link.DownlinkBTMSPassword;

            HttpResponseMessage response = await client.PostAsJsonAsync("", tpr);
            response.EnsureSuccessStatusCode();
            TransactProjectList tp = await response.Content.ReadAsAsync<TransactProjectList>();
            List<Project> projects = new List<Project>();

            MapProject(tp, ref projects);

            return projects.ToArray();

        }

        private void MapProject(TransactProjectList tp, ref List<Project> projects)
        {
            foreach (TransactProject transactProject in tp.data)
            {
                try
                {
                    // This is test line to filter for a single project

                    if (transactProject.number == "TLR0147694/10/1")
                    {

                        Project project = new Project();

                        project.ExternalProjectCode = transactProject.number;
                        switch (transactProject.status)
                        {
                            case "Confirmed":
                                project.Status = ProjectStatus.InProgress;
                                break;
                            case "Not confirmed":
                                project.Status = ProjectStatus.New;
                                break;
                            default:
                                project.Status = ProjectStatus.Undefined;
                                break;
                        }

                        project.InternalProjectCode = transactProject.your_processing_number;

                        DateTime tempDT = DateTime.MinValue;
                        if (DateTime.TryParse(transactProject.date_ordered, out tempDT))
                        {
                            project.DateOrdered = tempDT;
                        }
                        else
                        {
                            project.DateOrdered = DateTime.Now;
                        }

                        if (DateTime.TryParse(transactProject.date_confirmed, out tempDT))
                        {
                            project.DateApproved = tempDT;
                        }
                        else
                        {
                            project.DateApproved = DateTime.Now;
                        }

                        if (DateTime.TryParse(transactProject.date_delivery, out tempDT))
                        {
                            project.Deadline = tempDT;
                        }
                        else
                        {
                            project.Deadline = DateTime.Now;
                            Log.AddLog("Deadline could not be parsed.", ErrorLevels.Error);
                        }

                        project.ExternalProjectManagerName = transactProject.project_coordinator;
                        project.ExternalProjectManagerEmail = transactProject.project_coordinator_mail;
                        project.ExternalProjectManagerPhone = transactProject.project_coordinator_phone;
                        project.EndCustomer = transactProject.end_customer;
                        project.SpecialityID = MappingHelper.DoMappingToAbstract(MapType.Speciality, link.DownlinkBTMSSystemID, transactProject.specialty);
                        project.SourceLanguageID = MappingHelper.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_source);
                        project.TargetLanguageIDs.Add(MappingHelper.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_target));
                        project.Workflow = transactProject.to_do[0];
                        project.CATTool = transactProject.system;
                        project.AnalysisResult = transactProject.scaling;

                        double payableVolume = 0.0;
                        if (double.TryParse(transactProject.quantity, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out payableVolume))
                        {
                            project.PayableVolume = payableVolume;
                        }
                        else
                        {
                            project.PayableVolume = 0;
                        }

                        project.PayableUnitID = MappingHelper.DoMappingToAbstract(MapType.Unit, link.DownlinkBTMSSystemID, transactProject.quantity_unit);

                        project.Instructions = transactProject.instructions;

                        projects.Add(project);
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

