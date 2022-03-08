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

            IEnumerable<XElement> CTs  = XElement.Load(objectsFolder + "Falcon4_CT.xml" ).Elements("CT");
            IEnumerable<XElement> OCDs = XElement.Load(objectsFolder + "Falcon4_OCD.xml").Elements("OCD");
            IEnumerable<XElement> VCDs = XElement.Load(objectsFolder + "Falcon4_VCD.xml").Elements("VCD");
            IEnumerable<XElement> WCDs = XElement.Load(objectsFolder + "Falcon4_WCD.xml").Elements("WCD");
            IEnumerable<XElement> WLDs = XElement.Load(objectsFolder + "Falcon4_WLD.xml").Elements("WLD");

            IEnumerable<XElement> AFBs = CTs
                                           .Where(x => x.Element("Domain").Value   == "3")
                                           .Where(x => x.Element("Class").Value    == "4")
                                           .Where(x => x.Element("Type").Value     == "1")
                                           .Where(x => x.Element("SubType").Value  == "255")
                                           .Where(x => x.Element("Specific").Value == "255")
                                           .Where(x => x.Element("Owner").Value    == "0");

            IEnumerable<XElement> Flights = CTs
                                           .Where(x => x.Element("Domain").Value   == "2")
                                           .Where(x => x.Element("Class").Value    == "7")
                                           .Where(x => x.Element("Type").Value     == "1")
                                           .Where(x => x.Element("SubType").Value  == "7")
                                           .Where(x => x.Element("Owner").Value    == "0");

            XElement AFB_TO              = RandomXElement(AFBs, OCDs);
            XElement AFB_ATK             = RandomXElement(AFBs, OCDs);

            XElement Flights_Strikers    = RandomXElement(Flights, VCDs);
            XElement Flights_SEAD        = RandomXElement(Flights, VCDs);
            XElement Flights_Escort      = RandomXElement(Flights, VCDs);
            XElement Flights_Adversaries = RandomXElement(Flights, VCDs);

            List<string> Flights_Strikers_Ordances    = SuggestPayloads(WLDs, WCDs, Flights_Strikers);
            List<string> Flights_SEAD_Ordances        = SuggestPayloads(WLDs, WCDs, Flights_SEAD);
            List<string> Flights_Escort_Ordances      = SuggestPayloads(WLDs, WCDs, Flights_Escort);
            List<string> Flights_Adversaries_Ordances = SuggestPayloads(WLDs, WCDs, Flights_Adversaries);

            Console.WriteLine(
                    "AFB T/O:     " + AFB_TO .Element("Name").Value + "\n" +
                    "AFB STRIKE:  " + AFB_ATK.Element("Name").Value + "\n"
                    );

            Console.WriteLine(
                    "BLUE STK:    " + Flights_Strikers   .Element("Name").Value + "\n" +
                    "BLUE SEAD:   " + Flights_SEAD       .Element("Name").Value + "\n" +
                    "BLUE ESCORT: " + Flights_Escort     .Element("Name").Value + "\n" +
                    "ADVERSARY:   " + Flights_Adversaries.Element("Name").Value + "\n"
                    );

            Console.WriteLine("LOADOUT SUGGESTION:\n");
            foreach (string str in Flights_Strikers_Ordances)
            {
                Console.WriteLine("    " + str);
            }
        }

        static XElement RandomXElement(IEnumerable<XElement> CTs, IEnumerable<XElement> OCDs)
        {
            XElement CT = CTs.ElementAt(new Random().Next(CTs.Count()));
            return OCDs.First(x => x.Element("CtIdx").Value == CT.Attribute("Num").Value);
        }

        static List<string> SuggestPayloads(IEnumerable<XElement> WLDs, IEnumerable<XElement> WCDs, XElement Flights)
        {
            List<string> Ordances= new List<string>();

            List<string> HPs = new List<string>();

            for (int i = 0; i < 128; i++)
            {
                if (Flights.Element("WpnOrHpIdx_" + i) == null)
                    continue;
                if (Flights.Element("WpnCount_" + i).Value != "255")
                    continue;
                HPs.Add(Flights.Element("WpnOrHpIdx_" + i).Value);
            }

            IEnumerable<string> HP_WLDs = HPs.Distinct();

            foreach (string WLD_Num in HP_WLDs)
            {
                List<XElement> AvailableOrdances = new List<XElement>();

                XElement AvailableWeapons = WLDs.First(x => x.Attribute("Num").Value == WLD_Num);

                for (int i = 0; i < 128; i++)
                {
                    if (AvailableWeapons.Element("WpnIdx_" + i) == null)
                        continue;
                    AvailableOrdances.Add(WCDs.First(x => x.Attribute("Num").Value == AvailableWeapons.Element("WpnIdx_" + i).Value).Element("Name"));
                }

                if (AvailableOrdances.Count() == 0)
                    continue;

                Ordances.Add(AvailableOrdances.ElementAt(new Random().Next(AvailableOrdances.Count())).Value);
            }

            return Ordances;
        }

        static IEnumerable<XElement> FilterCT(IEnumerable<XElement> CTs)
        {
            return CTs.Where(x => x.Element("Domain").Value == "3");
        }
    }

    public static class ExMethod
    {
        public static XElement FetchCT(this XElement XE, IEnumerable<XElement> CTs)
        {
            return CTs.First(x => x.Attribute("Num").Value == XE.Element("CtIdx").Value);
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
