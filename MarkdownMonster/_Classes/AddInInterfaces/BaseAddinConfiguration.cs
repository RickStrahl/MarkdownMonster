using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using Westwind.Utilities.Configuration;

namespace MarkdownMonster.AddIns
{

    /// <summary>
    /// Base class that can be used for holding configuration values that are 
    /// persisted between execution. Create a class that inherits from this base
    /// class and use the `Current` property to access the active instance
    /// 
    /// You can save configuration to a json file by setting `ConfigurationFilename`
    /// and calling the `.Write()` method at any point.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseAddinConfiguration<T> : AppConfiguration, INotifyPropertyChanged
        where T:AppConfiguration,  new()
    {
        /// <summary>
        /// Just the file name of the configuraiton file in which to store configuration
        /// settings. This file will be auto-created in the MM Common folder.
        /// </summary>
        protected string ConfigurationFilename  { get; set;  } = typeof(T).Name + ".json";

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

        

        /// <summary>
        /// INotifyPropertyChanged handler implementation
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// INotifyPropertyChanged handler implementation
        /// </summary>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
