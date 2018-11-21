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
    internal class TransactDownloadOnboarding
    {
        private Link link;
        static HttpClient client = new HttpClient();

        private List<string> unmappedLanguages = new List<string>();
        private List<string> unmappedSpecialities = new List<string>();
        private List<string> unmappedUnits = new List<string>();
        private List<string> unmappedStatuses = new List<string>();
        internal bool obSuccess = false;

        public TransactDownloadOnboarding(Link link)
        {
            this.link = link;
            client.BaseAddress = new Uri(link.DownlinkBTMSURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal async Task<AbstractProject[]> GenerateReport()
        {
            List<AbstractProject> abstractProjects = new List<AbstractProject>();

            abstractProjects.AddRange(await GetTransactProjects("GetListActive"));
            abstractProjects.AddRange(await GetTransactProjects("GetListInvoicable"));
            abstractProjects.AddRange(await GetTransactProjects("GetListInvoicePending"));
            abstractProjects.AddRange(await GetTransactProjects("GetListCanceled"));

            GenerateOutput();

            return abstractProjects.ToArray();
        }

        private void GenerateOutput()
        {
            if (unmappedStatuses.Count > 0 || unmappedLanguages.Count > 0 || unmappedUnits.Count > 0 || unmappedSpecialities.Count > 0)
            {
                Log.AddLog("Unmapped Transact downlink values...", ErrorLevels.Warning);
                UnmappedReportWriter(unmappedStatuses, "status");
                UnmappedReportWriter(unmappedLanguages, "language");
                UnmappedReportWriter(unmappedUnits, "unit");
                UnmappedReportWriter(unmappedSpecialities, "speciality");
                obSuccess = true;
            }
            else
            {
                Log.AddLog("No unmapped Transact downlink values", ErrorLevels.Information);
                obSuccess = true;
            }

        }

        private void UnmappedReportWriter(List<string> unmappedValues, string valueName)
        {
            foreach (var unmappedValue in unmappedValues)
            {
                Log.AddLog("Unmapped " + valueName + " in Transact: " + unmappedValue, ErrorLevels.Warning);
            }
        }

        internal async Task<List<AbstractProject>> GetTransactProjects(string method)
        {
            List<AbstractProject> abstractProjects = new List<AbstractProject>();

            try
            {
                TransactProjectRequest tpr = new TransactProjectRequest();
                tpr.mode = "purchaseorder";
                tpr.token = link.DownlinkBTMSPassword;
                tpr.method = method;
                
                HttpResponseMessage response = await client.PostAsJsonAsync("", tpr);
                response.EnsureSuccessStatusCode();
                TransactProjectList tp = await response.Content.ReadAsAsync<TransactProjectList>();
                MapProject(tp, ref abstractProjects);
            }
            catch (Exception ex)
            {
                Log.AddLog("Transact onboarding failed for " + method + " - " + ex.Message, ErrorLevels.Error);
            }

            return abstractProjects;
        }

        private void MapProject(TransactProjectList tp, ref List<AbstractProject> abstractProjects)
        {
            foreach (TransactProject transactProject in tp.data)
            {
                try
                {
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
                            case "Denied bill acceptance":
                                abstractProject.Status = ProjectStatus.Completed;
                                break;
                            case "Completeness check started":
                                abstractProject.Status = ProjectStatus.Completed;
                                break;
                            default:
                                abstractProject.Status = ProjectStatus.Undefined;
                                break;
                        }
                    }

                    if (abstractProject.Status == ProjectStatus.Undefined)
                    {
                        if (!unmappedStatuses.Contains(transactProject.status)) unmappedStatuses.Add(transactProject.status);
                    }

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
                            abstractProject.SpecialityID = MappingManager.DoMappingToAbstract(MapType.Speciality, link.DownlinkBTMSSystemTypeID, transactProject.specialty);
                        }
                        catch (Exception ex)
                        {
                            if (!unmappedSpecialities.Contains(transactProject.specialty)) unmappedSpecialities.Add(transactProject.specialty);
                        }
                    }

                    if (!String.IsNullOrEmpty(transactProject.language_source))
                    {
                        try
                        {
                            abstractProject.SourceLanguageID = MappingManager.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemTypeID, transactProject.language_source);
                        }
                        catch (Exception ex)
                        {
                            if (!unmappedLanguages.Contains(transactProject.language_source)) unmappedLanguages.Add(transactProject.language_source);
                        }
                    }

                    if (!String.IsNullOrEmpty(transactProject.language_target))
                    {
                        try
                        {
                            abstractProject.TargetLanguageIDs.Add(MappingManager.DoMappingToAbstract(MapType.Language, link.DownlinkBTMSSystemTypeID, transactProject.language_target));
                        }
                        catch (Exception ex)
                        {
                            if (!unmappedLanguages.Contains(transactProject.language_target)) unmappedLanguages.Add(transactProject.language_target);
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
                            abstractProject.PayableUnitID = MappingManager.DoMappingToAbstract(MapType.Unit, link.DownlinkBTMSSystemTypeID, transactProject.quantity_unit);
                        }
                        catch (Exception ex)
                        {
                            if (!unmappedUnits.Contains(transactProject.quantity_unit)) unmappedUnits.Add(transactProject.quantity_unit);
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

                    abstractProjects.Add(abstractProject);

                }
                catch (Exception ex)
                {
                    Log.AddLog("Processing Transact project failed:" + ex.Message, ErrorLevels.Error);
                }
            }
        }
    }
}