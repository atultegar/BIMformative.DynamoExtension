using BIMformative.DynamoExtension.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Settings
{
    public sealed class FileSettingsService : ISettingsService
    {
        private static readonly string SettingsPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BIMformative",
                "settings.json");

        public AppSettings Current { get; private set; } = new();

        public void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    SetDefaults();
                    Save();
                    return;
                }

                var json = File.ReadAllText(SettingsPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json)
                    ?? new AppSettings();

                ApplyFallbacks();
            }
            catch
            {
                SetDefaults();
            }
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(
                Current,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(SettingsPath, json);
        }

        public void Reset()
        {
            SetDefaults();
            Save();
        }

        private void SetDefaults()
        {
            Current = new AppSettings
            {
                DefaultScriptSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
        }

        private void ApplyFallbacks()
        {
            if (string.IsNullOrWhiteSpace(Current.DefaultScriptSavePath))
            {
                Current.DefaultScriptSavePath =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

    }
}
