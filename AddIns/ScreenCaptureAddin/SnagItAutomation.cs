/*
 **************************************************************
 * Snagit Automation Class
 **************************************************************
 *  Author: Rick Strahl 
 *          (c) West Wind Technologies
 *          http://www.west-wind.com/
 * 
 * Created:  03/15/2007
 * 
 *           Snagit COM Documentation:
 *           http://download.techsmith.com/snagit/docs/comserver/enu/snagitcom.pdf
 **************************************************************  
*/

using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using MarkdownMonster.Windows;

namespace SnagItAddin
{
    /// <summary>
    /// This class interacts with SnagIt via COM automation using
    /// Reflection so no Interop assembly is required. The class
    /// has properties for most of the common SnagIt options.
    /// 
    /// This class requires that SnagIt 7 or later is installed.
    /// 
    /// The class is generic except the Create() and SaveSettings()
    /// methods which are specific to this implementation with
    /// LiveWriter as they write the settings into the registry
    /// under the LiveWriter key.
    /// </summary>
    [Serializable]
    public class SnagItAutomation
    {
        public static string SNAGIT_PROGID = "SnagIt.ImageCapture";

        /// <summary>
        /// The initial directory where files are saved.
        /// </summary>
        public string CapturePath
        {
            get { return _capturePath; }
            set { _capturePath = value; }
        }
        private string _capturePath = Path.GetTempPath();


        /// <summary>
        /// Determines how the SnagIt Capture captures window content.
        /// </summary>Capture
        public CaptureModes CaptureMode
        {
            get { return _captureMode; }
            set { _captureMode = value; }
        }
        private CaptureModes _captureMode = CaptureModes.AllInOne;

        /// <summary>
        /// The file that receives the SnagIt Capture
        /// </summary>
        public string OutputCaptureFile
        {
            get { return _outputCaptureFile; }            
            set { _outputCaptureFile = value; }
        }
        private string _outputCaptureFile = "capture_file.png";

        /// <summary>
        /// The file format for the captured image file.
        /// </summary>
        public CaptureFormats OutputFileCaptureFormat
        {
            get { return _outputFileCaptureFormat; }
            set { _outputFileCaptureFormat = value; }
        }
        private CaptureFormats _outputFileCaptureFormat = CaptureFormats.png;


        /// <summary>
        /// Determines the image color depth in bits: 1 (b&w), 8 (256 colors), 16, 24, 32
        /// </summary>
        public int ColorDepth
        {
            get { return _ImageDepth; }
            set { _ImageDepth = value; }
        }
        private int _ImageDepth = 24;

        /// <summary>
        /// Determines whether the cursor is included in the captureFormats
        /// </summary>
        public bool IncludeCursor
        {
            get { return _IncludeCursor; }
            set { _IncludeCursor = value; }
        }
        private bool _IncludeCursor = false;

        /// <summary>
        /// Determines whether the SnagIt preview window
        /// is displayed after the capture to allow you to
        /// edit and apply effects to the capture.
        /// </summary>
        public bool ShowPreviewWindow
        {
            get { return _showPreviewWindow; }
            set { _showPreviewWindow = value; }
        }
        private bool _showPreviewWindow = true;


        /// <summary>
        /// Determines how long to delay before capturing image
        /// </summary>
        public int DelayInSeconds
        {
            get { return _delayInSeconds; }
            set { _delayInSeconds = value; }
        }
        private int _delayInSeconds = 0;

        /// <summary>
        /// The ActiveForm which is minimized if assigned
        /// </summary>
        [XmlIgnore]
        public Window ActiveForm
        {
            get { return _activeForm; }
            set { _activeForm = value; }
        }
        [NonSerialized]
        private Window _activeForm = null;


        /// <summary>
        /// Determines whether the image is deleted
        /// </summary>
        public bool DeleteImageFromDisk
        {
            get { return _deleteImageFromDisk; }
            set { _deleteImageFromDisk = value; }
        }
        private bool _deleteImageFromDisk = false;


        /// <summary>
        /// Snagit COM Instance
        /// </summary>        
        public object SnagItCom
        {
            get 
            {
                if (_SnagItCom != null)
                    return _SnagItCom;

                try
                {
                    Type loT = Type.GetTypeFromProgID(SNAGIT_PROGID);
                    _SnagItCom = Activator.CreateInstance(loT);
                }
                catch
                {
                    return null;
                }

                return _SnagItCom; 
            }            
        }
        private object _SnagItCom = null;

        public static bool IsInstalled
        {
            get
            {
                Type loT = Type.GetTypeFromProgID(SNAGIT_PROGID);
                return !(loT == null);            
            }
        }

