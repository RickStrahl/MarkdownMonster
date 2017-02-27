using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class UnlockKey
    {
        /// <summary>
        /// The key to unlock this application
        /// </summary>        
        static string ProKey;
        static string RegisterFile;

        static UnlockKey()
        {
            RegisterFile = Path.Combine( mmApp.Configuration.InternalCommonFolder,"Registered.key");
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

        public static int FreeThreadLimit = 10;
        public static int FreeSitesLimit = 20;

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

                if (key == EncodeKey(ProKey))
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

                //if (RawKey == ProKey)
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
        /// <param name="Key"></param>
        /// <returns></returns>
        static string EncodeKey(string Key)
        {
            var Encoded = Encryption.EncryptString(Key, mmApp.EncryptionMachineKey);
            //string Encoded = Key; //  for now do nothing Key.GetHashCode().ToString("x");
            return Encoded;
        }

        //static string GetMacAddress()
        //{
        //    string macAddresses = string.Empty;

        //    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        if (nic.OperationalStatus == OperationalStatus.Up)
        //        {
        //            macAddresses += nic.GetPhysicalAddress().ToString();
        //            break;
        //        }
        //    }

        //    return macAddresses;
        //}
    }

    public enum RegTypes
    {
        Free,
        Professional,
        Enterprise
    }


}
