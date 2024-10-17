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
        public IHttpProgressReporter? ProgressReporter { get; init; }

        public static DownloadInfo Create(string uri, string savePath)
        {
            return Create(new Uri(uri), savePath, 4, new HttpProgressReporter());
        }
        public static DownloadInfo Create(string uri, string savePath, int maxRetryCount)
        {
            return Create(new Uri(uri), savePath, maxRetryCount, new HttpProgressReporter());
        }
        public static DownloadInfo Create(string uri, string savePath, IHttpProgressReporter? progressReporter)
        {
            return Create(new Uri(uri), savePath, 4, progressReporter);
        }
        public static DownloadInfo Create(string uri, string savePath, int maxRetryCount, IHttpProgressReporter? progressReporter)
        {
            return Create(new Uri(uri), savePath, maxRetryCount, progressReporter);
        }
        public static DownloadInfo Create(Uri uri,string savePath)
        {
            return Create(uri, savePath, 4, new HttpProgressReporter());
        }
        public static DownloadInfo Create(Uri uri, string savePath,int maxRetryCount)
        {
            return Create(uri, savePath, maxRetryCount, new HttpProgressReporter());
        }
        public static DownloadInfo Create(Uri uri, string savePath, IHttpProgressReporter? progressReporter)
        {
            return Create(uri, savePath, 4, progressReporter);
        }
        public static DownloadInfo Create(Uri uri, string savePath, int maxRetryCount , IHttpProgressReporter? progressReporter)
        {
            return new DownloadInfo
            {
                SavePath = savePath,
                RequestAddress = uri,
                MultiThread = false,
                ThreadCount = 0,
                MaxRetryCount = maxRetryCount,
                ProgressReporter = progressReporter
            };
        }
    }
}
