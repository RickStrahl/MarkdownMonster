using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Project file format that can load and save a bunch of files
    /// as a group of files.
    /// </summary>
    public class MarkdownMonsterProject : INotifyPropertyChanged
    {

        [JsonIgnore]
        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (value == _Filename) return;
                _Filename = value;
                OnPropertyChanged(nameof(Filename));
                OnPropertyChanged(nameof(ProjectPath));
                OnPropertyChanged(nameof(SaveProjectFilename));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
        private string _Filename;
        

        /// <summary>
        /// The active folder when the project is saved
        /// </summary>
        public string ActiveFolder { get; set; }

        /// <summary>
        /// Index of the sidebar that is active
        /// </summary>
        public int ActiveSidebarIndex { get; set; } = -1;

        /// <summary>
        /// A list of documents that were open when the project is saved
        /// </summary>
        public List<OpenFileDocument> OpenDocuments { get; set; } = new List<OpenFileDocument>();


        #region Internally used Properties

        [JsonIgnore]
        public string SaveProjectFilename
        {
            get
            {
                string result = "_Save Project";

                if (string.IsNullOrEmpty(Filename))
                    return result;

                return result + " " + Path.GetFileName(Filename);
            }
        }

        [JsonIgnore]
        public string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(Filename))
                    return null;

                return Path.GetDirectoryName(Filename);
            }
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get => string.IsNullOrEmpty(Filename);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="lineNumber"></param>
        /// <param name="isActive"></param>
        /// <param name="imageFolder"></param>
        /// <returns></returns>
        public OpenFileDocument AddDocuments(string filename,
            int lineNumber = 0,
            bool isActive = false,
            string imageFolder = null)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            if (!File.Exists(filename))
                return null;

            var doc = new OpenFileDocument() {
                Filename = filename,
                LastEditorLineNumber = lineNumber,
                IsActive = isActive,
                LastImageFolder = imageFolder
            };

            OpenDocuments.Add(doc);

            return doc;
        }



        /// <summary>
        /// Loads a new project
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static MarkdownMonsterProject Load(string filename)
        {
            if (!File.Exists(filename))
                return null;

            
            var project =  JsonSerializationUtils.DeserializeFromFile(filename,
                                                              typeof(MarkdownMonsterProject))
                                                              as MarkdownMonsterProject;
            if (project == null)
                return null;

            project.Filename = filename;

            return project;
        }

        /// <summary>
        /// Saves a project to disk
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;
            else
                Filename = filename;

            return JsonSerializationUtils.SerializeToFile(this, filename, formatJsonOutput: true);
        }
        

        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

   
}
