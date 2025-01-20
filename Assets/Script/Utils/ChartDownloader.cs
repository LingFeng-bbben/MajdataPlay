using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Threading;
using UnityEngine;


public class ChartDownloader
{
    /// <summary>
    /// Dump online chart to local
    /// </summary>
    /// <returns></returns>
    public static async UniTask<SongDetail> DumpOnlineChart(SongDetail _songDetail, CancellationTokenSource _cts)
    {
        var chartFolder = Path.Combine(MajEnv.ChartPath, $"MajnetPlayed/{_songDetail.Hash.Replace('/', '_')}");
        Directory.CreateDirectory(chartFolder);
        var dirInfo = new DirectoryInfo(chartFolder);
        var trackPath = Path.Combine(chartFolder, "track.mp3");
        var chartPath = Path.Combine(chartFolder, "maidata.txt");
        var bgPath = Path.Combine(chartFolder, "bg.png");
        var videoPath = Path.Combine(chartFolder, "bg.mp4");
        var trackUri = _songDetail.TrackPath;
        var chartUri = _songDetail.MaidataPath;
        var bgUri = _songDetail.BGPath;
        var videoUri = _songDetail.VideoPath;
        var token = _cts.Token;

        if (trackUri is null or "")
            throw new AudioTrackNotFoundException(trackPath);
        if (chartUri is null or "")
            throw new ChartNotFoundException(_songDetail);

        MajInstances.LightManager.SetAllLight(Color.blue);
        MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading")}...");
        await UniTask.Yield();
        if (!File.Exists(trackPath))
        {
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...");
            await UniTask.Yield();
            await DownloadFile(trackUri, trackPath, _cts);
        }
        token.ThrowIfCancellationRequested();
        if (!File.Exists(chartPath))
        {
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...");
            await UniTask.Yield();
            await DownloadFile(chartUri, chartPath, _cts);
        }
        token.ThrowIfCancellationRequested();
        SongDetail song;
        token.ThrowIfCancellationRequested();
        if (!File.Exists(bgPath))
        {
            try
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...");
                await UniTask.Yield();
                await DownloadFile(bgUri, bgPath, _cts);
            }
            catch
            {
                MajDebug.Log("No video for this song");
                File.Delete(videoPath);
                videoPath = "";
                MajInstances.SceneSwitcher.SetLoadingText("");
            }
        }
        token.ThrowIfCancellationRequested();
        if (!File.Exists(videoPath) && videoUri is not null)
        {
            try
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...");
                await UniTask.Yield();
                await DownloadFile(videoUri, videoPath, _cts);
            }
            catch
            {
                MajDebug.Log("No video for this song");
                File.Delete(videoPath);
                videoPath = "";
                MajInstances.SceneSwitcher.SetLoadingText("");
            }
        }
        token.ThrowIfCancellationRequested();
        song = await SongDetail.ParseAsync(dirInfo.GetFiles());
        song.Hash = _songDetail.Hash;
        song.OnlineId = _songDetail.OnlineId;
        song.ApiEndpoint = _songDetail.ApiEndpoint;
        MajInstances.SceneSwitcher.SetLoadingText("");
        await UniTask.Yield();
        return song;
    }
    static async UniTask DownloadFile(string uri, string savePath, CancellationTokenSource _cts)
    {
        var task = HttpTransporter.ShareClient.GetByteArrayAsync(uri);
        var token = _cts.Token;
        while (!task.IsCompleted)
        {
            token.ThrowIfCancellationRequested();
            await UniTask.Yield();
        }
        if (task.IsCanceled)
        {
            throw new Exception("Download failed");
        }
        File.WriteAllBytes(savePath, task.Result);
        return;
    }

    /*async UniTask<GetResult> DownloadFile(string uri,string savePath,Action<IHttpProgressReporter> onProgressChanged,int buffersize = 128*1024)
{
var dlInfo = GetRequest.Create(uri, savePath);
var reporter = dlInfo.ProgressReporter;
var task = _httpDownloader.GetAsync(dlInfo,buffersize);

while(!task.IsCompleted)
{
    onProgressChanged(reporter!);
    await UniTask.Yield();
}
onProgressChanged(reporter!);
await UniTask.Yield();
return task.Result;
}*/
    /*async UniTask DownloadString(string uri, string savePath)
    {
        var task = HttpTransporter.ShareClient.GetStringAsync(uri);

        while (!task.IsCompleted)
        {
            await UniTask.Yield();
        }
        File.WriteAllText(savePath, task.Result);
        return;
    }*/
    /*async UniTask DownloadFile(string uri, string savePath, Action<float> progressCallback)
    {
        UnityWebRequest trackreq = UnityWebRequest.Get(uri);
        trackreq.downloadHandler = new DownloadHandlerFile(savePath);
        var result = trackreq.SendWebRequest();
        while (!result.isDone)
        {
            progressCallback.Invoke(trackreq.downloadProgress);
            await UniTask.Yield();
        }
        if (trackreq.result != UnityWebRequest.Result.Success)
        {
            MajDebug.LogError("Error downloading file: " + trackreq.error);
            throw new Exception("Download file failed");
        }
    }*/
}

