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

            XElement CT  = XElement.Load(objectsFolder + "Falcon4_CT.xml");
            XElement OCD = XElement.Load(objectsFolder + "Falcon4_OCD.xml");
            XElement VCD = XElement.Load(objectsFolder + "Falcon4_VCD.xml");

            IEnumerable<XElement> AFBs = CT.Elements("CT")
                                           .Where(x => x.Element("Domain").Value   == "3")
                                           .Where(x => x.Element("Class").Value    == "4")
                                           .Where(x => x.Element("Type").Value     == "1")
                                           .Where(x => x.Element("SubType").Value  == "255")
                                           .Where(x => x.Element("Specific").Value == "255")
                                           .Where(x => x.Element("Owner").Value    == "0");

            IEnumerable<XElement> Flights = CT.Elements("CT")
                                           .Where(x => x.Element("Domain").Value   == "2")
                                           .Where(x => x.Element("Class").Value    == "7")
                                           .Where(x => x.Element("Type").Value     == "1")
                                           .Where(x => x.Element("SubType").Value  == "7")
                                           .Where(x => x.Element("Owner").Value    == "0");

            XElement AFB_TO     = AFBs.ElementAt(new Random().Next(AFBs.Count()));
            XElement AFB_TARGET = AFBs.ElementAt(new Random().Next(AFBs.Count()));

            XElement Flights_Strikers    = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement Flights_SEAD        = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement Flights_Escort      = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement Flights_Adversaries = Flights.ElementAt(new Random().Next(Flights.Count()));

            string randomAFB_TO = OCD.Elements("OCD")
                                  .First(x => x.Element("CtIdx").Value == AFB_TO.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomAFB_ATK = OCD.Elements("OCD")
                                  .First(x => x.Element("CtIdx").Value == AFB_TARGET.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomFlights_Strikers = VCD.Elements("VCD")
                                  .First(x => x.Element("CtIdx").Value == Flights_Strikers.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomFlights_SEAD = VCD.Elements("VCD")
                                  .First(x => x.Element("CtIdx").Value == Flights_SEAD.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomFlights_Escort = VCD.Elements("VCD")
                                  .First(x => x.Element("CtIdx").Value == Flights_Escort.Attribute("Num").Value)
                                  .Element("Name").Value;

            string randomFlights_Adversaries = VCD.Elements("VCD")
                                  .First(x => x.Element("CtIdx").Value == Flights_Adversaries.Attribute("Num").Value)
                                  .Element("Name").Value;

            Console.WriteLine(
                    "AFB T/O:     " + randomAFB_TO + "\n" +
                    "AFB STRIKE:  " + randomAFB_ATK + "\n" +
                    "BLUE STK:    " + randomFlights_Strikers + "\n" +
                    "BLUE SEAD:   " + randomFlights_SEAD + "\n" +
                    "BLUE ESCORT: " + randomFlights_Escort + "\n" +
                    "ADVERSARY:   " + randomFlights_Adversaries + "\n"
                    );
        }
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
