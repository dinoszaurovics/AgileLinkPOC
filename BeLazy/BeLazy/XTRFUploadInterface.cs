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

        static HttpClient client = new HttpClient();
        private Link link;
        private Project[] projects;

        public XTRFUploadInterface(Link link, Project[] projects)
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

            foreach (Project project in projects)
            {
                try
                {
                    XTRF_CreateProjectRequest xcprequest = new XTRF_CreateProjectRequest()
                    {
                        clientId = Int32.Parse(clientID),
                        name = MappingManager.GetScriptedValue(link, MapType.ProjectName, project),
                        serviceId = Convert.ToInt32(MappingManager.DoMappingToUplinkCustomValues(MapType.Workflow, link, MappingManager.GetScriptedValue(link, MapType.Workflow, project)))
                    };

                    HttpResponseMessage response = await client.PostAsJsonAsync("v2/projects", xcprequest);
                    response.EnsureSuccessStatusCode();
                    XTRF_ProjectCreateResponse xpcresponse = await response.Content.ReadAsAsync<XTRF_ProjectCreateResponse>();
                    string newXTRFProjectID = xpcresponse.id;
                    string newXTRFProjectIDCode = xpcresponse.projectIdNumber;

                    XTRF_SetSourceLanguage xssl = new XTRF_SetSourceLanguage()
                    {
                        sourceLanguageId = Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemID, project.SourceLanguageID.ToString()))
                    };
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/sourceLanguage", xssl);

                    XTRF_SetTargetLanguage xstl = new XTRF_SetTargetLanguage();
                    foreach(int languageID in project.TargetLanguageIDs)
                    {
                        xstl.targetLanguageIds.Add(Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemID, languageID.ToString())));
                    }
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/targetLanguages", xstl);

                    XTRF_SetSpecialization xsspec = new XTRF_SetSpecialization()
                    {
                        specializationId = Convert.ToInt32(MappingManager.DoMappingToUplinkGeneral(MapType.Speciality, link.UplinkBTMSSystemID, project.SpecialityID.ToString()))
                    };
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/specialization", xsspec);

                    XTRF_SetClientContact xscc = new XTRF_SetClientContact();
                    xscc.primaryId = Convert.ToInt32(MappingManager.DoMappingToUplinkCustomValues(MapType.Contact, link, project.ExternalProjectManagerName));
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientContacts", xscc);

                    XTRF_SetValue xsv = new XTRF_SetValue();
                    
                    DateTimeOffset dto = new DateTimeOffset(project.Deadline);
                    xsv.value = dto.ToUnixTimeMilliseconds().ToString();
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientDeadline", xsv);

                    dto = new DateTimeOffset(project.DateOrdered);
                    xsv.value = dto.ToUnixTimeMilliseconds().ToString();
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/orderDate", xsv);

                    xsv.value = project.ClientNotes;
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientNotes", xsv);

                    xsv.value = project.ExternalProjectCode;
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/clientReferenceNumber", xsv);

                    xsv.value = project.PMNotes;
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/internalNotes", xsv);

                    xsv.value = project.VendorNotes;
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/vendorInstructions", xsv);

                    xsv.value = project.PayableVolume.ToString();
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/volume", xsv);

                    xsv.value = "";

                    foreach (var analysisCategory in project.AnalysisCategories)
                    {
                        if (xsv.value != "") xsv.value += "¤";
                        xsv.value += ((analysisCategory.StartPc == -1) ? "Rep":("F" + analysisCategory.StartPc)) + ";" 
                            + analysisCategory.WordCount + ";" + analysisCategory.Weight;
                    }
                    response = await client.PutAsJsonAsync("v2/projects/" + newXTRFProjectID + "/customFields/BeLazyField", xsv);

                    
                    response = await client.GetAsync("browser?viewId=58&q.idNumber=" + newXTRFProjectIDCode);
                    response.EnsureSuccessStatusCode();
                    string projectList = await response.Content.ReadAsStringAsync();
                    Regex internalProjectIDMatcher = new Regex(@"(?<=\s*""rows""\s*:\s*{\s*""\d+""\s*:\s*{\s*""id""\s*:\s*)(\d+)(?=,)");
                    if(internalProjectIDMatcher.IsMatch(projectList))
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
                    Log.AddLog("XTRF project creation failed. " + ex.Message, ErrorLevels.Error);
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

}