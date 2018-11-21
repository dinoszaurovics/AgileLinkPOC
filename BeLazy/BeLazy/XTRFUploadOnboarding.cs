using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XTRFJSON;
using System.Linq;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace BeLazy
{
    internal class XTRFUploadOnboarding
    {
        private static HttpClient client = new HttpClient();
        internal bool obSuccess = false;
        private Link link;
        private AbstractProject[] projects;
        List<string> unmappedWorkflows = new List<string>();
        List<string> unmappedLanguages = new List<string>();
        List<string> unmappedSpecialities = new List<string>();
        List<string> unmappedPMs = new List<string>();

        public XTRFUploadOnboarding(Link link, AbstractProject[] projects)
        {
            this.link = link;
            this.projects = projects;
            client.BaseAddress = new Uri(link.UplinkBTMSURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.xtrf-v1+json"));
            client.DefaultRequestHeaders.Add("X-AUTH-ACCESS-TOKEN", link.UplinkBTMSPassword);
        }

        internal async Task GenerateReport()
        {
            string clientID = link.ClientIDForUplinkProject;

            List<string> checkMappingWorkflows = new List<string>();
            List<string> checkMappingLanguages = new List<string>();
            List<string> checkMappingSpecialities = new List<string>();
            List<string> checkMappingPMs = new List<string>();

            foreach (AbstractProject abstractProject in projects)
            {
                try
                {
                    string value = MappingManager.GetScriptedValue(link, MapType.Workflow, abstractProject);
                    if (!checkMappingWorkflows.Contains(value)) checkMappingWorkflows.Add(value);

                    if (!checkMappingLanguages.Contains(abstractProject.SourceLanguageID.ToString()))
                        checkMappingLanguages.Add(abstractProject.SourceLanguageID.ToString());

                    foreach (int languageID in abstractProject.TargetLanguageIDs)
                    {
                        if (!checkMappingLanguages.Contains(languageID.ToString()))
                            checkMappingLanguages.Add(languageID.ToString());
                    }

                    if (!checkMappingSpecialities.Contains(abstractProject.SpecialityID.ToString()))
                        checkMappingSpecialities.Add(abstractProject.SpecialityID.ToString());

                    if (!checkMappingPMs.Contains(abstractProject.ExternalProjectManagerName))
                        checkMappingPMs.Add(abstractProject.ExternalProjectManagerName);
                }
                catch (Exception ex)
                {
                    Log.AddLog("Mapping check for XTRF failed for project: " + abstractProject.ExternalProjectCode, ErrorLevels.Error);
                }
            }

            Log.AddLog("XTRF uplink onboarding succesfully processed abstract projects", ErrorLevels.Information);

            Dictionary<string, string> checkXTRFValuesWorkflows = new Dictionary<string, string>();
            Dictionary<string, string> checkXTRFValuesLanguages = new Dictionary<string, string>();
            Dictionary<string, string> checkXTRFValuesPMs = new Dictionary<string, string>();
            Dictionary<string, string> checkXTRFValuesSpecialities = new Dictionary<string, string>();

            foreach (var wfItem in checkMappingWorkflows)
            {
                try
                {
                    string value = MappingManager.DoMappingToUplinkCustomValues(MapType.Workflow, link, wfItem);
                    if (!checkXTRFValuesWorkflows.ContainsKey(value)) checkXTRFValuesWorkflows.Add(value, wfItem);
                }
                catch (Exception ex)
                {
                    if (!unmappedWorkflows.Contains(wfItem)) unmappedWorkflows.Add(wfItem);
                }
            }

            foreach (var langItem in checkMappingLanguages)
            {
                try
                {
                    string value = MappingManager.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemTypeID, langItem).ToString();
                    if (!checkXTRFValuesLanguages.ContainsKey(value)) checkXTRFValuesLanguages.Add(value, langItem);
                }
                catch (Exception ex)
                {
                    if (!unmappedLanguages.Contains(langItem)) unmappedLanguages.Add(langItem);
                }
            }

            foreach (var spItem in checkMappingSpecialities)
            {
                try
                {
                    string value = MappingManager.DoMappingToUplinkGeneral(MapType.Speciality, link.UplinkBTMSSystemTypeID, spItem).ToString();
                    if (!checkXTRFValuesSpecialities.ContainsKey(value)) checkXTRFValuesSpecialities.Add(value, spItem);
                }
                catch (Exception ex)
                {
                    if (!unmappedSpecialities.Contains(spItem)) unmappedSpecialities.Add(spItem);
                }
            }

            foreach (var pmItem in checkMappingPMs)
            {
                try
                {
                    string value = MappingManager.DoMappingToUplinkCustomValues(MapType.Contact, link, pmItem);
                    if (!checkXTRFValuesPMs.ContainsKey(value)) checkXTRFValuesPMs.Add(value, pmItem);
                }
                catch (Exception ex)
                {
                    if (!unmappedPMs.Contains(pmItem)) unmappedPMs.Add(pmItem);
                }
            }

            Log.AddLog("XTRF uplink onbboarding - SQL mapping check succesful.", ErrorLevels.Information);

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync("dictionaries/language/active");
                response.EnsureSuccessStatusCode();
                string xllJson = await response.Content.ReadAsStringAsync();
                List<XTRF_Language> xllObj = JsonConvert.DeserializeObject<List<XTRF_Language>>(xllJson);
                List<int> activeXTRFLangs = xllObj.Select(x => x.id).ToList();

                foreach (string languageItem in checkXTRFValuesLanguages.Keys)
                {
                    if (!activeXTRFLangs.Contains(Convert.ToInt32(languageItem)))
                    {
                        unmappedLanguages.Add(checkXTRFValuesLanguages[languageItem] + " - " + languageItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddLog("Error processing XTRF language mappings. " + ex.Message, ErrorLevels.Error);
            }

            try
            {
                response = await client.GetAsync("dictionaries/specialization/active");
                response.EnsureSuccessStatusCode();
                string xslJson = await response.Content.ReadAsStringAsync();
                List<XTRF_DictionaryItem> xslObj = JsonConvert.DeserializeObject<List<XTRF_DictionaryItem>>(xslJson);
                List<int> activeXTRFSpecializations = xslObj.Select(x => x.id).ToList();

                foreach (string specializationItem in checkXTRFValuesSpecialities.Keys)
                {
                    if (!activeXTRFSpecializations.Contains(Convert.ToInt32(specializationItem)))
                    {
                        unmappedSpecialities.Add(checkXTRFValuesSpecialities[specializationItem] + " - " + specializationItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddLog("Error processing XTRF specialization mappings. " + ex.Message, ErrorLevels.Error);
            }

            try
            {
                response = await client.GetAsync("services/active");
                response.EnsureSuccessStatusCode();
                string xservlJson = await response.Content.ReadAsStringAsync();
                List<XTRF_DictionaryItem> xservlObj = JsonConvert.DeserializeObject<List<XTRF_DictionaryItem>>(xservlJson);
                List<int> activeXTRFServices = xservlObj.Select(x => x.id).ToList();

                foreach (string serviceItem in checkXTRFValuesWorkflows.Keys)
                {
                    if (!activeXTRFServices.Contains(Convert.ToInt32(serviceItem)))
                    {
                        unmappedWorkflows.Add(checkXTRFValuesWorkflows[serviceItem] + " - " + serviceItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddLog("Error processing XTRF services (workflow) mappings. " + ex.Message, ErrorLevels.Error);
            }

            try
            {
                foreach (var contactID in checkXTRFValuesPMs.Keys)
                {
                    try
                    {
                        response = await client.GetAsync("customers/persons/" + contactID);
                        response.EnsureSuccessStatusCode();
                        string xcJson = await response.Content.ReadAsStringAsync();
                        XTRF_Contact xc = JsonConvert.DeserializeObject<XTRF_Contact>(xcJson);
                        if (xc.customerId.ToString() != clientID)
                        {
                            unmappedPMs.Add(checkXTRFValuesPMs[contactID] + " - " + contactID);
                        }
                    }
                    catch (Exception ex)
                    {
                        unmappedPMs.Add(checkXTRFValuesPMs[contactID] + " - " + contactID);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddLog("Error processing XTRF client PM mappings. " + ex.Message, ErrorLevels.Error);
            }

            Log.AddLog("Generating output for XTRF uplink onboarding.", ErrorLevels.Information);
            GenerateOutput();
        }

        private void GenerateOutput()
        {
            if (unmappedWorkflows.Count > 0 || unmappedLanguages.Count > 0 || unmappedPMs.Count > 0 || unmappedSpecialities.Count > 0)
            {
                Log.AddLog("Unmapped XTRF uplink values...", ErrorLevels.Warning);
                UnmappedReportWriter(unmappedWorkflows, "service");
                UnmappedReportWriter(unmappedLanguages, "language");
                UnmappedReportWriter(unmappedPMs, "client PM");
                UnmappedReportWriter(unmappedSpecialities, "speciality");
                obSuccess = false;
            }
            else
            {
                obSuccess = true;
            }
        }

        private void UnmappedReportWriter(List<string> unmappedValues, string valueName)
        {
            foreach (var unmappedValue in unmappedValues)
            {
                Log.AddLog("Unmapped " + valueName + " in XTRF: " + unmappedValue, ErrorLevels.Warning);
            }
        }
    }
}
