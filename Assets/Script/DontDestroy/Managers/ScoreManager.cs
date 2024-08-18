using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public static class ScoreManager
{
    static string GetHash(string filepath)
    {
        var hashComputer = SHA256.Create();
        using var stream = File.OpenRead(filepath);
        var hash = hashComputer.ComputeHash(stream);

        return Encoding.UTF8.GetString(hash);
    }
}
