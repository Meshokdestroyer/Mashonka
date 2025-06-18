using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Phemedrone.Tools.Interface;
using Phemedrone.Tools.Interface.Settings;

namespace Phemedrone.Tools;

public static class Startup
{

    public static void Init()
    {
        if (!Directory.Exists("stub"))
        {
            Directory.CreateDirectory("stub");

            new Popup(new DefaultSettings
            {
                Title = "Stub was not found",
                Description = "Stub file was not found. Download it manually, rename it to 'stub' without extension and move it into 'stub' folder. Alternatively, you can use auto-updater feature to get the latest stub."
            }).Draw();
        }

        if (!File.Exists("stub\\stub"))
        {
            SearchFile();
        }
    }

    private static void SearchFile()
    {
        var files = Directory.GetFiles("stub\\", "*.exe");
        if (files.Length == 0)
        {
            new Popup(new DefaultSettings
            {
                Title = "Stub was not found",
                Description = "Stub file was not found. Download it manually, rename it to 'stub' without extension and move it into 'stub' folder. Alternatively, you can use auto-updater feature to get the latest stub."
            }).Draw();
            
            Environment.Exit(-1);
        }

        var firstStub = files.First();
        
        var useStub = new BooleanSelection(new BooleanSelectionSettings
        {
            Title = "A file was found",
            Description = $"We found a file, which you may use as a stub ({Path.GetFileName(firstStub)}). Would you like to use it?",
            DefaultValue = true
        }).Draw();

        if (useStub)
        {
            var newFilePath = Path.Combine("stub\\", "stub");
            File.Move(firstStub, newFilePath);
            
            return;
        }
        
        new Popup(new DefaultSettings
        {
            Title = "Stub was not found",
            Description = "Stub file was not found. Download it manually, rename it to 'stub' without extension and move it into 'stub' folder. Alternatively, you can use auto-updater feature to get the latest stub."
        }).Draw();
    }
}