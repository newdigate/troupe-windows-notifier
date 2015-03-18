using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Troupe.Common.Config;
using Troupe.Common.Helpers;


namespace Trou.pe.NotificationApp.ConfigEncrypter
{
    public class Program
    {
        static void Main(string[] args)
        {
            TroupeConfigSection section = (TroupeConfigSection)ConfigurationManager.GetSection("troupe");

            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(TroupeConfigSection));
            serializer.Serialize(writer, section);
            string plainText = writer.ToString();
            
            byte[] encrypted = Encryption.EncryptStringToBytes(plainText, Encryption.Common_Key, Encryption.Common_iv);
            string s = Convert.ToBase64String(encrypted);

            Console.Write(s);
            if (args.Length == 1) {
                try {
                    StreamWriter fileWriter = new StreamWriter( File.OpenWrite(args[0]));
                    fileWriter.Write(s);
                    fileWriter.Flush();
                    fileWriter.Close();
                } catch (Exception exc) {
                    Console.WriteLine(exc);
                }
            }
        }
    }
}
