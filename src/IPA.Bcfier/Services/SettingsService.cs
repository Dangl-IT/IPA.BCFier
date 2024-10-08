﻿using IPA.Bcfier.Models.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IPA.Bcfier.Services
{
    public class SettingsService
    {
        private static Settings? _currentSettings;

        public async Task<Settings> LoadSettingsAsync()
        {
            if (_currentSettings != null)
            {
                return _currentSettings;
            }

            var settingsPath = GetPathToSettingsFile();
            if (!File.Exists(settingsPath))
            {
                return new Settings
                {
                    Username = Environment.UserName
                };
            }

            using var settingsFileStream = File.OpenRead(settingsPath);
            using var streamReader = new StreamReader(settingsFileStream);
            var serializedSettings = await streamReader.ReadToEndAsync();
            var deserializedSettings = JsonConvert.DeserializeObject<Settings>(serializedSettings);

            if (deserializedSettings == null)
            {
                return new Settings
                {
                    Username = Environment.UserName
                };
            }

            _currentSettings = deserializedSettings;

            return deserializedSettings;
        }

        public async Task SaveSettingsAsync(Settings settings)
        {
            _currentSettings = null;

            var serializedSettings = JsonConvert.SerializeObject(settings);
            var settingsFilePath = GetPathToSettingsFile();
            if (File.Exists(settingsFilePath))
            {
                File.Delete(settingsFilePath);
            }

            using var settingsFileStream = File.Create(settingsFilePath);
            using var streamWriter = new StreamWriter(settingsFileStream);
            await streamWriter.WriteAsync(serializedSettings);
        }

        private string GetPathToSettingsFile()
        {
            var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IPA.BCFier",
                "settings.json");

            if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            }

            return settingsPath;
        }
    }
}
