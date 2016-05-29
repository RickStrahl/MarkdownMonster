using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SnagItAddin.Annotations;

namespace SnagItAddin
{
    public class ScreenCaptureModel : INotifyPropertyChanged
    {
        

        public ScreenCaptureConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (Equals(value, _configuration)) return;
                _configuration = value;
                OnPropertyChanged();
            }
        }
        private ScreenCaptureConfiguration _configuration;


        public Dictionary<CaptureModes, string> CaptureModeList
        {
            get
            {
                if (_captureModeList == null)
                {
                    _captureModeList = new Dictionary<CaptureModes, string>();
                    _captureModeList.Add(CaptureModes.AllInOne, "All in One");
                    _captureModeList.Add(CaptureModes.Object, "Windows Object");
                    _captureModeList.Add(CaptureModes.Window, "Window");                    
                    _captureModeList.Add(CaptureModes.Desktop, "Desktop");
                    _captureModeList.Add(CaptureModes.ScrollableArea, "Scrollable Area or Window");
                    _captureModeList.Add(CaptureModes.Region, "Region");
                    _captureModeList.Add(CaptureModes.FreeHand, "Free Hand Selection");
                    _captureModeList.Add(CaptureModes.Menu, "Menu");                    
                }                

                return _captureModeList;
            }
        }
        public Dictionary<CaptureModes, string> _captureModeList;

    
        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
