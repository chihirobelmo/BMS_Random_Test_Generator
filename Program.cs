using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Win32;

namespace BMS_Random_Test_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            AppRegInfo appRegInfo = new AppRegInfo();

            string objectsFolder = appRegInfo.installDir + "\\Data\\TerrData\\Objects\\";

            XElement CT = XElement.Load(objectsFolder + "Falcon4_CT.xml");

            XElement OCD = XElement.Load(objectsFolder + "Falcon4_OCD.xml");

            IEnumerable<XElement> AFBs = CT.Elements("CT")
                                           .Where(x => x.Element("Domain").Value == "3")
                                           .Where(x => x.Element("Class").Value == "4")
                                           .Where(x => x.Element("Type").Value == "1")
                                           .Where(x => x.Element("SubType").Value == "255")
                                           .Where(x => x.Element("Specific").Value == "255")
                                           .Where(x => x.Element("Owner").Value == "0");

            XElement AFB_TO = AFBs.ElementAt(new Random().Next(AFBs.Count()));
            XElement AFB_TARGET = AFBs.ElementAt(new Random().Next(AFBs.Count()));

            string randomAFB_TO = OCD.Elements("OCD")
                                  .First(x => x.Element("CtIdx").Value == AFB_TO.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomAFB_ATK = OCD.Elements("OCD")
                                  .First(x => x.Element("CtIdx").Value == AFB_TARGET.Attribute("Num").Value)
                                  .Element("Name").Value;

            Console.WriteLine("Hello World!");
        }
    }

    public class CT
    {
        
    }

    public class AppRegInfo
    {
        public string installDir;
        public string currentTheater;

        public AppRegInfo()
        {
            string regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.36 (Internal)";
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(regName, true);

            if (regkey == null)
                return;

            installDir     = (string)regkey.GetValue("baseDir");
            currentTheater = (string)regkey.GetValue("curTheater");

            regkey.Close();
        }
    }
}
