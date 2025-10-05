using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Peaking Equalizer Effect.
    /// </summary>
    public sealed class PeakEQ : IDisposable
    {
        readonly PeakEQParameters _parameters;
        readonly int _channel, _handle;
        GCHandle _gch;

        /// <summary>
        /// Creates a new instance of <see cref="PeakEQ"/>.
        /// </summary>
        public PeakEQ(int Channel, double Q = 0, double Bandwith = 2.5)
        {
            _channel = Channel;
            _handle = Bass.ChannelSetFX(Channel, EffectType.PeakEQ, 0);

            _parameters = new PeakEQParameters
            {
                lBand = -1,
                fQ = (float)Q,
                fBandwidth = (float)Bandwith
            };

            _gch = GCHandle.Alloc(_parameters, GCHandleType.Pinned);
        }

        /// <summary>
        /// Adds a Band.
        /// </summary>
        /// <param name="CenterFrequency">The Band's center frequency in Hz. Default = 1000. Max = 1/2 of SampleRate.</param>
        /// <returns>The Band Index to be used with <see cref="UpdateBand"/>.</returns>
        public int AddBand(double CenterFrequency = 1000)
        {
            ++_parameters.lBand;

            _parameters.fCenter = (float)CenterFrequency;
            _parameters.fGain = 0;

            Bass.FXSetParameters(_handle, _gch.AddrOfPinnedObject());

            return _parameters.lBand;
        }

        /// <summary>
        /// Updates a Band.
        /// </summary>
        /// <param name="Band">The Index of the Band to Update (as returned by <see cref="AddBand"/>).</param>
        /// <param name="Gain">The new Gain value for the Band (-15 ... 0 ... 15).</param>
        public void UpdateBand(int Band, double Gain)
        {
            var cur = _parameters.lBand;

            _parameters.lBand = Band;

            Bass.FXGetParameters(_handle, _gch.AddrOfPinnedObject());

            _parameters.fGain = (float)Gain;

            Bass.FXSetParameters(_handle, _gch.AddrOfPinnedObject());

            _parameters.lBand = cur;
        }

        /// <summary>
        /// Frees all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            _gch.Free();
            Bass.ChannelRemoveFX(_channel, _handle);
        }
    }
}