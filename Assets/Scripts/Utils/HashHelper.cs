using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    internal static class HashHelper
    {
        public static async Task<byte[]> ComputeHashAsync(byte[] data)
        {
            using var md5 = MD5.Create();
            return await Task.Run(() =>
            {
                return md5.ComputeHash(data);
            });
        }
        public static async Task<byte[]> ComputeHashAsync(byte[] data,int offset, int count)
        {
            using var md5 = MD5.Create();
            return await Task.Run(() =>
            {
                return md5.ComputeHash(data, offset, count);
            });
        }
        public static async Task<byte[]> ComputeHashAsync(Stream inputStream)
        {
            using var md5 = MD5.Create();
            return await Task.Run(() =>
            {
                return md5.ComputeHash(inputStream);
            });
        }
        public static async Task<string> ComputeHashAsBase64StringAsync(byte[] data)
        {
            var hash = await ComputeHashAsync(data);

            return Convert.ToBase64String(hash);
        }
        public static async Task<string> ComputeHashAsBase64StringAsync(byte[] data, int offset, int count)
        {
            var hash = await ComputeHashAsync(data, offset, count);

            return Convert.ToBase64String(hash);
        }
        public static async Task<string> ComputeHashAsBase64StringAsync(Stream inputStream)
        {
            var hash = await ComputeHashAsync(inputStream);

            return Convert.ToBase64String(hash);
        }

        public static byte[] ComputeHash(byte[] data)
        {
            return ComputeHashAsync(data).Result;
        }
        public static byte[] ComputeHash(byte[] data, int offset, int count)
        {
            return ComputeHashAsync(data, offset, count).Result;
        }
        public static byte[] ComputeHash(Stream inputStream)
        {
            return ComputeHashAsync(inputStream).Result;
        }
        public static string ComputeHashAsBase64String(byte[] data)
        {
            return ComputeHashAsBase64StringAsync(data).Result;
        }
        public static string ComputeHashAsBase64String(byte[] data, int offset, int count)
        {
            return ComputeHashAsBase64StringAsync(data, offset, count).Result;
        }
        public static string ComputeHashAsBase64String(Stream inputStream)
        {
            return ComputeHashAsBase64StringAsync(inputStream).Result;
        }

        public static string ToHexString(byte[] hash) => BitConverter.ToString(hash);
        public static byte[] FromHexString(in ReadOnlySpan<char> hexStr)
        {
            if (hexStr.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length");

            var result = new byte[hexStr.Length / 2];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (byte)((GetHexValue(hexStr[i * 2]) << 4) | GetHexValue(hexStr[i * 2 + 1]));
            }

            return result;
        }
        static int GetHexValue(in char c)
        {
            return c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => throw new ArgumentException($"Invalid hex character: {c}")
            };
        }
    }
}
