using System;
using System.Security.Cryptography;
using System.Text;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal static class DeterministicGuid
    {
        // Returns a deterministic GUID string derived from the test name and optional iteration.
        // Using string (not System.Guid) because JsonUtility does not support Guid serialization.
        public static string Create(string name, int iteration = 0, string salt = null)
        {
            var input = iteration > 0 ? $"{name}:{iteration}" : (name ?? string.Empty);
            if (salt != null)
                input = $"{salt}:{input}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes).ToString();
        }
    }
}
