using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XperiaCompanionMessagesConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("messages.xml"))
            {
                // XT XML VERSION=00000001
                XmlDocument collection = new XmlDocument();

                try
                {
                    collection.Load("messages.xml");
                    XmlNodeList messages = collection.GetElementsByTagName("sms");

                    XmlDocument smsesDoc = new XmlDocument();
                    XmlDeclaration xmlDeclaration = smsesDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlElement root = smsesDoc.DocumentElement;
                    smsesDoc.InsertBefore(xmlDeclaration, root);

                    XmlElement smses = smsesDoc.CreateElement(string.Empty, "smses", string.Empty);
                    smses.SetAttribute("count", messages.Count.ToString());

                    Guid backupSet = new Guid();
                    smses.SetAttribute("backup_set", backupSet.ToString());
                    smses.SetAttribute("backup_date", Extensions.ToUnixTime(DateTime.Now).ToString());

                    for (int i = 0; i < messages.Count; i++)
                    {
                        Console.Write("\rLine " + (i + 1) + " out of " + messages.Count);

                        XmlElement sms = smsesDoc.CreateElement("sms");

                        // default values
                        sms.SetAttribute("date_sent", "0");
                        sms.SetAttribute("locked", "0");
                        sms.SetAttribute("protocol", "0");
                        sms.SetAttribute("read", "1");
                        sms.SetAttribute("sc_toa", "null");
                        sms.SetAttribute("service_center", "null");
                        sms.SetAttribute("status", "-1");
                        sms.SetAttribute("subject", "null");
                        sms.SetAttribute("toa", "null");

                        // values from messages.xml
                        sms.SetAttribute("address", messages[i]["address"].InnerText);
                        sms.SetAttribute("date", messages[i]["timestamp"].InnerText);
                        sms.SetAttribute("type", messages[i]["type"].InnerText.Equals("incoming") ? "1" : "2");
                        sms.SetAttribute("body", messages[i]["text"].InnerText);

                        // don't know if i need readable_date
                        //sms.SetAttribute("readable_date", Extensions.UnixTimeStampToDateTime(double.Parse(messages[i]["timestamp"].InnerText)).ToString("d MMMM HH:mm:ss"));
                        // don't know if i have contact name in messages.xml
                        //sms.SetAttribute("contact_name", "");

                        smses.AppendChild(sms);
                    }

                    smsesDoc.AppendChild(smses);

                    //smsesDoc.OuterXml
                    smsesDoc.Save("sms-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught.");
                    Console.WriteLine("Could be that you have XT XML VERSION=00000001 at the top of messages.xml. Delete that line and try again.");
                    Console.WriteLine("The error message:");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Press any key to finish.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Could not find messages.xml.");
                Console.WriteLine("Please place the messages.xml file from Sony Xperia Companion Backups in the same directory as the executable.");
                Console.WriteLine("Press any key to finish.");
                Console.ReadKey();
            }
        }
    }

    public static class Extensions
    {
        // courtesy of https://stackoverflow.com/questions/24837474/c-sharp-convert-dates-to-timestamp
        public static double ToUnixTime(this DateTime input)
        {
            return input.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        // courtesy of https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

    }
}
