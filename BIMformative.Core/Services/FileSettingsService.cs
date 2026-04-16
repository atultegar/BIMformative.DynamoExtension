using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BIMformative.Core.Services
{
    public sealed class FileSettingsService : ISettingsService
    {
        private static readonly string SettingsPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BIMformative",
                "settings.json");

        public AppSettings Current { get; private set; } = new AppSettings();

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
                Current = JsonConvert.DeserializeObject<AppSettings>(json)
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
            var dir = Path.GetDirectoryName(SettingsPath);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(
                Current);

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
