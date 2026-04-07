using DiscordRPC;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal sealed class DiscordManager
    {
        const string APPLICATION_ID = "1491157666423378180";

        static DiscordRpcClient? _client;
        static RichPresence _presence;

        static readonly string _largeImageKey = "majdata";
        static readonly string _largeImageText = "MajdataPlay";

        public static void Initialize()
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
                Assets = new Assets()
                {
                    LargeImageKey = _largeImageKey,
                    LargeImageText = _largeImageText,
                    SmallImageKey = smallImageKey,
                    SmallImageText = smallImageText,
                },
                Timestamps = Timestamps.Now,
            };
            _client.SetPresence(_presence);
        }

        public static void Dispose()
        {
            if (_client is null)
                return;

            _client.Dispose();
            _client = null;
        }

        static (string details, string state, string smallImageKey, string smallImageText) GetSceneInfo(MajScenes scene)
        {
            return scene switch
            {
                MajScenes.Title => ("In Menu", "Title Screen", "menu", "Title"),
                MajScenes.Login => ("In Menu", "Login", "menu", "Login"),
                MajScenes.List => ("Browsing", "Song Select", "menu", "Song Select"),
                MajScenes.Game => ("Playing", "In Game", "playing", "Playing"),
                MajScenes.Result => ("Viewing Results", "Result Screen", "result", "Result"),
                MajScenes.Setting => ("In Menu", "Settings", "menu", "Settings"),
                MajScenes.SortFind => ("Browsing", "Sort & Find", "menu", "Sort & Find"),
                MajScenes.TotalResult => ("Viewing Results", "Total Results", "result", "Total Result"),
                MajScenes.Parctice => ("Playing", "Practice Mode", "playing", "Practice"),
                MajScenes.View => ("Viewing", "Chart View", "menu", "View"),
                _ => ("In Menu", "Idle", "menu", "Idle"),
            };
        }
    }
}
