using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Utils
{
    public static class HashUtils
    {
        public static string Sha256FromFile(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);

            var hashBytes = sha.ComputeHash(stream);
            return ToHex(hashBytes);
        }

        public static string Sha256FromString(string content)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);

            var hashBytes = sha.ComputeHash(bytes);
            return ToHex(hashBytes);
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}
