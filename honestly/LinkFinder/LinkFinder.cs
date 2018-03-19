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
using System;

namespace honestly
{
    public static class LinkFinder
    {
        /// <summary>
        /// Use this function to retrieve all anchor tags HREF attribute values from a webpage
        /// Install HTTPie: https://httpie.org/
        /// Use HTTPie to make a POST request: https://httpie.org/doc#http-method
        /// C:\>http POST http://localhost:7071/api/LinkFinder url="http://www.webeccentric.com"
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns>A collection of URLs in JSON format</returns>
        [FunctionName("LinkFinder")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // Get request body
                dynamic data = req.Content.ReadAsAsync<object>().Result;

                // Set url to query string or body data
                string url_value = data?.url;

                string content = null;
                ArrayList list = new ArrayList();

                log.Info(String.Format("LinkFinder: Requesting Url {0}", url_value));

                if(!String.IsNullOrEmpty(url_value))
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] page_data = client.DownloadData(new System.Uri(url_value));

                        UTF8Encoding utf8 = new UTF8Encoding();

                        content = utf8.GetString(page_data);
                    }

                    log.Info("LinkFinder: Loading content into an HTML document");

                    var document = new HtmlDocument();
                    document.LoadHtml(content);

                    var nodes = document.DocumentNode.Descendants("a");

                    log.Info(String.Format("LinkFinder: Locating HREF nodes. Count {0}", nodes.Count()));

                    if (nodes != null && nodes.Count() > 0)
                    {
                        foreach (HtmlNode node in nodes)
                        {
                            if (node.Attributes["href"] != null && !String.IsNullOrEmpty(node.Attributes["href"].Value))
                                list.Add(node.Attributes["href"].Value);
                        }
                    }

                    int _count = list.Count;                    
                }
                return req.CreateResponse(HttpStatusCode.OK, list);
            }
            catch(Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.OK, String.Format("Something went wrong while I was doing some work for you!. Error message: {0}", ex.Message));
            }           
        }
    }
}