        /// <summary>
        /// Captures an image to file
        /// </summary>
        /// <returns></returns>
        public string CaptureImageToFile()
        {

            var OldState = WindowState.Minimized;
            if (ActiveForm != null)
            {
                OldState = ActiveForm.WindowState;
                ActiveForm.WindowState = WindowState.Minimized;
            }

            dynamic snagIt = SnagItCom;

            try
            {
                snagIt.OutputImageFile.Directory =  CapturePath;
            }
            catch
            {
                SetError("SnagIt isn't installed - COM Access failed.\r\nPlease install SnagIt from Techsmith Corporation (www.techsmith.com\\snagit).");
                return null;
            }


            snagIt.Input = CaptureMode;
            snagIt.EnablePreviewWindow = ShowPreviewWindow;

            snagIt.OutputImageFile.Filename = OutputCaptureFile;
            snagIt.OutputImageFile.Quality = 86; //86% quality
            snagIt.OutputImageFile.FileType = (int) OutputFileCaptureFormat;
            snagIt.OutputImageFile.ColorDepth = ColorDepth;

            snagIt.IncludeCursor = IncludeCursor;
            
            if (DelayInSeconds > 0)
            {
                snagIt.DelayOptions.EnableDelayedCapture = true;
                snagIt.DelayOptions.DelaySeconds = DelayInSeconds;
            }


            if (ActiveForm != null)
            {
                // *** Need to delay a little here so that the form has properly minimized first
                // *** especially under Vista/Win7
                for (int i = 0; i < 20; i++)
                {
                    WindowUtilities.DoEvents();
                    Thread.Sleep(5);
                }
            }

            snagIt.OnStateChange += new Action<CaptureState>(SnagImg_OnStateChange);            

            try
            {
                IsDone = false;
                snagIt.Capture();

                while (!IsDone && !HasError)
                {     
                    WindowUtilities.DoEvents();
                    Thread.Sleep(20);
                }
            }
            catch(Exception ex)
            {                
                ErrorMessage = "An error occurred during the image capture: " + ex.Message;                
            }
            // *** No catch let it throw
            finally
            {
                if (IsDone)
                    _outputCaptureFile = snagIt.LastFileWritten;
                else
                {
                    _outputCaptureFile = null;
                    ErrorMessage = "Image capture failed.";
                }

                if (ActiveForm != null)
                {
                    // Reactivate Editor
                    ActiveForm.WindowState = OldState;

                    // Make sure it pops on top of SnagIt Editors
                    ActiveForm.Topmost = true;
                    WindowUtilities.DoEvents();
                    Thread.Sleep(5);
                    ActiveForm.Topmost = false;
                }

                Marshal.ReleaseComObject(SnagItCom);
            }


            // If deleting the file we'll fire on a new thread and then delay by 
            // a few seconds until Writer has picked up the image.
            if ((DeleteImageFromDisk))
            {
                new Timer(
                    (imgFile) =>
                    {
                        var image = imgFile as string;
                        if (image == null)
                            return;
                        try
                        {
                            File.Delete(image);
                        }
                        catch {}
                    }, _outputCaptureFile, 10000, Timeout.Infinite);
            }

            return OutputCaptureFile;
        }
        

        private bool IsDone;
        private bool HasError;

        private void SnagImg_OnStateChange(CaptureState newState)
        {            
            if (newState != CaptureState.scsBusy && newState != CaptureState.scsIdle)
                IsDone = true;

            if (newState == CaptureState.scsCaptureFailed)
                HasError = true;

            Debug.WriteLine(newState.ToString() + " - IsDone " + IsDone );
        }


        public object ImageCaptureFilename { get; set; }

        /// <summary>
        /// Saves the current settings of this object to the registry
        /// </summary>
        /// <returns></returns>
        public bool SaveSettings()
        {
           return ScreenCaptureConfiguration.Current.Write();
        }

        /// <summary>
        /// Factory method that creates teh SnagItAutomation object by trying to read last capture settings
        /// from the registry.
        /// </summary>
        /// <returns></returns>
        public static SnagItAutomation Create()
        {
            var config = ScreenCaptureConfiguration.Current;

            return new SnagItAutomation()
            {
                 CaptureMode = config.CaptureMode,
                 DelayInSeconds = config.CaptureDelaySeconds,
                 IncludeCursor = config.IncludeCursor,
                 ColorDepth = config.ColorDepth,
                 DeleteImageFromDisk = false,
                 ShowPreviewWindow = true,
                 OutputFileCaptureFormat = CaptureFormats.png
            };
        }


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

    }

    public enum CaptureFormats
    {
        png = 5,
        gif = 4,
        jpg = 3,
        tif = 2,
        bmp = 0
    }
    
    public enum CaptureModes
    {
        Window = 1,
        Region = 4,
        Desktop = 0,
        Object = 10,
        FreeHand = 12,
        Clipboard = 7,
        Menu = 9,
        ScrollableArea = 18,
        AllInOne = 25
    }

    public enum CaptureState
    {
        scsIdle = 0,
        scsCaptureSucceeded = 10,
        scsCaptureFailed = 11,
        scsBusy = 12
    }
    public enum snagError
    {
        serrUnknown = -1,
        serrNone = 0,
        serrSnagItExpired = 1,
        serrInvalidInput = 2,
        serrInvalidOutput = 3,
        serrEngineBusy = 4,
        serrInvalidScrollDelay = 5,
        serrInvalidDelay = 6,
        serrInvalidColorEffectValue = 7,
        serrInvalidFileProgressiveValue = 8,
        serrInvalidFileQualityValue = 9,
        serrInvalidFileDirectory = 10,
        serrInvalidColorConversionValue = 11,
        serrInvalidImageResolution = 12
    }
}
