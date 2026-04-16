using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Auth;
using BIMformative.Core.Security;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class LocalAuthStore : ILocalAuthStore
    {
        private static readonly string Folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BIMformative");

        private static readonly string FilePath =
            Path.Combine(Folder, "auth.dat");

        public Task SaveAsync(AuthCache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            Directory.CreateDirectory(Folder);

            string json = JsonConvert.SerializeObject(cache);
            byte[] plaintext = Encoding.UTF8.GetBytes(json);

            byte[] encrypted = DpapiProtector.Protect(plaintext);

            File.WriteAllBytes(FilePath, encrypted);

            return Task.CompletedTask;
        }

        public Task<AuthCache> LoadAsync()
        {
            if (!File.Exists(FilePath))
                return Task.FromResult<AuthCache>(null);

            try
            {
                var encrypted = File.ReadAllBytes(FilePath);
                var decrypted = DpapiProtector.Unprotect(encrypted);

                var json = Encoding.UTF8.GetString(decrypted);
                var result = JsonConvert.DeserializeObject<AuthCache>(json);

                return Task.FromResult(result);
            }
            catch
            {
                ClearAsync();
                return Task.FromResult<AuthCache>(null);
            }
        }

        public Task ClearAsync()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);

            return Task.CompletedTask;
        }
    }
}
