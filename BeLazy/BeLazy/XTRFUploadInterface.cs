using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XTRFJSON;

namespace BeLazy
{
    internal class XTRFUploadInterface
    {

        private static HttpClient client = new HttpClient();
        private Link link;
        private AbstractProject[] projects;

        public XTRFUploadInterface(Link link, AbstractProject[] projects)
        {
            this.link = link;
            this.projects = projects;
            client.BaseAddress = new Uri(link.UplinkBTMSURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.xtrf-v1+json"));
            client.DefaultRequestHeaders.Add("X-AUTH-ACCESS-TOKEN", link.UplinkBTMSPassword);
        }

        internal async Task<bool> UploadProjects()
        {
            string clientID = link.ClientIDForUplinkProject;

            foreach (AbstractProject abstractProject in projects)
            {
                Log.AddLog("Adding project to XTRF: " + abstractProject.ExternalProjectCode, ErrorLevels.Information);

                try
                {
                    HttpResponseMessage response;
                    string newXTRFProjectID = "";
                    string newXTRFProjectIDCode = "";

                    XTRF_CreateProjectRequest xcprequest = new XTRF_CreateProjectRequest()
                    {
                        clientId = Int32.Parse(clientID),
                        name = MappingManager.GetScriptedValue(link, MapType.ProjectName, abstractProject),
                        serviceId = Convert.ToInt32(MappingManager.DoMappingToUplinkCustomValues(MapType.Workflow, link, MappingManager.GetScriptedValue(link, MapType.Workflow, abstractProject)))
                    };

                    try
                    {
                        response = await client.PostAsJsonAsync("v2/projects", xcprequest);
                        response.EnsureSuccessStatusCode();
                        XTRF_ProjectCreateResponse xpcresponse = await response.Content.ReadAsAsync<XTRF_ProjectCreateResponse>();
                        newXTRFProjectID = xpcresponse.id;
                        newXTRFProjectIDCode = xpcresponse.projectIdNumber;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("XTRF project creation failed. " + ex.Message);
                    }

                    try
                    {
                        XTRF_SetSourceLanguage xssl = new XTRF_SetSourceLanguage()
                        {
                            sourceLanguageId = Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemTypeID, abstractProject.SourceLanguageID.ToString()))
                        };
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/sourceLanguage", xssl);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project source language (" + abstractProject.SourceLanguageID.ToString() + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        XTRF_SetTargetLanguage xstl = new XTRF_SetTargetLanguage();
                        foreach (int languageID in abstractProject.TargetLanguageIDs)
                        {
                            try
                            {
                                xstl.targetLanguageIds.Add(Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemTypeID, languageID.ToString())));
                            }
                            catch (Exception ex)
                            {
                                Log.AddLog("XTRF project target language is not mapped (" + languageID.ToString() + " ). " + ex.Message, ErrorLevels.Error);
                            }
                        }

                        if (xstl.targetLanguageIds.Count > 0)
                        {
                            response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/targetLanguages", xstl);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project target language setting error. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        XTRF_SetSpecialization xsspec = new XTRF_SetSpecialization()
                        {
                            specializationId = Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Speciality, link.UplinkBTMSSystemTypeID, abstractProject.SpecialityID.ToString()))
                        };
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/specialization", xsspec);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project specialization (" + abstractProject.SpecialityID.ToString() + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        XTRF_SetClientContact xscc = new XTRF_SetClientContact();
                        xscc.primaryId = Convert.ToInt32(MappingManager.DoMappingToUplinkCustomValues(MapType.Contact, link, abstractProject.ExternalProjectManagerName));
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientContacts", xscc);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project client PM (" + abstractProject.ExternalProjectManagerName + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    XTRF_SetValue xsv = new XTRF_SetValue();
                    DateTimeOffset dto;

                    try
                    {
                        dto = new DateTimeOffset(abstractProject.Deadline);
                        xsv.value = dto.ToUnixTimeMilliseconds().ToString();
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientDeadline", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project deadline (" + abstractProject.Deadline.ToString() + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        dto = new DateTimeOffset(abstractProject.DateOrdered);
                        xsv.value = dto.ToUnixTimeMilliseconds().ToString();
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/orderDate", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project order date (" + abstractProject.DateOrdered.ToString() + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        xsv.value = abstractProject.ClientNotes;
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientNotes", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project client notes (" + abstractProject.ClientNotes + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        xsv.value = abstractProject.ExternalProjectCode;
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientReferenceNumber", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project client reference number (" + abstractProject.ExternalProjectCode + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        xsv.value = abstractProject.PMNotes;
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/internalNotes", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project PM notes (" + abstractProject.PMNotes + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        xsv.value = abstractProject.VendorNotes;
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/vendorInstructions", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project vendor instructions (" + abstractProject.VendorNotes + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        xsv.value = abstractProject.PayableVolume.ToString();
                        response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/volume", xsv);
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project volume (" + abstractProject.PayableVolume.ToString() + " ) cannot be set. " + ex.Message, ErrorLevels.Error);
                    }

