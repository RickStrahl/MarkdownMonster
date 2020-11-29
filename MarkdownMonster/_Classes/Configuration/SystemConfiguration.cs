using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster.Configuration
{
    public class SystemConfiguration
    {
        
        /// <summary>
        /// If set makes the application not use GPU accelleration.
        /// Set this setting if you have problems with MM starting up
        /// with a black screen. A very few  video drivers are known to
        /// have render problems and this setting allows getting around
        /// this driver issue.
        /// </summary>
        public bool DisableHardwareAcceleration { get; set; }

        /// <summary>
        /// If true opens the browser developer tools on startup for the
        /// Previewer. (only works with Chromium Controls)
        /// </summary>
        public bool ShowPreviewDeveloperTools { get; set; }

        /// <summary>
        /// If true opens the browser developer tools on startup for the
        /// Previewer. (only works with Chromium Controls)
        /// </summary>

        public bool ShowEditorDeveloperTools { get; set; }


        /// <summary>
        /// If true immediately opens the developer tools for hte preview on startup
        /// </summary>
        public bool ShowDeveloperToolsOnStartup { get; set; }

        
        #region Bug Reporting and Telemetry

        /// <summary>
        /// Determines whether errors are reported anonymously
        /// </summary>
        public bool ReportErrors { get; set; } = true;


        /// <summary>
        /// Flag to determine whether telemetry is sent
        /// </summary>
        public bool SendTelemetry { get; set; } = true;

        
        #endregion


    }
}
