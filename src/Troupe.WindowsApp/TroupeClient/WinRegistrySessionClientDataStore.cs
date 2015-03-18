using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using log4net;
using Microsoft.Win32;
using Troupe.Common.Helpers;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class WinRegistrySessionClientDataStore : ISessionClientDataStore {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WinRegistrySessionClientDataStore));

        private static XmlSerializer authTokenSerializer = new XmlSerializer(typeof(TroupeAuthenticationToken));
        private static readonly string _regKeyName_software = "Software";
        private static readonly string _regKeyName_troupeforwindows = "TroupeForWindows";
        private static readonly string _regKeyName_SessionKey = "SessionKey";
        private readonly string _environmentName;

        public WinRegistrySessionClientDataStore(string environmentName){
            _environmentName = environmentName;
        }

        public void SaveSessionClientData(ITroupeAuthenticationToken token) {
            logger.Info("Saving client session data");
            RegistryKey regKey_currentUser = Registry.CurrentUser;
            try {
                RegistryKey regKey_software = regKey_currentUser.OpenSubKey(_regKeyName_software, true);
                try {

                    if (regKey_software == null) {
                        regKey_software = regKey_currentUser.CreateSubKey(_regKeyName_software);
                    }

                    RegistryKey regKey_troupeForWindows = regKey_software.OpenSubKey(_regKeyName_troupeforwindows, true);
                    try {

                        if (regKey_troupeForWindows == null){
                            regKey_troupeForWindows = regKey_software.CreateSubKey(_regKeyName_troupeforwindows);
                        }

                        if (regKey_troupeForWindows != null) {

                            RegistryKey regKey_troupeForWindowsEnvironment = regKey_troupeForWindows.OpenSubKey(_environmentName, true);

                            if (regKey_troupeForWindowsEnvironment == null) {
                                regKey_troupeForWindowsEnvironment = regKey_troupeForWindows.CreateSubKey(_environmentName);
                            }

                            try {

                                if (token != null) {
                                    string authenticationTokenXML = null;
                                    try {
                                        StringBuilder stringBuilder = new StringBuilder();
                                        StringWriter writer = new StringWriter(stringBuilder);
                                        authTokenSerializer.Serialize(writer, token);
                                        authenticationTokenXML = Convert.ToBase64String(Encryption.EncryptStringToBytes(stringBuilder.ToString(), Encryption.Common_Key, Encryption.Common_iv));
                                        regKey_troupeForWindowsEnvironment.SetValue(_regKeyName_SessionKey, authenticationTokenXML);
                                    }
                                    catch (Exception exception) {
                                        logger.Error(exception);
                                    }
                                }

                            } finally {
                                regKey_troupeForWindowsEnvironment.Close();
                            }
                        }

                    } finally {
                        regKey_troupeForWindows.Close();
                    }

                } finally {
                    regKey_software.Close();
                }

            } finally {
                regKey_currentUser.Close();

            }

        }

        public void DeleteSessionClientData() {
            string keyName = String.Format("{0}\\{1}\\{2}", _regKeyName_software, _regKeyName_troupeforwindows, _environmentName);
            RegistryKey regKey_troupeForWindows = Registry.CurrentUser.OpenSubKey(keyName, true);
            try {
                regKey_troupeForWindows.DeleteValue(_regKeyName_SessionKey);
            } catch (Exception exc) {
                logger.Warn(exc);
            } finally {
                regKey_troupeForWindows.Close();
            }
        }

        public ITroupeAuthenticationToken GetClientSessionData() {
            RegistryKey regKey_currentUser = Registry.CurrentUser;
            try {

                RegistryKey regKey_troupeUser = regKey_currentUser.OpenSubKey(string.Concat(_regKeyName_software, "\\", _regKeyName_troupeforwindows, "\\", _environmentName));
                if (regKey_troupeUser != null)
                {
                    try
                    {
                        object sessionToken = regKey_troupeUser.GetValue(_regKeyName_SessionKey);
                        if (sessionToken != null)
                        {
                            string authenticationTokenXML = sessionToken.ToString();
                            try
                            {
                                authenticationTokenXML = Encryption.DecryptStringFromBytes(Convert.FromBase64String(authenticationTokenXML), Encryption.Common_Key, Encryption.Common_iv);
                                TroupeAuthenticationToken result = (TroupeAuthenticationToken)authTokenSerializer.Deserialize(new StringReader(authenticationTokenXML));
                                logger.Info("Found client data session key");
                                return result;
                            }
                            catch (Exception exception)
                            {
                                logger.Error(exception);
                                return null;
                            }
                        }
                    }
                    finally
                    {
                        regKey_troupeUser.Close();
                    }
                }

            } finally {
                regKey_currentUser.Close();
            }

            logger.Info("Client session data not found...");
            return null;
        }


    }
}