/*
 **************************************************************
 * Snagit Automation Class
 **************************************************************
 *  Author: Rick Strahl 
 *          (c) West Wind Technologies
 *          http://www.west-wind.com/
 * 
 * Created:  03/15/2007
 **************************************************************  
*/

using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Reflection;
using System.Windows;
using Westwind.Utilities;

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
        public static string REGISTRY_STORAGE_SUBKEY = "Software\\Windows Live Writer\\SnagItScreenCapture";
        public static string SNAGIT_PROGID = "SnagIt.ImageCapture";

        /// <summary>
        /// The initial directory where files are saved.
        /// </summary>
        public string CapturePath
        {
            get { return _CapturePath; }
            set { _CapturePath = value; }
        }
        private string _CapturePath = Path.GetTempPath();


        /// <summary>
        /// Determines how the SnagIt Capture captures window content.
        /// </summary>
        public CaptureModes CaptureMode
        {
            get { return _CaptureMode; }
            set { _CaptureMode = value; }
        }
        private CaptureModes _CaptureMode = CaptureModes.Window;

        /// <summary>
        /// The file that receives the SnagIt Capture
        /// </summary>
        public string OutputCaptureFile
        {
            get { return _OutputCaptureFile; }            
        }
        private string _OutputCaptureFile = "";

        /// <summary>
        /// The file format for the captured image file.
        /// </summary>
        public CaptureFormats OutputFileCaptureFormat
        {
            get { return _OutputFileCaptureFormat; }
            set { _OutputFileCaptureFormat = value; }
        }
        private CaptureFormats _OutputFileCaptureFormat = CaptureFormats.png;


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
            get { return _ShowPreviewWindow; }
            set { _ShowPreviewWindow = value; }
        }
        private bool _ShowPreviewWindow = true;


        /// <summary>
        /// Determines how long to delay before capturing image
        /// </summary>
        public int DelayInSeconds
        {
            get { return _DelayInSeconds; }
            set { _DelayInSeconds = value; }
        }
        private int _DelayInSeconds = 0;

        /// <summary>
        /// The ActiveForm which is minimized if assigned
        /// </summary>
        [XmlIgnore]
        public Window ActiveForm
        {
            get { return _ActiveForm; }
            set { _ActiveForm = value; }
        }
        [NonSerialized]
        private Window _ActiveForm = null;


        /// <summary>
        /// Determines whether the image is deleted
        /// </summary>
        public bool DeleteImageFromDisk
        {
            get { return _DeleteImageFromDisk; }
            set { _DeleteImageFromDisk = value; }
        }
        private bool _DeleteImageFromDisk = false;


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
                    this._SnagItCom = Activator.CreateInstance(loT);
                }
                catch
                {
                    return null;
                }

                return _SnagItCom; 
            }            
        }
        private object _SnagItCom = null;
 

        /// <summary>
        /// Captures an image to file
        /// </summary>
        /// <returns></returns>
        public string CaptureImageToFile()
        {
            WindowState OldState = this.ActiveForm.WindowState;
            if (this.ActiveForm != null)
                this.ActiveForm.WindowState =  WindowState.Minimized;


      
            
            dynamic snagIt = SnagItCom;

            /// *** Capture first access to check if SnagIt is installed
            try
            {
                snagIt.OutputImageFile.Directory =  CapturePath;
            }
            catch
            {
                SetError("SnagIt isn't installed - COM Access failed.\r\nPlease install SnagIt from Techsmith Corporation (www.techsmith.com\\snagit).");
                return null;
            }


            snagIt.EnablePreviewWindow = ShowPreviewWindow;
            snagIt.OutputImageFile.Filename = "captured_image.png";
            //ReflectionUtils.SetPropertyCom(this.SnagItCom,"EnablePreviewWindow",this.ShowPreviewWindow);
            //ReflectionUtils.SetPropertyExCom(this.SnagItCom,"OutputImageFile.Filename", "captured_Image.png");

            ReflectionUtils.SetPropertyExCom(this.SnagItCom, "Input", this.CaptureMode);


            ReflectionUtils.SetPropertyExCom(this.SnagItCom, "OutputImageFile.FileType", (int)this.OutputFileCaptureFormat);           

            // Removed in SnagIt 11
            //ReflectionUtils.SetPropertyExCom(this.SnagItCom,"Filters.ColorConversion.ColorDepth",this.ColorDepth);

            ReflectionUtils.SetPropertyExCom(this.SnagItCom, "OutputImageFile.ColorDepth", this.ColorDepth);

            ReflectionUtils.SetPropertyExCom(this.SnagItCom, "IncludeCursor", this.IncludeCursor);
            
            if (this.DelayInSeconds > 0)
            {
                ReflectionUtils.SetPropertyExCom(this.SnagItCom, "DelayOptions.EnableDelayedCapture", true);
                ReflectionUtils.SetPropertyExCom(this.SnagItCom, "DelayOptions.DelaySeconds", this.DelayInSeconds);
            }



            // *** Need to delay a little here so that the form has properly minimized first
            // *** especially under Vista/Win7
            for (int i = 0; i < 20; i++)
            {                
                Thread.Sleep(5);                
            }            

            ReflectionUtils.CallMethodCom(this.SnagItCom, "Capture");

            try
            {
                bool TimedOut = true;
                while (true)
                {
                    if ((bool)ReflectionUtils.GetPropertyCom(this.SnagItCom, "IsCaptureDone"))
                    {
                        TimedOut = false;
                        break;
                    }

                    Thread.Sleep(100);                    
                }
            }
            // *** No catch let it throw
            finally
            {
                this._OutputCaptureFile = ReflectionUtils.GetPropertyCom(this.SnagItCom, "LastFileWritten") as string;

                if (this.ActiveForm != null)
                {
                    // Reactivate Live Writer
                    this.ActiveForm.WindowState = OldState;
                    
                    // Make sure it pops on top of SnagIt Editors
                    this.ActiveForm.Topmost = true;                    
                    Thread.Sleep(5);
                    this.ActiveForm.Topmost = false;
                }

                Marshal.ReleaseComObject(this.SnagItCom);
            }


            // *** If deleting the file we'll fire on a new thread and then delay by 
            // *** a few seconds until Writer has picked up the image.
            if ((this.DeleteImageFromDisk))
            {
                Thread thread = new Thread( new ParameterizedThreadStart(DeleteImage));
                thread.Start(this.OutputCaptureFile);                
            }

            
            return this.OutputCaptureFile;
        }

        /// <summary>
        /// Saves the current settings of this object to the registry
        /// </summary>
        /// <returns></returns>
        public bool SaveSettings()
        {
            SnagItAutomation Snag = this;

            byte[] Buffer = null;

            if (!SerializationUtils.SerializeObject(Snag,out Buffer))
                return false;

            RegistryKey SubKey = Registry.CurrentUser.OpenSubKey(REGISTRY_STORAGE_SUBKEY,true);
            if (SubKey == null)
                SubKey = Registry.CurrentUser.CreateSubKey(REGISTRY_STORAGE_SUBKEY);
            if (SubKey == null)
                return false;

            SubKey.SetValue("ConfigData", Buffer, RegistryValueKind.Binary);
            SubKey.Close();

            return true;
        }

        /// <summary>
        /// Factory method that creates teh SnagItAutomation object by trying to read last capture settings
        /// from the registry.
        /// </summary>
        /// <returns></returns>
        public static SnagItAutomation Create()
        {

            return new SnagItAutomation()
            {
                 CaptureMode = CaptureModes.Window,
                 DelayInSeconds = 4,
                 IncludeCursor = true,
                 ColorDepth = 24,
                 DeleteImageFromDisk = false,
                 ShowPreviewWindow = true,
                 OutputFileCaptureFormat = CaptureFormats.png
            };

            byte[] Buffer = null;


            RegistryKey SubKey = Registry.CurrentUser.OpenSubKey(REGISTRY_STORAGE_SUBKEY);
            if (SubKey != null)
                Buffer = SubKey.GetValue("ConfigData",null,RegistryValueOptions.None) as byte[];


            if (Buffer == null)
                return new SnagItAutomation();

            // Force Assembly resolving code to fire so we can load the assembly
            // from deserialization to avoid type load error
            AppDomain.CurrentDomain.AssemblyResolve +=
                new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            SnagItAutomation SnagIt = SerializationUtils.DeSerializeObject(Buffer,typeof(SnagItAutomation)) as SnagItAutomation;

            // *** Unhook the event handler for the rest of the application
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            if (SnagIt == null)
                return new SnagItAutomation();

            return SnagIt;
        }

        /// <summary>
        /// Handle custom loading of the current assembly if the assmebly won't
        /// resolve with the name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                // simply load by name which fails occasionally inside of Live Writer
                Assembly assembly = System.Reflection.Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch { ;}

            // otherwise default to this assembly which is all we care
            // about for this component anyway
            return Assembly.GetExecutingAssembly();            
        }

        /// <summary>
        /// Deletes an image file
        /// </summary>
        /// <param name="FileName"></param>
        static void DeleteImage(object FileName)
        {
            // *** wait a few seconds then delete Image
            //     LiveWriter creates its own copies of images
            //     that are linked from the file system
            //     so the originals are not required
            Thread.Sleep(10000);

            try
            {
                File.Delete(FileName as string);
            }
            catch { ; }
        }

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            this.SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                this.ErrorMessage = string.Empty;
                return;
            }
            this.ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                this.ErrorMessage = string.Empty;

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
        ScrollableArea = 18
    }
}
