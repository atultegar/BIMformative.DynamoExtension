using System.Security.Cryptography;
using System.Text;

namespace BIMformative.DynamoExtension.Infrastructure.Security
{
    public class DpapiProtector
    {
        private static readonly byte[] Entropy =
            Encoding.UTF8.GetBytes("BIMformative.Auth.v1");

        public static byte[] Protect(byte[] data)
        {
            return ProtectedData.Protect(
                data,
                Entropy,
                DataProtectionScope.CurrentUser
            );
        }

        public static byte[] Unprotect(byte[] data)
        {
            return ProtectedData.Unprotect(
                data,
                Entropy,
                DataProtectionScope.CurrentUser
            );
        }
    }
}
