using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using MarkdownMonster;
using SnagItAddin.Annotations;
using Westwind.Utilities.Configuration;

namespace SnagItAddin
{
    public class ScreenCaptureConfiguration : AppConfiguration, INotifyPropertyChanged
    {
        public static ScreenCaptureConfiguration Current;

        static ScreenCaptureConfiguration()
        {
            Current = new ScreenCaptureConfiguration();
            Current.Initialize();
        }


        public ScreenCaptureConfiguration()
        {
            // The following settings only apply if snagit is installed
            // External capture ignores all of these
            UseSnagItForImageCapture = true;
            CaptureMode = CaptureModes.AllInOne;
            ColorDepth = 24;
            OutputFormat = 4;
            IncludeCursor = true;
            CaptureDelaySeconds = 4;
            ShowPreviewWindow = true;            
        }


        /// <summary>
        /// If true and SnagIt is installed uses SnagIt for Image
        /// capture. Otherwise External Form capture is used.
        /// Default is true
        /// </summary>
        public bool UseSnagItForImageCapture
        {
            get { return _useSnagItForImageCapture; }
            set
            {
                if (value == _useSnagItForImageCapture) return;
                _useSnagItForImageCapture = value;
                OnPropertyChanged(nameof(UseSnagItForImageCapture));
            }
        }
        private bool _useSnagItForImageCapture;


        public CaptureModes CaptureMode
        {
            get { return _captureMode; }
            set
            {
                if (value == _captureMode) return;
                _captureMode = value;
                OnPropertyChanged(nameof(CaptureMode));
            }
        }

        public int ColorDepth
        {
            get { return _colorDepth; }
            set
            {
                if (value == _colorDepth) return;
                _colorDepth = value;
                OnPropertyChanged(nameof(ColorDepth));
            }
        }

        public int OutputFormat
        {
            get { return _outputFormat; }
            set
            {
                if (value == _outputFormat) return;
                _outputFormat = value;
                OnPropertyChanged(nameof(OutputFormat));
            }
        }

        public bool IncludeCursor
        {
            get { return _includeCursor; }
            set
            {
                if (value == _includeCursor) return;
                _includeCursor = value;
                OnPropertyChanged(nameof(IncludeCursor));
            }
        }

        public int CaptureDelaySeconds
        {
            get { return _captureDelaySeconds; }
            set
            {
                if (value == _captureDelaySeconds) return;
                _captureDelaySeconds = value;
                OnPropertyChanged(nameof(CaptureDelaySeconds));
            }
        }
        private int _captureDelaySeconds;


        public bool ShowPreviewWindow
        {
            get { return _showPreviewWindow; }
            set
            {
                if (value == _showPreviewWindow) return;
                _showPreviewWindow = value;
                OnPropertyChanged(nameof(ShowPreviewWindow));
            }
        }

        public bool AlwaysShowCaptureOptions
        {
            get { return _alwaysShowCaptureOptions; }
            set
            {
                if (value == _alwaysShowCaptureOptions) return;
                _alwaysShowCaptureOptions = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Keyboard shortcut for this addin.
        /// </summary>
        public string KeyboardShortcut
        {
            get { return _keyboardShortcut; }
            set
            {
                if (_keyboardShortcut == value) return;
                _keyboardShortcut = value;
                OnPropertyChanged(nameof(KeyboardShortcut));
            }
        }
        private string _keyboardShortcut = string.Empty;

        public string ImageEditorPath { get; set; } =
            @"C:\Program Files\paint.net\PaintDotNet.exe";

        public double WindowHeight { get; set; } = 730;
        public double WindowWidth { get; set; } = 910;
        

        private bool _showPreviewWindow;
        private bool _includeCursor;
        private int _outputFormat;
        private int _colorDepth;
        private CaptureModes _captureMode;
        private bool _alwaysShowCaptureOptions;

        #region IPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region AppConfiguration
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<ScreenCaptureConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "ScreenCaptureAddIn.json")
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
        #endregion
    }
}
