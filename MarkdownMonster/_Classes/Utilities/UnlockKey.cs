using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class UnlockKey
    {
        /// <summary>
        /// The key to unlock this application
        /// </summary>        
        static readonly string ProKey;
        static readonly string RegisterFile;

        static UnlockKey()
        {
            RegisterFile = Path.Combine( mmApp.Configuration.CommonFolder,"Registered.key");
            ProKey = Encoding.UTF8.GetString(Convert.FromBase64String(mmApp.Signature));
        }

        /// <summary>
        /// Determines whether the app is unlocked
        /// </summary>
        public static bool Unlocked
        {
            get
            {
                if (RegisteredCalled)
                    return _unlocked;

                return IsRegistered();
            }
        }
        static bool _unlocked = false;


        /// <summary>
        /// Special Unlock check for Premium features.
        /// Premium features will work 2 out of 3 time;
        /// </summary>
        public static bool UnlockedPremium {
            get
            {
                if (Unlocked)
                    return true;

                return DateTime.Now.Ticks % 2 != 0;
            }
        }

        /// <summary>
        /// Determines whether the app is running the Pro Version
        /// </summary>
        /// <returns></returns>
        public static RegTypes RegType
        {
            get
            {
                if (RegisteredCalled)
                    return _regType;

                IsRegistered();
                return _regType;
            }
        }


        static RegTypes _regType = RegTypes.Free;

        private static readonly object LockKey = new Object();
        private static bool RegisteredCalled = false;

        /// <summary>
        /// Figures out if this copy is registered
        /// </summary>
        /// <returns></returns>
        public static bool IsRegistered()
        {
            lock (LockKey)
            {
                RegisteredCalled = true;

                _unlocked = false;
                _regType = RegTypes.Free;

                if (!File.Exists(RegisterFile))
                    return false;

                string key = File.ReadAllText(RegisterFile);
                var encodedKey = EncodeKey(ProKey);

                if (key == encodedKey )
                {
                    _regType = RegTypes.Professional;
                    _unlocked = true;
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// Writes out the registration information
        /// </summary>
        public static bool Register(string key)
        {
            lock (LockKey)
            {
                string rawKey = key;

                _regType = RegTypes.Free;
                _unlocked = false;

                if (rawKey != ProKey)
                    return false;

                _unlocked = true;
                key = EncodeKey(key);
                File.WriteAllText(RegisterFile, key);
                
                _regType = RegTypes.Professional;
            }
            return true;
        }

        public static void UnRegister()
        {
            _unlocked = false;
            _regType = RegTypes.Free;
            File.WriteAllText(RegisterFile, "");
        }

        /// <summary>
        /// Encodes the plain text key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static string EncodeKey(string key)
        {
            var encoded = Encryption.ComputeHash(key,"HMACSHA256", mmApp.InternalMachineKey);                        
            return encoded;
        }
        
        static System.Timers.Timer timer;
        static RegisterDialog regDialog;

        public static void Startup()
        {            
            if (regDialog != null)
                return; // already up

            if (!Unlocked && mmApp.Configuration.ApplicationUpdates.AccessCount > 50)
            {               
                timer = new System.Timers.Timer(25 * 1000 * 60); // 25 minutes
                timer.Elapsed += (s, ev) =>
                {
                    mmApp.Model?.Window?.Dispatcher?.Invoke(() =>
                    {
                        try
                        {
                            if (regDialog != null && regDialog.IsVisible)
                                return;
                            
                            regDialog = new RegisterDialog
                            {
                                Owner = mmApp.Model.Window
                            };
                            regDialog.ShowDialog();
                            regDialog = null;

                            timer.Stop();
                            timer.Start();
                        }
                        catch { }
                    });
                };
                timer.Start();
            }
        }

        public static void Shutdown()
        {
            timer?.Stop();
            timer?.Dispose();            

            try
            {
                regDialog?.Close();
            }
            catch { }
            regDialog = null;
        }

        /// <summary>
        /// Displays the Premium Feature dialog and returns the number of the button 0-n
        /// that was pressed. 1 if the close box was used (same as Cancel or ButtonCancel)
        /// </summary>
        /// <param name="premiumFeatureName">Name of the feature displayed in the dialog</param>
        /// <param name="premiumFeatureLink">Optional doc link - if provided a button for Feature Info is shown</param>
        /// <returns></returns>
        public static int ShowPremiumDialog(string premiumFeatureName, string premiumFeatureLink = null)
        {
            var form = new BrowserMessageBox();
            form.Owner = mmApp.Model?.Window;
            form.Title = "Premium Feature not available";
            form.Width = 550;
            form.Height = 370;
            form.SetMessage(string.Empty);

            string md = $@"<h3 style=""color: steelblue"">{premiumFeatureName} is a Premium Feature</h3>

This premium feature is only available in the registered version
of Markdown Monster. If you would like to use **{premiumFeatureName}** in Markdown Monster,
you can purchase a licensed copy of the software on our Web site.

<small style=""color: #888"">
Note: premium features are randomly disabled in the free edition
</small>
";


            form.ShowMarkdown(md);
            form.Icon = mmApp.Model.Window.Icon;
            form.ButtonOkText.Text = "Buy a License";
            form.ButtonCancelText.Text = "Continue Unlicensed";

            Button featureButton = null;
            if (!string.IsNullOrEmpty(premiumFeatureLink))
            {
                featureButton = form.AddButton("Feature Info", FontAwesomeIcon.InfoCircle, Brushes.SteelBlue);
            }

            var result = form.ShowDialog();

            if (form.ButtonResult == form.ButtonOk)
                ShellUtils.GoUrl("https://markdownmonster.west-wind.com/purchase.aspx");
            else if (form.ButtonResult == featureButton)
                ShellUtils.GoUrl(premiumFeatureLink);

            if (form.ButtonResult == null)
                return 1;

            return form.PanelButtonContainer.Children.IndexOf(form.ButtonResult);
        }
    }

    public enum RegTypes
    {
        Free,
        Professional,
        Enterprise
    }


}
