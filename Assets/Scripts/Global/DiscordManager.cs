using DiscordRPC;
using MajdataPlay.Diagnostics;
using MajdataPlay.Scenes.Game;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal static class DiscordManager
    {
        const string APPLICATION_ID = "1491157666423378180";

        static DiscordRpcClient? _client;
        static RichPresence _presence;

        static readonly string _largeImageKey = "majdata";
        static readonly string _largeImageText = "MajdataPlay";

        static readonly Button _buttonViewChart = new() { Label = "View Chart" };

        public static void Init()
        {
            if (_client is not null)
                return;

            _client = new DiscordRpcClient(APPLICATION_ID);
            _client.Initialize();
            _client.OnReady += (sender, e) =>
            {
                MajDebug.LogInfo($"Discord Rich Presence connected as {e.User.Username}");
            };
            _client.OnError += (sender, e) =>
            {
                MajDebug.LogError($"Discord Rich Presence error: {e.Message}");
            };
            _client.OnConnectionEstablished += (sender, e) =>
            {
                MajDebug.LogInfo("Discord Rich Presence connection established");
            };
            _client.OnConnectionFailed += (sender, e) =>
            {
                MajDebug.LogWarning("Discord Rich Presence connection failed (Discord may not be running)");
            };
            SceneSwitcher.OnSceneChanged += OnSceneChanged;
            GameManager.OnAppQuit += OnAppQuit;
        }

        public static void UpdatePresence(MajScenes scene)
        {
            if (_client is null)
                return;

            var (details, state, smallImageKey, smallImageText) = GetSceneInfo(scene);
            _presence = new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new DiscordRPC.Assets()
                {
                    LargeImageKey = _largeImageKey,
                    LargeImageText = _largeImageText,
                    SmallImageKey = smallImageKey,
                    SmallImageText = smallImageText,
                },
                Timestamps = Timestamps.Now,
            };

            if (scene is MajScenes.Game)
            {
                if (Majdata<GameInfo>.Instance is GameInfo info && info.Current is ISongDetail currentSong)
                {
                    UpdateState($"Playing {currentSong.Title} by {currentSong.Artist}");
                    UpdateDetails($"Charted by {currentSong.Designers[(int)info.CurrentLevel]}");
                    if (currentSong is OnlineSongDetail onlineSongDetail)
                    {
                        _buttonViewChart.Url = $"https://majdata.net/song?id={onlineSongDetail.Id}";
                        UpdateButtons(new[] { _buttonViewChart });
                    }
                }
            }
            _client.SetPresence(_presence);
        }

        public static void UpdateDetails(string details, bool setOnce = false)
        {
            if (_client is null)
                return;
            _presence.Details = details;
            if (setOnce)
                _client.SetPresence(_presence);
        }

        public static void UpdateState(string state, bool setOnce = false)
        {
            if (_client is null)
                return;
            _presence.State = state;
            if (setOnce)
                _client.SetPresence(_presence);
        }

        public static void UpdateButtons(Button[] buttons, bool setOnce = false)
        {
            if (_client is null)
                return;
            _presence.Buttons = buttons;
            if (setOnce)
                _client.SetPresence(_presence);
        }

        public static void UpdateParty(Party party, bool setOnce = false)
        {
            if (_client is null)
                return;
            _presence.Party = party;
            if (setOnce)
                _client.SetPresence(_presence);
        }

        static void OnAppQuit(object? sender, EventArgs? args)
        {
            _client?.Dispose();
            _client = null;
            SceneSwitcher.OnSceneChanged -= OnSceneChanged;
            GameManager.OnAppQuit -= OnAppQuit;
        }
        static void OnSceneChanged(object? sender, (MajScenes NewScene, MajScenes OldScene) args)
        {
            UpdatePresence(args.NewScene);
        }
        static (string details, string state, string smallImageKey, string smallImageText) GetSceneInfo(MajScenes scene)
        {
            return scene switch
            {
                MajScenes.Title         => ("Knocking the Door", "Title", "Idle", "Title"),
                MajScenes.Login         => ("Fighting Network", "Login", "Idle", "Login"),
                MajScenes.List          => ("What to Eat?", "Song Select", "Idle", "Song Select"),
                MajScenes.Game          => ("Working Out", "In Game", "Playing", "Playing"),
                MajScenes.Result        => ("Lying on the Cold Hard Ground", "Result", "Result", "Result"),
                MajScenes.Setting       => ("Model Fine-tuning", "Settings", "Idle", "Settings"),
                MajScenes.SortFind      => ("Yeah, What to Eat?", "Sort & Find", "Idle", "Sort & Find"),
                MajScenes.TotalResult   => ("Lying on the Cold Hard Ground", "Results", "Result", "Total Result"),
                MajScenes.Parctice      => ("Involuting", "Practice Mode", "Playing", "Practice"),
                MajScenes.View          => ("Appreciating", "Chart View", "Idle", "View"),
                _                       => ("Idle", "Idle", "Idle", "Idle"),
            };
        }
    }
}
