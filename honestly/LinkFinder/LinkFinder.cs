using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using HtmlAgilityPack;

namespace honestly
{
    public static class LinkFinder
    {
        /// <summary>
        /// Use HTTPie to make a POST request: https://httpie.org/doc#http-method
        /// C:\>http POST http://localhost:7071/api/LinkFinder url="http://www.webeccentric.com"
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("LinkFinder")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            dynamic data = req.Content.ReadAsAsync<object>().Result;

            // Set url to query string or body data
            string url_value = data?.url;

            string content = null;
            ArrayList list = new ArrayList();

            using (WebClient client = new WebClient())
            {
                byte[] page_data = client.DownloadData(new System.Uri(url_value));

                UTF8Encoding utf8 = new UTF8Encoding();

                content = utf8.GetString(page_data);                
            }

            var document = new HtmlDocument();
            document.LoadHtml(content);

            var nodes = document.DocumentNode.Descendants("a");

            foreach (HtmlNode node in nodes)
            {
                list.Add(node.Attributes["href"].Value);
            }

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, list);
        }
    }
}
