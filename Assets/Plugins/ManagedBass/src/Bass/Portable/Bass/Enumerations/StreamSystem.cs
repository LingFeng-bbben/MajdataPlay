using System;

namespace ManagedBass
{
    /// <summary>
    /// User file system flag to be used with <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)" />
    /// </summary>
    public enum StreamSystem
    {
        /// <summary>
        /// Unbuffered file system (like also used by <see cref="Bass.CreateStream(string,long,long,BassFlags)" />).
        /// <para>
        /// The unbuffered file system is what is used by <see cref="Bass.CreateStream(string,long,long,BassFlags)" />.
        /// In this system, BASS does not do any intermediate buffering - it simply requests data from the file as and when it needs it.
        /// This means that reading (<see cref="FileReadProcedure" />) must be quick,
        /// otherwise the decoding will be delayed and playback Buffer underruns (old data repeated) are a possibility.
        /// It's not so important for seeking (<see cref="FileSeekProcedure" />) to be fast, as that is generally not required during decoding, except when looping a file.
        /// </para>
        /// </summary>
        NoBuffer,

        /// <summary>
        /// Buffered file system (like also used by <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" />).
        /// <para>
        /// The buffered file system is what is used by <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" />.
        /// As the name suggests, data from the file is buffered so that it's readily available for decoding -
        /// BASS creates a thread dedicated to "downloading" the data. This is ideal for when the data is coming from a source that has high latency, like the internet.
        /// It's not possible to seek in buffered file streams, until the download has reached the requested position - it's not possible to seek at all if it's being streamed in blocks.
        /// </para>
        /// </summary>
        Buffer,

        /// <summary>
        /// Buffered, with the data pushed to BASS via <see cref="Bass.StreamPutFileData(int,IntPtr,int)" />.
        /// <para>
        /// The push buffered file system is the same as <see cref="Buffer"/>, except that instead of the file data being pulled from the <see cref="FileReadProcedure" /> function in a "download" thread,
        /// the data is pushed to BASS via <see cref="Bass.StreamPutFileData(int,IntPtr,int)" />.
        /// A <see cref="FileReadProcedure" /> function is still required, to get the initial data used in the creation of the stream.
        /// </para>
        /// </summary>
        BufferPush
    }
}
