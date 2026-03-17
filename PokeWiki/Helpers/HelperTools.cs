using System.Security.Cryptography;

namespace MvcCoreCryptography.Helpers
{
    public class HelperTools
    {
        public static string GenerarSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public static bool CompareArrays(byte[] a, byte[] b)
        {
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}
