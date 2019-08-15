using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Configuration;
using Westwind.TypeImporter;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.ConfigurationEditor
{
    /// <summary>
    /// Parses Configuration Classes into an object
    /// that includes information about each configuration switch and is
    /// searchable
    /// </summary>
    public class ConfigurationParser
    {

        public List<DotnetObject> ConfigObjects = new List<DotnetObject>();

        public ObservableCollection<ObjectProperty> FilteredProperties = new ObservableCollection<ObjectProperty>();
        
        public DotnetObject ParseConfigurationObject(Type appConfigType, bool addToCollection = false)
        {
            
            var typeParser = new Westwind.TypeImporter.TypeParser()
            {
                ParseXmlDocumentation = true
            };
            var dotnetObject = typeParser.ParseObject(appConfigType);
            if (dotnetObject == null)
            {
                return null;

            }

            if (addToCollection)
                ConfigObjects.Add(dotnetObject);

            return dotnetObject;
        }

        public void ParseAllConfigurationObjects()
        {
            ParseConfigurationObject(typeof(ApplicationConfiguration),true);
            ParseConfigurationObject(typeof(EditorConfiguration), true);
            ParseConfigurationObject(typeof(MarkdownOptionsConfiguration), true);
            ParseConfigurationObject(typeof(GitConfiguration), true);
            ParseConfigurationObject(typeof(FolderBrowserConfiguration), true);
            ParseConfigurationObject(typeof(ImageConfiguration), true);
            ParseConfigurationObject(typeof(WindowPositionConfiguration), true);
            ParseConfigurationObject(typeof(ApplicationUpdatesConfiguration), true);
        }

    

        public List<ConfigurationPropertyItem> FindProperty(string textToFind)
        {
            var list = new List<ConfigurationPropertyItem>();

            foreach (var obj in ConfigObjects)
            {
                foreach (var prop in obj.Properties)
                {

                    if (!prop.Name.Contains(textToFind, StringComparison.InvariantCultureIgnoreCase) &&
                        !prop.HelpText.Contains(textToFind, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var item = new ConfigurationPropertyItem
                    {
                        Property = prop,
                        Section = obj.Name
                    };
                    item.Section = StringUtils.FromCamelCase(item.Section.Replace("Configuration",""));

                    list.Add(item);
                }
            }

            return list;
        }


        #region Errors
        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
        }


        #endregion
    }

    public class ConfigurationPropertyItem
    {
        public ObjectProperty Property { get; set; }
        public string Section { get; set;  }
    }


}
