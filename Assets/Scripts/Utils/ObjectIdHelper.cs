using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    internal static class ObjectIdHelper
    {
        public static Guid ToGuid(string oid)
        {
            Span<byte> hash = stackalloc byte[16];

            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(oid));

            bytes.AsSpan(0, 16).CopyTo(hash);

            return new Guid(hash);
        }
    }
}
