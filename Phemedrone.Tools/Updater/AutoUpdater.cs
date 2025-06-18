using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Phemedrone.Tools.Interface;
using Phemedrone.Tools.Interface.Settings;

namespace Phemedrone.Tools.Updater;

public class AutoUpdater
{
    public void LoadFromServer()
    {
        var config = Config.Instance;
        if (!config.UseAutoUpdater)
            return;
        
        var progress = new ProgressWindow(new ProgressWindowSettings
        {
            Title = "Checking for updates",
            Description = "This may take a while",
            Progress = 0,
            Stage = "Fetching data..."
        });
        progress.Draw();
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };
        string remoteResponse;

        try
        {
            remoteResponse = client.GetStringAsync(Constants.UpdaterLink).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            new Popup(new DefaultSettings
            {
                Title = "Error while making request",
                Description = $"Message: {ex.InnerException?.Message}"
            }).Draw();
            
            return;
        }

        var reader = new ContentDecoder(remoteResponse);
        
        progress.Update("Done", 1);
        progress.Draw();

        if (!Directory.Exists("stub"))
            Directory.CreateDirectory("stub");
        
        if (Directory.GetFiles("stub").Length == 0)
        {
            config.LatestVersion = string.Empty;
            config.Save();
        }
        
        if (!IsUpdateRequired(reader["Version"], config.LatestVersion)) return;
        
        var downloadPrompted = new BooleanSelection(new BooleanSelectionSettings
        {
            Title = "A new update was found",
            Description = $"Would you like to download a new update? {(string.IsNullOrEmpty(config.LatestVersion) ? "None" : config.LatestVersion)} -> {reader["Version"]}",
            DefaultValue = true
        }).Draw();

        if (!downloadPrompted)
        {
            var disableAutoUpdate = new BooleanSelection(new BooleanSelectionSettings
            {
                Title = "Disable auto-update",
                Description = "If you don't like auto-updating feature, you may disable it. Select 'Yes' if you would like to disable auto-update",
                DefaultValue = false
            }).Draw();

            if (!disableAutoUpdate) return;
            
            config.UseAutoUpdater = false;
            config.Save();
            
            return;
        }
        
        DownloadRemoteFile(
            reader["Link"] .Contains("www.mediafire.com/file/") ? MediaFire.GetDirectLink(reader["Link"]) : reader["Link"],
            Path.Combine(Directory.GetCurrentDirectory(), "stub", "stub")
        );

        config.LatestVersion = reader["Version"];
        config.Save();
    }

    private void DownloadRemoteFile(string directUrl, string destinationPath)
    {
        var progress = new ProgressWindow(new ProgressWindowSettings
        {
            Title = "Downloading file",
            Description = "This may take a while",
            Progress = 0,
            Stage = "Making request..."
        });
        progress.Draw();
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };
        
        byte[] fileBytes;

        try
        {
            fileBytes = client.GetByteArrayAsync(directUrl).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            new Popup(new DefaultSettings
            {
                Title = "Error while making request",
                Description = $"Message: {ex.InnerException?.Message}"
            }).Draw();
            
            return;
        }

        var fileDecoder = new FileDecoder(fileBytes);
        var decodedFileBytes = fileDecoder.SourceFile;
        
        using var fs = File.OpenWrite(destinationPath);
        fs.Write(decodedFileBytes, 0, decodedFileBytes.Length);
        
        progress.Update("Done", 1);
        progress.Draw();
    }

    private bool IsUpdateRequired(string remoteVersion, string currentVersion)
    {
        if (string.IsNullOrEmpty(currentVersion))
            return true;

        if (remoteVersion.Split('.').Length != 4)
            throw new ArgumentException("Remote version should be in such format: %d.%d.%d.%d. Got: " + remoteVersion, nameof(remoteVersion));

        if (currentVersion.Split('.').Length != 4)
            throw new ArgumentException("Current version should be in such format: %d.%d.%d.%d. Got: " + currentVersion, nameof(currentVersion));
        
        var verRemote = new Version(remoteVersion);
        var verCurrent = new Version(currentVersion);
        
        return verRemote.CompareTo(verCurrent) > 0;
    }
}