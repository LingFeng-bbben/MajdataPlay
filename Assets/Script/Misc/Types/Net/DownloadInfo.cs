using System;

#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct DownloadInfo
    {
        public string SavePath { get; init; }
        public Uri RequestAddress { get; init; }
        public bool MultiThread { get; init; }
        public int ThreadCount { get; init; }
        public int MaxRetryCount { get; init; }
        public Action<DLProgress>? OnProgressUpdated { get; init; }

        public static DownloadInfo Create(string uri, string savePath)
        {
            return Create(new Uri(uri), savePath, 4, null);
        }
        public static DownloadInfo Create(string uri, string savePath, int maxRetryCount)
        {
            return Create(new Uri(uri), savePath, maxRetryCount, null);
        }
        public static DownloadInfo Create(string uri, string savePath, Action<DLProgress>? onProgressUpdated)
        {
            return Create(new Uri(uri), savePath, 4, onProgressUpdated);
        }
        public static DownloadInfo Create(string uri, string savePath, int maxRetryCount, Action<DLProgress>? onProgressUpdated)
        {
            return Create(new Uri(uri), savePath, maxRetryCount, onProgressUpdated);
        }
        public static DownloadInfo Create(Uri uri,string savePath)
        {
            return Create(uri, savePath, 4, null);
        }
        public static DownloadInfo Create(Uri uri, string savePath,int maxRetryCount)
        {
            return Create(uri, savePath, maxRetryCount, null);
        }
        public static DownloadInfo Create(Uri uri, string savePath, Action<DLProgress>? onProgressUpdated)
        {
            return Create(uri, savePath, 4, onProgressUpdated);
        }
        public static DownloadInfo Create(Uri uri, string savePath, int maxRetryCount ,Action<DLProgress>? onProgressUpdated)
        {
            return new DownloadInfo
            {
                SavePath = savePath,
                RequestAddress = uri,
                MultiThread = false,
                ThreadCount = 0,
                MaxRetryCount = maxRetryCount,
                OnProgressUpdated = onProgressUpdated
            };
        }
    }
}
