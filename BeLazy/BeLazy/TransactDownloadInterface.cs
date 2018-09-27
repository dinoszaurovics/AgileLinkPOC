﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BeLazy
{
    internal class TransactDownloadInterface
    {
        static HttpClient client = new HttpClient();
        private Link link;

        public TransactDownloadInterface(Link link)
        {
            this.link = link;
            client.BaseAddress = new Uri("https://transact.transline.de/api/vendor?api"); //  new Uri(link.DownlinkBTMSURL);
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
                project.Speciality = MappingHelper.DoMappingToAbstract(MapType.Speciality, link.DownlinkBTMSSystemID, transactProject.specialty);
                project.SourceLanguage = MappingHelper.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_source);
                project.TargetLanguage = MappingHelper.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemID, transactProject.language_target);
                project.Workflow = transactProject.to_do;
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

                project.PayableUnit = MappingHelper.DoMappingToAbstract(MapType.Unit, link.DownlinkBTMSSystemID, transactProject.quantity_unit);

                project.Instructions = transactProject.instructions;
                
                projects.Add(project);
            }
        }
    }

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
        public List<string> files { get; set; }
        public string instructions { get; set; }
        public string customer_check_criteria { get; set; }
        public List<AcrossTask> across_tasks { get; set; }
        public List<string> feedback_deliveries { get; set; }
        public List<string> delivery_format { get; set; }

        public TransactProject()
        {
            to_do = new List<string>();
            scaling = new List<string>();
            files = new List<string>();
            feedback_deliveries = new List<string>();
            delivery_format = new List<string>();
            across_tasks = new List<AcrossTask>();
        }
    }

    public class TransactProjectRequest
    {
        public string token { get; set; }
        public string mode { get; set; }
        public string method { get; set; }
    }

}
