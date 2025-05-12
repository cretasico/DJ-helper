using System;
using System.IO;
using System.Xml.Linq;
using System.Windows;

namespace DJ_WPF.Model
{
    public class Config
    {
        // Propiedades para los valores de configuración
        public string ExcelPath { get; set; }
        public string ExcelPathOld { get; set; }
        public string Source { get; set; }
        public string SpotifyAPIKey { get; set; }
        public string RatingLevelNormal { get; set; }
        public string RatingLevelGood { get; set; }
        public string RatingLevelSuper { get; set; }

        private static string GetDefaultConfigPath()
        {
            try
            {
                string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                return Path.Combine(projectPath, "config.xml");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error getting the configuration file path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static Config Load(string customPath = null)
        {
            string configFilePath = customPath ?? GetDefaultConfigPath();
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {configFilePath}");
            }

            XDocument doc = XDocument.Load(configFilePath);
            XElement root = doc.Root;

            return new Config
            {
                ExcelPath = root.Element("ExcelPath")?.Value,
                ExcelPathOld = root.Element("ExcelPath_Old")?.Value,
                Source = root.Element("Source")?.Value,
                SpotifyAPIKey = root.Element("SpotifyAPIKey")?.Value,
                RatingLevelNormal = root.Element("RatingScale")?.Element("Level_Normal")?.Value,
                RatingLevelGood = root.Element("RatingScale")?.Element("Level_Good")?.Value,
                RatingLevelSuper = root.Element("RatingScale")?.Element("Level_Super")?.Value
            };
        }

        public void Save(string customPath = null)
        {
            string configFilePath = customPath ?? GetDefaultConfigPath();

            var doc = new XDocument(
                new XElement("Configuration",
                    new XElement("ExcelPath", ExcelPath ?? ""),
                    new XElement("ExcelPath_Old", ExcelPathOld ?? ""),
                    new XElement("Source", Source ?? ""),
                    new XElement("SpotifyAPIKey", SpotifyAPIKey ?? ""),
                    new XElement("RatingScale",
                        new XElement("Level_Normal", RatingLevelNormal ?? ""),
                        new XElement("Level_Good", RatingLevelGood ?? ""),
                        new XElement("Level_Super", RatingLevelSuper ?? "")
                    )
                )
            );

            doc.Save(configFilePath);
        }

        // Acceso puntual a valores si se necesita algo dinámico:
        public static string GetValue(string key)
        {
            string path = GetDefaultConfigPath();
            if (!File.Exists(path)) return null;
            XDocument doc = XDocument.Load(path);
            return doc.Root.Element(key)?.Value;
        }

        public static void SetValue(string key, string value)
        {
            string path = GetDefaultConfigPath();
            XDocument doc = XDocument.Load(path);
            XElement element = doc.Root.Element(key);
            if (element != null)
                element.Value = value;
            else
                doc.Root.Add(new XElement(key, value));

            doc.Save(path);
        }

        public static string GetNestedValue(string parentKey, string childKey)
        {
            string path = GetDefaultConfigPath();
            XDocument doc = XDocument.Load(path);
            XElement parent = doc.Root.Element(parentKey);
            return parent?.Element(childKey)?.Value;
        }

        public static void SetNestedValue(string parentKey, string childKey, string value)
        {
            string path = GetDefaultConfigPath();
            XDocument doc = XDocument.Load(path);
            XElement parent = doc.Root.Element(parentKey);
            if (parent == null)
            {
                parent = new XElement(parentKey);
                doc.Root.Add(parent);
            }

            XElement child = parent.Element(childKey);
            if (child != null)
                child.Value = value;
            else
                parent.Add(new XElement(childKey, value));

            doc.Save(path);
        }
    }
}
