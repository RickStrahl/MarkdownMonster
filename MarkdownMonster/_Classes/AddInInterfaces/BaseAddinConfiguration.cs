using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using Westwind.Utilities.Configuration;

namespace MarkdownMonster.AddIns
{
    public class BaseAddinConfiguration<T> : AppConfiguration, INotifyPropertyChanged
        where T:AppConfiguration,  new()
    {
        /// <summary>
        /// Just the file name of the configuraiton file in which to store configuration
        /// settings. This file will be auto-created in the MM Common folder.
        /// </summary>
        protected virtual string ConfigurationFilename  { get; set;  } = typeof(T).Name + ".json";

        /// <summary>
        /// The current configuration instance. 
        /// Automatically loaded on first use - always available.
        /// </summary>
        public static T Current;

        /// <summary>
        /// Static constructor sets up Current instance.
        /// </summary>
        static BaseAddinConfiguration()
        {
            Current = new T();
            Current.Initialize();            
        }
        
        /// <summary>
        /// Override default to use JsonFileConfiguraiton Provider using the ConfigurationFilename
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="configData"></param>
        /// <returns></returns>
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {            
            var provider = new JsonFileConfigurationProvider<T>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, ConfigurationFilename)
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }       


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
