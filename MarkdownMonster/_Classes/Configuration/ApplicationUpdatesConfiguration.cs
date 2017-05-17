using System;
using Newtonsoft.Json;

namespace MarkdownMonster
{

    /// <summary>
    /// Configuration item for Application Updates which are attached
    /// to the main Configuration 
    /// </summary>
    public class ApplicationUpdatesConfiguration
    {
        ///// <summary>
        ///// Url to check version info from
        ///// </summary>
        //[JsonIgnore]
        //public string UpdateCheckUrl { get; }

        /// <summary>
        /// Last date and time when an update check was performed
        /// </summary>
        public DateTime LastUpdateCheck { get; set; }

        /// <summary>
        /// Frequency for update checks in days. Done on shutdown
        /// </summary>
        public int  UpdateFrequency { get; set; }


        /// <summary>
        /// Keeps track how many times MM was run. Use in Telemetry
        /// for informational info.
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// Determines if this is the first time MM is started
        /// </summary>
        public bool FirstRun { get; set; }


        public ApplicationUpdatesConfiguration()
        {                        
            UpdateFrequency = 7;
            // prevent immediate update check for at least a day on new install
            LastUpdateCheck = DateTime.UtcNow.AddDays((UpdateFrequency -1) * -1 ); 
            FirstRun = true;            
        }
    }
}