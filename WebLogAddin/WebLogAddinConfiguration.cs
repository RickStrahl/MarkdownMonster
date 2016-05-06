using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using Westwind.Utilities.Configuration;

namespace WebLogAddin
{
    internal class WeblogApp
    {
        public static WeblogAddinConfiguration Configuration;

        static WeblogApp()
        {
            Configuration = new WeblogAddinConfiguration();
            Configuration.Initialize();
        }
    }

    public class WeblogAddinConfiguration : AppConfiguration
    {
        public string WeblogUsername { get; set; }
        public string WeblogPassword { get; set; }
        public string WeblogApiUrl { get; set; }


        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<WeblogAddinConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "WebLogAddIn.json")                                
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
     
    }
}