using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebLogAddin;

namespace WeblogAddin.Test
{
    [TestClass]
    public class BlogEndpointDiscoveryTests
    {
        [TestMethod]
        public void DiscoverWordpress()
        {
            string discoveryUrl = "https://rickstrahl.wordpress.com";
            RsdToUrlTest(discoveryUrl);
        }

        [TestMethod]
        public void DiscoverCodeplex()
        {
            string discoveryUrl = "https://markdownmonstertest.codeplex.com/";
            RsdToUrlTest(discoveryUrl);
        }

        [TestMethod]
        public void DiscoverFSharpForCSharpDevelopers()
        {
            string discoveryUrl = "http://fsharpforcsharpdevelopers.com/";
            RsdToUrlTest(discoveryUrl);
        }

        [TestMethod]
        public async void DiscoverFSharpForWestWindBlogNoRDS()
        {
            // no RDS
            string discoveryUrl = "http://weblog.west-wind.com/metaweblogapi.ashx";

            var discover = new BlogEndpointDiscovery();
            BlogApiEndpoint ep = await discover.DiscoverBlogEndpointAsync(discoveryUrl, "1", "MetaWeblog");


            Assert.IsNotNull(ep);
            Console.WriteLine(ep.ApiUrl + " " + ep.BlogType + " - " + ep.BlogId + "\r\n" + ep.Rsd);
            Assert.AreEqual(discoveryUrl, ep.ApiUrl);
        }


        private async void RsdToUrlTest(string discoveryUrl)
        {

            var discover = new BlogEndpointDiscovery();
            BlogApiEndpoint ep = await discover.DiscoverBlogEndpointAsync(discoveryUrl,"1","MetaWeblog");

            Assert.IsNotNull(ep);
            Console.WriteLine(ep.ApiUrl + " " + ep.BlogType + " - " + ep.BlogId + "\r\n" + ep.Rsd );
            Assert.AreNotEqual(discoveryUrl, ep.ApiUrl);
        }


        [TestMethod]
        public async void CheckRpcEndPointCodeplex()
        {
            string endpoint = "https://www.codeplex.com/site/metaweblog";
            var discover = new BlogEndpointDiscovery();

            Assert.IsTrue(await discover.CheckRpcEndpointAsync(endpoint));

        }

        [TestMethod]
        public async void CheckRpcEndPointWordpress()
        {
            string endpoint = "https://rickstrahl.wordpress.com/xmlrpc.php";
            var discover = new BlogEndpointDiscovery();

            Assert.IsTrue(await discover.CheckRpcEndpointAsync(endpoint));

        }

    }
}
