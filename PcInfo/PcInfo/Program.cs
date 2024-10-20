using System;
using Microsoft.Win32;
using System.Drawing.Printing;
using System.IO;
using System.Net.NetworkInformation;
using System.Management;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string nazwaKomputera = Environment.MachineName;
            string filePath = $"{nazwaKomputera}_wyniki.csv";

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                PobierzNumerSeryjnyBIOSTaNumeryProduktu(sw);
                sw.WriteLine();
                PobierzWersjeSystemu(sw);
                sw.WriteLine();
                PobierzInformacjeSprzetowe(sw);
                sw.WriteLine();
                PobierzListeDrukarek(sw);
                sw.WriteLine();
                PobierzDyskiSieciowe(sw);
                sw.WriteLine();
                PobierzAdresyMAC(sw);
                sw.WriteLine();
                PobierzDatyOstatnichLogowan(sw);
                sw.WriteLine();
                PobierzListeProgramow(sw);
            }

            Console.WriteLine($"Wyniki zostały zapisane do pliku: {filePath}");
            Environment.Exit(0);
        }

        static void PobierzWersjeSystemu(StreamWriter sw)
        {
            OperatingSystem os = Environment.OSVersion;
            Version ver = os.Version;

            sw.WriteLine("Informacje o systemie:");
            sw.WriteLine("----------------------");
            sw.WriteLine($"Platforma: {os.Platform}");
            sw.WriteLine($"Wersja: {ver}");
            sw.WriteLine($"Wersja przyjazna: {PobierzPrzyjaznaNazweSystemu()}");
            sw.WriteLine();
        }

        static string PobierzPrzyjaznaNazweSystemu()
        {
            string nazwa = "Nieznany";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                nazwa = os["Caption"].ToString();
                break;
            }
            return nazwa;
        }


        static void PobierzListeProgramow(StreamWriter sw)
        {
            sw.WriteLine("Lista programow:");
            sw.WriteLine("----------------------------------");
            sw.WriteLine("Nazwa;Wersja;Producent");
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            string displayName = sk.GetValue("DisplayName") as string;
                            string displayVersion = sk.GetValue("DisplayVersion") as string ?? "Brak informacji";
                            string displayPublisher = sk.GetValue("Publisher") as string ?? "Brak informacji";

                            if (!string.IsNullOrWhiteSpace(displayName))
                            {
                                sw.WriteLine($"{displayName};{displayVersion};{displayPublisher}");
                            }
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"Wystąpił błąd: {ex.Message}");
                        }
                    }
                }
            }
        }

        static void PobierzListeDrukarek(StreamWriter sw)
        {
            sw.WriteLine("Drukarki");
            sw.WriteLine("----------------------------------");
            sw.WriteLine("Nazwa;Port");
            if (PrinterSettings.InstalledPrinters.Count > 0)
            {
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    string portName = PobierzPortDrukarki(printerName);
                    sw.WriteLine($"{printerName};{portName}");
                }
            }
            else
            {
                sw.WriteLine("Nie znaleziono zainstalowanych drukarek.");
            }
        }

        static string PobierzPortDrukarki(string printerName)
        {
            string portName = "Nieznany port";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("\\", "\\\\")}'");

            foreach (ManagementObject printer in searcher.Get())
            {
                portName = printer["PortName"]?.ToString() ?? "Nieznany port";
                break; // Zakładamy, że jest tylko jedna drukarka o danej nazwie
            }

            return portName;
        }

        static void PobierzAdresyMAC(StreamWriter sw)
        {
            sw.WriteLine("Karty sieciowe i ich adresy MAC:");
            sw.WriteLine("----------------------------------");
            sw.WriteLine("Nazwa;Adres MAC");

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Pomijanie interfejsów, które nie mają przypisanego adresu fizycznego (np. pętle zwrotne)
                if (networkInterface.GetPhysicalAddress().ToString() != "")
                {
                    sw.WriteLine($"{networkInterface.Name};{networkInterface.GetPhysicalAddress()}");
                }
            }
        }

        static void PobierzInformacjeSprzetowe(StreamWriter sw)
        {
            sw.WriteLine("Podzespoły:");
            sw.WriteLine("---------------------");

            PobierzInformacjeProcesora(sw);
            PobierzInformacjeRAM(sw);
            PobierzInformacjeDyskow(sw);
        }

        static void PobierzInformacjeProcesora(StreamWriter sw)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                sw.WriteLine("Procesor");
                sw.WriteLine($"Nazwa;{obj["Name"]}");
                sw.WriteLine($"Identyfikator;{obj["ProcessorId"]}");
                sw.WriteLine($"Opis;{obj["Description"]}");
                sw.WriteLine();
            }
        }

        static void PobierzInformacjeRAM(StreamWriter sw)
        {
            int NumerKosciRam = 0;
            float SumaPamieciRAM = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                sw.WriteLine($"Pamięć: " + NumerKosciRam);
                sw.WriteLine($"Producent;{obj["Manufacturer"].ToString()}");
                sw.WriteLine($"Numer produktu;{obj["PartNumber"]?.ToString()}");
                sw.WriteLine($"Pojemność;{Convert.ToInt64(obj["Capacity"]) / 1024 / 1024 / 1024} GB");
                sw.WriteLine($"Szybkość;{obj["Speed"]} MHz");
                sw.WriteLine();
                SumaPamieciRAM = SumaPamieciRAM + Convert.ToInt64(obj["Capacity"]) / 1024 / 1024 / 1024;
                NumerKosciRam++;
            }
            sw.WriteLine($"Suma pamięć RAM; {SumaPamieciRAM}");
            sw.WriteLine();
        }

        static void PobierzInformacjeDyskow(StreamWriter sw)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject obj in searcher.Get())
            {
                sw.WriteLine("Dysk");
                sw.WriteLine($"Model;{obj["Model"]}");
                sw.WriteLine($"Interfejs;{obj["InterfaceType"]}");
                sw.WriteLine($"Pojemność;{Convert.ToInt64(obj["Size"]) / 1024 / 1024 / 1024} GB");
                sw.WriteLine();
            }
        }

        static void PobierzDyskiSieciowe(StreamWriter sw)
        {
            sw.WriteLine("Dyski sieciowe i ich lokalizacje:");
            sw.WriteLine("---------------------------------");
            sw.WriteLine("Nazwa;Ścieżka zdalna");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkConnection");

            foreach (ManagementObject networkDrive in searcher.Get())
            {
                string localName = networkDrive["LocalName"]?.ToString() ?? "Brak lokalnej nazwy";
                string remotePath = networkDrive["RemoteName"]?.ToString() ?? "Brak ścieżki zdalnej";

                sw.WriteLine($"{localName};{remotePath}");
            }

            sw.WriteLine();
        }

        static void PobierzNumerSeryjnyBIOSTaNumeryProduktu(StreamWriter sw)
        {
            sw.WriteLine("Informacje o sprzęcie:");
            sw.WriteLine("----------------------");

            ManagementObjectSearcher computerSystemSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in computerSystemSearcher.Get())
            {
                string manufacturer = obj["Manufacturer"]?.ToString() ?? "Brak informacji";
                string model = obj["Model"]?.ToString() ?? "Brak informacji";

                sw.WriteLine($"Producent komputera;{manufacturer}");
                sw.WriteLine($"Model komputera;{model}");
            }

            ManagementObjectSearcher computerSystemProductSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystemProduct");
            foreach (ManagementObject obj in computerSystemProductSearcher.Get())
            {
                string serialNumber = obj["IdentifyingNumber"]?.ToString() ?? "Brak informacji";
                string productNumber = obj["Name"]?.ToString() ?? "Brak informacji";
                string VersiionNumber = obj["Version"]?.ToString() ?? "Brak informacji";

                sw.WriteLine($"Numer seryjny komputera;{serialNumber}");
                sw.WriteLine($"Numer produktu komputera;{productNumber}");
                sw.WriteLine($"Wersja;{VersiionNumber}");
            }

            sw.WriteLine();
        }

        static void PobierzDatyOstatnichLogowan(StreamWriter sw)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkLoginProfile");

            sw.WriteLine("Ostatnie logowania użytkowników:");
            sw.WriteLine("---------------------");
            foreach (ManagementObject obj in searcher.Get())
            {
                // Nazwa użytkownika
                string name = obj["Name"]?.ToString() ?? "Nieznana nazwa";

                // Konwersja daty ostatniego logowania z formatu CIM_DATETIME na DateTime
                string lastLogon = obj["LastLogon"]?.ToString() ?? string.Empty;
                DateTime lastLogonDate = ConvertCimDateTime(lastLogon);

                sw.WriteLine($"Użytkownik: {name}, Ostatnie logowanie: {lastLogonDate}");
            }
        }

        static DateTime ConvertCimDateTime(string cimDateTime)
        {
            if (string.IsNullOrEmpty(cimDateTime))
            {
                return DateTime.MinValue; // Zwróć minimalną wartość DateTime, jeśli data jest pusta lub null
            }

            return ManagementDateTimeConverter.ToDateTime(cimDateTime);
        }
    }
}
