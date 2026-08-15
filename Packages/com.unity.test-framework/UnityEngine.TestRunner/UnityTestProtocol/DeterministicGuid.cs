using System;
using System.Security.Cryptography;
using System.Text;

namespace UnityEngine.TestRunner.TestProtocol
{
    internal static class DeterministicGuid
    {
        // Returns a deterministic GUID string derived from the test name and optional iteration.
        // Using string (not System.Guid) because JsonUtility does not support Guid serialization.
        private static readonly string ProcessSalt = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

        public static string Create(string name, int iteration = 0)
        {
            var input = iteration > 0 ? $"{name}:{iteration}" : (name ?? string.Empty);
            input = $"{ProcessSalt}:{input}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes).ToString();
        }
    }
}
