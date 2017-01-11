using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebLogAddin;

namespace WeblogAddin.Test
{
    [TestClass]
    public class BlogEndpointDiscoveryTests
    {
        [TestMethod]
        public  void RsdToUrlTest()
        {
            string discoveryUrl = "http://fsharpforcsharpdevelopers.com/";
            //string discoveryUrl = "https://rickstrahl.wordpress.com";

            var discover = new BlogEndpointDiscovery();
            BlogApiEndpoint ep = discover.DiscoverBlogEndpoint(discoveryUrl,"1","MetaWeblog");
            
            Assert.IsNotNull(ep);
            Console.WriteLine(ep.ApiUrl + " " + ep.BlogType + " - " + ep.BlogId + "\r\n" + ep.Rsd );
            Assert.AreNotEqual(discoveryUrl, ep.ApiUrl);
        }


    }
}
