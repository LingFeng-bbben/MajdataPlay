using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    internal class NetProgress: INetProgress
    {
        public float Percent { get; private set; }
        long INetProgress.TotalBytes { get; set; }
        long INetProgress.ReadBytes { get; set; }

        readonly SendOrPostCallback _callback;
        readonly SynchronizationContext _synchronizationContext;
        public NetProgress()
        {
            _synchronizationContext = SynchronizationContext.Current;
            _callback = OnReport;
        }

        public void Reset()
        {
            Percent = 0;
        }
        void IProgress<float>.Report(float value)
        {
            _synchronizationContext.Post(_callback, value);
        }
        void OnReport(object value)
        {
            Percent = (float)value;
        }
    }
}
