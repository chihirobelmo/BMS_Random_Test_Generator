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

            XElement CT  = XElement.Load(objectsFolder + "Falcon4_CT.xml" );
            XElement OCD = XElement.Load(objectsFolder + "Falcon4_OCD.xml");
            XElement VCD = XElement.Load(objectsFolder + "Falcon4_VCD.xml");
            XElement WCD = XElement.Load(objectsFolder + "Falcon4_WCD.xml");
            XElement WLD = XElement.Load(objectsFolder + "Falcon4_WLD.xml");

            IEnumerable<XElement> CTs = CT.Elements("CT");

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

            XElement CT_AFB_TO              = AFBs   .ElementAt(new Random().Next(AFBs   .Count()));
            XElement CT_AFB_TARGET          = AFBs   .ElementAt(new Random().Next(AFBs   .Count()));
            XElement CT_Flights_Strikers    = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement CT_Flights_SEAD        = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement CT_Flights_Escort      = Flights.ElementAt(new Random().Next(Flights.Count()));
            XElement CT_Flights_Adversaries = Flights.ElementAt(new Random().Next(Flights.Count()));

            XElement AFB_TO              = OCD.Elements("OCD").First(x => x.Element("CtIdx").Value == CT_AFB_TO             .Attribute("Num").Value);
            XElement AFB_ATK             = OCD.Elements("OCD").First(x => x.Element("CtIdx").Value == CT_AFB_TARGET         .Attribute("Num").Value);
            XElement Flights_Strikers    = VCD.Elements("VCD").First(x => x.Element("CtIdx").Value == CT_Flights_Strikers   .Attribute("Num").Value);
            XElement Flights_SEAD        = VCD.Elements("VCD").First(x => x.Element("CtIdx").Value == CT_Flights_SEAD       .Attribute("Num").Value);
            XElement Flights_Escort      = VCD.Elements("VCD").First(x => x.Element("CtIdx").Value == CT_Flights_Escort     .Attribute("Num").Value);
            XElement Flights_Adversaries = VCD.Elements("VCD").First(x => x.Element("CtIdx").Value == CT_Flights_Adversaries.Attribute("Num").Value);

            List<string> Flights_Strikers_HPs    = new List<string>();
            List<string> Flights_SEAD_HPs        = new List<string>();
            List<string> Flights_Escort_HPs      = new List<string>();
            List<string> Flights_Adversaries_HPs = new List<string>();

            ListWeaponSlots(Flights_Strikers,    Flights_Strikers_HPs);
            ListWeaponSlots(Flights_SEAD,        Flights_SEAD_HPs);
            ListWeaponSlots(Flights_Escort,      Flights_Escort_HPs);
            ListWeaponSlots(Flights_Adversaries, Flights_Adversaries_HPs);

            List<string> Flights_Strikers_Ordances    = new List<string>();
            List<string> Flights_SEAD_Ordances        = new List<string>();
            List<string> Flights_Escort_Ordances      = new List<string>();
            List<string> Flights_Adversaries_Ordances = new List<string>();

            SuggestPayloads(WLD, WCD, Flights_Strikers_HPs,    Flights_Strikers_Ordances);
            SuggestPayloads(WLD, WCD, Flights_SEAD_HPs,        Flights_SEAD_Ordances);
            SuggestPayloads(WLD, WCD, Flights_Escort_HPs,      Flights_Escort_Ordances);
            SuggestPayloads(WLD, WCD, Flights_Adversaries_HPs, Flights_Adversaries_Ordances);

            Console.WriteLine(
                    "AFB T/O:     " + AFB_TO             .Element("Name").Value + "\n" +
                    "AFB STRIKE:  " + AFB_ATK            .Element("Name").Value + "\n"
                    );

            Console.WriteLine(
                    "BLUE STK:    " + Flights_Strikers.Element("Name").Value + "\n"
                    );

            foreach (string str in Flights_Strikers_Ordances)
            {
                Console.WriteLine("    " + str + "\n");
            }

            Console.WriteLine(
                    "BLUE STK:    " + Flights_Strikers.Element("Name").Value + "\n" +
                    "BLUE SEAD:   " + Flights_SEAD.Element("Name").Value + "\n" +
                    "BLUE ESCORT: " + Flights_Escort.Element("Name").Value + "\n" +
                    "ADVERSARY:   " + Flights_Adversaries.Element("Name").Value + "\n"
                    );
        }

        static void SuggestPayloads(XElement WLD, XElement WCD, List<string> Flights_Strikers_HPs, List<string> Flights_Strikers_Ordances)
        {
            IEnumerable<string> Flights_Strikers_HP_WLDs = Flights_Strikers_HPs.Distinct();

            foreach (string WLD_Num in Flights_Strikers_HP_WLDs)
            {
                List<string> AvailableOrdances = new List<string>();

                XElement AvailableWeapons = WLD.Elements("WLD").First(x => x.Attribute("Num").Value == WLD_Num);

                for (int i = 0; i < 128; i++)
                {
                    if (AvailableWeapons.Element("WpnIdx_" + i) == null)
                        continue;
                    AvailableOrdances.Add(WCD.Elements("WCD").First(x => x.Attribute("Num").Value == AvailableWeapons.Element("WpnIdx_" + i).Value).Element("Name").Value);
                }

                Flights_Strikers_Ordances.Add(AvailableOrdances.ElementAt(new Random().Next(AvailableOrdances.Count())));
            }
        }

        static void ListWeaponSlots(XElement Flights, List<string> HPs)
        {
            for (int i = 0; i < 128; i++)
            {
                if (Flights.Element("WpnOrHpIdx_" + i) == null)
                    continue;
                if (Flights.Element("WpnCount_" + i).Value != "255")
                    continue;
                HPs.Add(Flights.Element("WpnOrHpIdx_" + i).Value);
            }
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
