using System;
using System.IO;
using System.Xml;

namespace Phemedrone.Tools;

// можно было б рефлексией реализовать конфиг систему, но мне лень это делать ради двух настроек

public class Config
{
    public bool UseAutoUpdater { get; set; } = true;
    public string LatestVersion { get; set; } = string.Empty;
    
    private static Config _instance;
    public static Config Instance => _instance ??= new Config();
    
    private const string ConfigFile = "config.xml";

    public Config()
    {
        this.Load();
    }

    private void Load()
    {
        if (!File.Exists(ConfigFile))
            return;
        
        using var reader = XmlReader.Create(ConfigFile);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case nameof(UseAutoUpdater):
                        this.UseAutoUpdater = Convert.ToBoolean(reader.ReadElementContentAsString());
                        break;
                    case nameof(LatestVersion):
                        this.LatestVersion = reader.ReadElementContentAsString();
                        break;
                }
            }
        }
    }

    public void Save()
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = false
        };

        using var writer = XmlWriter.Create(ConfigFile, settings);
        
        writer.WriteStartDocument();
        writer.WriteStartElement("Configuration");
            
        writer.WriteElementString(nameof(UseAutoUpdater), this.UseAutoUpdater.ToString());
        writer.WriteElementString(nameof(LatestVersion), this.LatestVersion);
        
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}