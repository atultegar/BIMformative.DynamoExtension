using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Auth;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class LocalAuthStore : ILocalAuthStore
    {
        private readonly IAppLogger _logger;

        private static readonly string Folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BIMformative");

        private static readonly string FilePath =
            Path.Combine(Folder, "auth.json");

        public LocalAuthStore(IAppLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SaveAsync(AuthCache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            Directory.CreateDirectory(Folder);

            string json = JsonConvert.SerializeObject(cache, Formatting.Indented);

            File.WriteAllText(FilePath, json, Encoding.UTF8);

            _logger.Info("Auth cache saved:" + FilePath);

            return Task.CompletedTask;
        }

        public Task<AuthCache> LoadAsync()
        {
            if (!File.Exists(FilePath))
                return Task.FromResult<AuthCache>(null);

            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                var result = JsonConvert.DeserializeObject<AuthCache>(json);

                if (result == null)
                    throw new InvalidOperationException("Deserialized auth cache is null.");

                _logger.Info("Auth cache loaded successfully");

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load auth cache: " + ex);
                
                return Task.FromResult<AuthCache>(null);
            }
        }

        public Task ClearAsync()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                _logger.Info("Auth cache deleted.");
            }
                

            return Task.CompletedTask;
        }
    }
}
