using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
#nullable enable
namespace MajdataPlay.Net
{
    public class NetProgress: INetProgress
    {
        public float Percent
        {
            get => Volatile.Read(ref _percent);
        }
        public long TotalBytes
        {
            get => Volatile.Read(ref _totalBytes);
        }
        public long TransferredBytes
        {
            get => Volatile.Read(ref _transferredBytes);
        }

        long INetProgress.TotalBytes
        {
            get => Volatile.Read(ref _totalBytes);
            set => Volatile.Write(ref _totalBytes, value);
        }
        long INetProgress.TransferredBytes
        {
            get => Volatile.Read(ref _transferredBytes);
            set => Volatile.Write(ref _transferredBytes, value);
        }

        long _totalBytes;
        long _transferredBytes;
        float _percent;

        public void Reset()
        {
            Volatile.Write(ref _percent, 0);
        }
        void IProgress<float>.Report(float value)
        {
            Volatile.Write(ref _percent, (float)value);
        }
    }
}
