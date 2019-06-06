using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster._Classes
{

    public class MarkdownMonsterProject : INotifyPropertyChanged
    {
        public string Title { get; set; }


        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (value == _Filename) return;
                _Filename = value;
                OnPropertyChanged(nameof(Filename));
                OnPropertyChanged(nameof(ProjectPath));
            }
        }

        private string _Filename;


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

        public List<OpenMarkdownDocument> OpenDocuments { get; set; } = new List<OpenMarkdownDocument>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="lineNumber"></param>
        /// <param name="isActive"></param>
        /// <param name="imageFolder"></param>
        /// <returns></returns>
        public OpenMarkdownDocument AddDocuments(string filename,
            int lineNumber = 0,
            bool isActive = false,
            string imageFolder = null)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            if (!File.Exists(filename))
                return null;

            var doc = new OpenMarkdownDocument() {
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
            return JsonSerializationUtils.DeserializeFromFile(filename,
                                                              typeof(MarkdownMonsterProject))
                                                              as MarkdownMonsterProject;
        }

        /// <summary>
        /// Saves a project to disk
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename = null)
        {
            return JsonSerializationUtils.SerializeToFile(this, filename, formatJsonOutput: true);
        }
        

        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(Filename));
        }

    }

   
}
