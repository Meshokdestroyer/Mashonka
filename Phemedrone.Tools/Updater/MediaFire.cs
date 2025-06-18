using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Phemedrone.Tools.Interface;
using Phemedrone.Tools.Interface.Settings;

namespace Phemedrone.Tools.Updater;

public static class MediaFire
{
    // https://github.com/0xWaleed/MediaFire-Direct-Link-Grabber/blob/e7ed826fd164a61ad8e00ee836d175f8a5916e22/Direct%20MediaFire%20Link/Direct%20MediaFire%20Link/Form1.cs#L69C17-L69C63
    public static string GetDirectLink(string link)
    {
        try
        {
            var source = new WebClient().DownloadString(link);
            var directLink = Regex.Match(source, "https://download.*").Value.Split('\"')[0].Replace("\n", "");
            return directLink;
        }
        catch
        {
            new Popup(new DefaultSettings
            {
                Title = "Mediafire direct link failure",
                Description = "We couldn't fetch the direct link for Mediafire file. Source link: " + link
            }).Draw();
            
            Environment.Exit(-1);
            
            return null;
        }
        
    }
}