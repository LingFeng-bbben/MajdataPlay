using MajdataPlay.IO;
using ManagedBass;
using ManagedBass.Aac;
using ManagedBass.Opus;
using System;

namespace MajdataPlay.Utils
{
    public static class BassHelper
    {
        public static int CreateStream(byte[] buffer, long offset, long length, BassFlags flags)
        {
            var format = 0;
            var decode = 0;

            for (; ; format++ )
            {
                decode = format switch
                {
                    0 => Bass.CreateStream(buffer, offset, length, flags),
                    1 => BassOpus.CreateStream(buffer, offset, length, flags),
                    2 => BassAac.CreateStream(buffer, offset, length, flags),
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (decode == 0)
                {
                    var lastError = Bass.LastError;
                    if (lastError == Errors.FileFormat)
                    {
                        continue;
                    }
                    else
                    {
                        MajDebug.LogError(Bass.LastError);
                        lastError.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    var channelInfo = Bass.ChannelGetInfo(decode);
                    MajDebug.LogDebug($"Decoded format: {channelInfo.ChannelType}");
                    break;
                }
            }
            return decode;
        }
        public static int CreateStream(IntPtr buffer, long offset, long length, BassFlags flags)
        {
            var format = 0;
            var decode = 0;

            for (; ; format++)
            {
                decode = format switch
                {
                    0 => Bass.CreateStream(buffer, offset, length, flags),
                    1 => BassOpus.CreateStream(buffer, offset, length, flags),
                    2 => BassAac.CreateStream(buffer, offset, length, flags),
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (decode == 0)
                {
                    var lastError = Bass.LastError;
                    MajDebug.LogError(Bass.LastError);
                    if (lastError == Errors.FileFormat)
                    {
                        continue;
                    }
                    else
                    {
                        lastError.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    break;
                }
            }
            return decode;
        }
    }
}