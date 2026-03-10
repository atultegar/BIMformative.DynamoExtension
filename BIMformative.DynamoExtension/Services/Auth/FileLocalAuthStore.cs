using BIMformative.DynamoExtension.Infrastructure.Security;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class FileLocalAuthStore : ILocalAuthStore
    {
        private static readonly string Folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BIMformative");

        private static readonly string FilePath =
            Path.Combine(Folder, "auth.dat");

        public async Task SaveAsync(AuthCache cache)
        {
            Directory.CreateDirectory(Folder);

            var json = JsonSerializer.Serialize(cache);
            var plaintext = System.Text.Encoding.UTF8.GetBytes(json);

            var encrypted = DpapiProtector.Protect(plaintext);

            await File.WriteAllBytesAsync(FilePath, encrypted);
        }

        public async Task<AuthCache?> LoadAsync()
        {
            if (!File.Exists(FilePath))
                return null;

            try
            {
                var encrypted = await File.ReadAllBytesAsync(FilePath);
                var decrypted = DpapiProtector.Unprotect(encrypted);

                var json = System.Text.Encoding.UTF8.GetString(decrypted);
                return JsonSerializer.Deserialize<AuthCache>(json);
            }
            catch
            {
                await ClearAsync();
                return null;
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
