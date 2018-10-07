using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
            client.DefaultRequestHeaders.Add("X-AUTH-ACCESS-TOKEN", "hYC6yGmkTTxrNqPhNlr9c3HsIo");
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
                        name = project.ExternalProjectCode,
                        serviceId = Convert.ToInt32(MappingHelper.DoMappingToUplinkCustomValues(MapType.Workflow, link, project.Workflow))
                    };

                    HttpResponseMessage response = await client.PostAsJsonAsync("projects", xcprequest);
                    response.EnsureSuccessStatusCode();
                    XTRF_ProjectCreateResponse xpcresponse = await response.Content.ReadAsAsync<XTRF_ProjectCreateResponse>();
                    string newXTRFProjectID = xpcresponse.id;

                    XTRF_SetSourceLanguage xssl = new XTRF_SetSourceLanguage()
                    {
                        sourceLanguageId = Convert.ToInt32(MappingHelper.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemID, project.SourceLanguageID.ToString()))
                    };
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/sourceLanguage", xssl);

                    XTRF_SetTargetLanguage xstl = new XTRF_SetTargetLanguage();
                    foreach(int languageID in project.TargetLanguageIDs)
                    {
                        xstl.targetLanguageIds.Add(Convert.ToInt32(MappingHelper.DoMappingToUplinkGeneral(MapType.Language, link.UplinkBTMSSystemID, languageID.ToString())));
                    }
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/targetLanguages", xstl);

                    XTRF_SetSpecialization xsspec = new XTRF_SetSpecialization()
                    {
                        specializationId = Convert.ToInt32(MappingHelper.DoMappingToUplinkGeneral(MapType.Speciality, link.UplinkBTMSSystemID, project.SpecialityID.ToString()))
                    };
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/specialization", xsspec);

                    XTRF_SetValue xsv = new XTRF_SetValue();
                    
                    DateTimeOffset dto = new DateTimeOffset(project.Deadline);
                    xsv.value = dto.ToUnixTimeMilliseconds().ToString();
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/clientDeadline", xsv);

                    xsv.value = project.Instructions;
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/clientNotes", xsv);

                    // xsv.value = project.Instructions;
                    //response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/clientReferenceNumber", xsv);

                    //xsv.value = project.Instructions;
                    //response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/internalNotes", xsv);

                    //xsv.value = project.Instructions;
                    //response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/vendorInstructions", xsv);

                    xsv.value = project.PayableVolume.ToString();
                    response = await client.PutAsJsonAsync("projects/" + newXTRFProjectID + "/volume", xsv);

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

}