                    try
                    {
                        try
                        {
                            xsv.value = "";

                            foreach (var analysisCategory in abstractProject.AnalysisCategories)
                            {
                                if (xsv.value != "") xsv.value += "¤";
                                xsv.value += ((analysisCategory.StartPc == -1) ? "Rep" : ("F" + analysisCategory.StartPc)) + ";"
                                    + analysisCategory.WordCount + ";" + analysisCategory.Weight;
                            }
                            response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/customFields/BeLazyField", xsv);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("XTRF project custom field cannot be set. " + ex.Message);
                        }

                        response = await client.GetAsync("browser?viewId=58&q.idNumber=" + newXTRFProjectIDCode);  // This view ID (58) is custom, system specific
                        response.EnsureSuccessStatusCode();
                        string projectList = await response.Content.ReadAsStringAsync();
                        Regex internalProjectIDMatcher = new Regex(@"(?<=\s*""rows""\s*:\s*{\s*""\d+""\s*:\s*{\s*""id""\s*:\s*)(\d+)(?=,)");
                        if (internalProjectIDMatcher.IsMatch(projectList))
                        {
                            int internalProjectID = Convert.ToInt32(internalProjectIDMatcher.Match(projectList).Value);
                            XTRF_RunMacro xrm = new XTRF_RunMacro();
                            xrm.ids.Add(internalProjectID);
                            response = await client.PostAsJsonAsync("macros/4/run", xrm);  // This macro ID (4) is custom, system specific
                            string akarmi = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("XTRF project macro cannot be run." + ex.Message, ErrorLevels.Error);
                    }

                }
                catch (Exception ex)
                {
                    Log.AddLog("Error while creating XTRF project. " + ex.Message, ErrorLevels.Error);
                }
            }

            return true;
        }

    }
}

namespace XTRFJSON
{
    public class XTRF_CreateProjectRequest
    {
        public int clientId { get; set; }
        public string name { get; set; }
        public int serviceId { get; set; }
    }

    public class Volume
    {
        public object value { get; set; }
        public int unitId { get; set; }
    }

    public class Languages
    {
        public string sourceLanguageId { get; set; }
        public List<string> targetLanguageIds { get; set; }
        public int specializationId { get; set; }
        public List<string> languageCombinations { get; set; }

        public Languages()
        {
            targetLanguageIds = new List<string>();
            languageCombinations = new List<string>();
        }
    }

    public class Documents
    {
        public string projectConfirmationStatus { get; set; }
    }

    public class People
    {
        public int projectManagerId { get; set; }
    }

    public class XTRF_ProjectCreateResponse
    {
        public string id { get; set; }
        public string projectId { get; set; }
        public bool isClassicProject { get; set; }
        public string quoteIdNumber { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public List<string> categoryIds { get; set; }
        public string budgetCode { get; set; }
        public int clientId { get; set; }
        public int serviceId { get; set; }
        public string origin { get; set; }
        public string clientDeadline { get; set; }
        public string clientReferenceNumber { get; set; }
        public string clientNotes { get; set; }
        public string internalNotes { get; set; }
        public Volume volume { get; set; }
        public Languages languages { get; set; }
        public Documents documents { get; set; }
        public People people { get; set; }
        public string instructionsForAllJobs { get; set; }
        public string projectIdNumber { get; set; }
        public long orderedOn { get; set; }

        public XTRF_ProjectCreateResponse()
        {
            categoryIds = new List<string>();
        }
    }

    public class XTRF_SetSourceLanguage
    {
        public int sourceLanguageId { get; set; }
    }

    public class XTRF_SetSpecialization
    {
        public int specializationId { get; set; }
    }

    public class XTRF_SetTargetLanguage
    {
        public List<int> targetLanguageIds { get; set; }

        public XTRF_SetTargetLanguage()
        {
            targetLanguageIds = new List<int>();
        }
    }

    public class XTRF_SetValue
    {
        public string value { get; set; }
    }

    public class XTRF_SetClientContact
    {
        public int primaryId { get; set; }
        public List<int> additionalIds { get; set; }

        public XTRF_SetClientContact()
        {
            additionalIds = new List<int>();
        }
    }

    public class XTRF_RunMacro
    {
        public bool async = false; // It would be the macro being run async within XTRF, I do not know how that would work, and how to track
        public List<int> ids { get; set; }

        public XTRF_RunMacro()
        {
            ids = new List<int>();
        }
    }

    public class XTRF_LanguageList
    {
        public List<XTRF_Language> languages;

        public XTRF_LanguageList()
        {
            languages = new List<XTRF_Language>();
        }
    }

    public class XTRF_Language
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public bool preferred { get; set; }
        public string symbol { get; set; }
        public string iso6391 { get; set; }
        public string iso6392 { get; set; }
        public bool @default { get; set; }
    }

    public class XTRF_SpecializationList
    {
        public List<XTRF_DictionaryItem> specializations;

        public XTRF_SpecializationList()
        {
            specializations = new List<XTRF_DictionaryItem>();
        }
    }

    public class XTRF_ServicesList
    {
        public List<XTRF_DictionaryItem> services;

        public XTRF_ServicesList()
        {
            services = new List<XTRF_DictionaryItem>();
        }
    }
    
    public class XTRF_DictionaryItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public bool preferred { get; set; }
        public bool @default { get; set; }
    }

    public class Contact_Emails
    {
        public string primary { get; set; }
        public List<string> additional { get; set; }

        public Contact_Emails()
        {
            additional = new List<string>();
        }
    }

    public class ContactDetails
    {
        public List<string> phones { get; set; }
        public string sms { get; set; }
        public string fax { get; set; }
        public Contact_Emails emails { get; set; }

        public ContactDetails()
        {
            phones = new List<string>();
        }
    }

    public class XTRF_Contact
    {
        public int id { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public ContactDetails contact { get; set; }
        public string positionId { get; set; }
        public string gender { get; set; }
        public bool active { get; set; }
        public List<string> motherTonguesIds { get; set; }
        public List<string> customFields { get; set; }
        public int customerId { get; set; }

        public XTRF_Contact()
        {
            motherTonguesIds = new List<string>();
            customFields = new List<string>();
        }
    }

}