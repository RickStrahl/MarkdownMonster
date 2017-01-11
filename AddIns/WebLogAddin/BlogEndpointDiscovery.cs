using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Westwind.Utilities;

namespace WebLogAddin
{
    public class BlogEndpointDiscovery
    {

        /// <summary>
        /// This method uses RSD discovery to attempt to find a
        /// URL endpoint to a blog:
        /// * <link rel='EditURI' />
        /// * RSD data document
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public BlogApiEndpoint DiscoverBlogEndpoint(string url, string blogId, string blogType)
        {
            string apiLink = url;

            // if an error occurs just return original data
            var result = new BlogApiEndpoint(apiLink, blogId, blogType);

            var settings = new HttpRequestSettings()
            {
                Url = url
            };
            settings.Headers.Add("User-Agent",
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            settings.Headers.Add("Accept",
                "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

            string html = HttpUtils.HttpRequestString(settings);

            // if not HTML assume we are on the endpoint
            if (!settings.Response.ContentType.StartsWith("text/html"))
                return result;

            var doc = new HtmlDocument();
            try
            {                
                doc.LoadHtml(html);
            }
            catch (Exception ex)
            {                
                result.HasError = true;
                return result;    
            }

            var nodes = doc.DocumentNode.SelectNodes("//link[@rel='EditURI']");
            if (nodes == null || nodes.Count < 1)
            {
                result.HasError = true;
                return result;
            }

            string rsdUrl = nodes[0].GetAttributeValue("href", null);
            if (string.IsNullOrEmpty(rsdUrl))
            {
                result.HasError = true;
                return result;
            }
            
            settings.Url = rsdUrl;
            string rsd = HttpUtils.HttpRequestString(settings);
            
            XNamespace ns = "http://archipelago.phrasewise.com/rsd";

            var xdoc = XDocument.Parse(rsd);
            var apis = xdoc.Descendants(ns + "api");

            var preferredApi = apis.FirstOrDefault(v => v.Attribute("preferred").Value == "true");
            if (preferredApi == null ||
                preferredApi.Attribute("name").Value != "MetaWeblog" &&
                preferredApi.Attribute("name").Value != "WordPress")
            {
                foreach (var api in apis)
                {
                    string name = api.Attribute("name").Value;
                    string preferred = api.Attribute("preferred").Value;
                    string blogIdXml = api.Attribute("blogID").Value;
                    string apiLinkXml = api.Attribute("apiLink").Value;

                    if (name == "WordPress")
                    {
                        apiLink = apiLinkXml;
                        blogId = blogIdXml;
                        blogType = name;
                        break;
                    }
                    if (name == "MetaWeblog")
                    {
                        apiLink = apiLinkXml;
                        blogId = blogIdXml;
                        blogType = name;
                        break;
                    }
                }
            }
            else
            {
                apiLink = preferredApi.Attribute("apiLink").Value;
                blogId = preferredApi.Attribute("blogID").Value;
            }

            return new BlogApiEndpoint(apiLink, blogId, blogType, rsd);
        }
    }

    public class BlogApiEndpoint
    {
        public BlogApiEndpoint(string apiUrl , string blogId, string blogType, string rsd = null )
        {
            ApiUrl = apiUrl;
            BlogId = blogId;
            Rsd = rsd;
            BlogType = blogType;
            
        }

        public  string ApiUrl;
        public string BlogId;
        public string BlogType;
        public string Rsd;
        public bool HasError;
    }
}
