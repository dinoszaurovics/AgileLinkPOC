using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        }

        internal async Task<bool> UploadProjects()
        {
            
        }
    }
}