using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Extensions
{
    public static class ErrorsExtensions
    {
        public static void EnsureSuccessStatusCode(this Errors errCode)
        {
            if (errCode == Errors.OK)
                return;

            throw new BassException(errCode);
        }
    }
}
