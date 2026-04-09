using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Settings;
using QRCoder;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static QRCoder.QRCodeGenerator;
#nullable enable
namespace MajdataPlay.Scenes.Login
{
    internal class LoginManager : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _sceneTitle;
        [SerializeField]
        GameObject _usernameComponent;
        [SerializeField]
        GameObject _passwordComponent;
        [SerializeField]
        GameObject _qrCodeComponent;
        [SerializeField]
        GameObject _qrCodeLoading;
        [SerializeField]
        GameObject _qrCodeErrorIcon;

        [SerializeField]
        InputField _usernameInput;
        [SerializeField]
        InputField _passwordInput;
        [SerializeField]
        GameObject _button4;

        [SerializeField]
        GameObject _loading;
        [SerializeField]
        GameObject _hintTextObject;
        [SerializeField]
        TextMeshProUGUI _hintText;
        [SerializeField]
        Color ErrorColor;
        [SerializeField]
        Color SucceedColor;

        RawImage _qrCodeRawImage = null!;
        EventSystem _eventSystem = null!;
        ApiEndpoint[] _apiEndpoints = Array.Empty<ApiEndpoint>();
        ApiEndpoint[] _enabledEndpoints = Array.Empty<ApiEndpoint>();

        bool _isReady = false;
        bool _isExited = false;
        bool _requiresPlayerLogin = false;
        readonly static QRCodeGenerator _qrGenerator = new ();
        readonly static Exception _exception = new();

        public static EndpointRole? TargetRole { get; set; } = null;

        void Awake()
        {
            _apiEndpoints = MajEnv.ApiEndpoints;
            _qrCodeRawImage = _qrCodeComponent.GetComponent<RawImage>();
            _eventSystem = GetComponent<EventSystem>();
            using var rentedApiEndpoints = new RentedList<ApiEndpoint>();
            EndpointRole? roleFilter = TargetRole ?? EndpointRole.Player;
            _requiresPlayerLogin = roleFilter == EndpointRole.Player;
            TargetRole = null;
            for (var i = 0; i < _apiEndpoints.Length; i++)
            {
                var endpoint = _apiEndpoints[i];
                if (roleFilter.HasValue && endpoint.Role != roleFilter.Value)
                {
                    continue;
                }
                rentedApiEndpoints.Add(endpoint);
            }
            _enabledEndpoints = rentedApiEndpoints.ToArray();
            if (_enabledEndpoints.Length == 0)
            {
                if (_requiresPlayerLogin)
                {
                    _loading.SetActive(false);
                    _sceneTitle.text = "Login to\nGeoDanceClub";
                    _qrCodeComponent.SetActive(false);
                    _usernameComponent.SetActive(false);
                    _passwordComponent.SetActive(false);
                    Hint("GeoDance login is required, but no Player endpoint is configured.", true);
                    return;
                }
                MajInstances.SceneSwitcher.SwitchScene("List");
                return;
            }
            _loading.SetActive(false);
            Hint();
            LoginProcessor().Forget();
        }
        void Update()
        {
            if(!_isReady || _isExited)
            {
                return;
            }
            var isUsernameInputClicked = InputManager.IsSensorClickedUpInThisFrame(SensorArea.B2) ||
                                         InputManager.IsSensorClickedUpInThisFrame(SensorArea.B1) ||
                                         InputManager.IsSensorClickedUpInThisFrame(SensorArea.E2);
            var isPasswordInputClicked = InputManager.IsSensorClickedUpInThisFrame(SensorArea.B3) ||
                                         InputManager.IsSensorClickedUpInThisFrame(SensorArea.C) ||
                                         InputManager.IsSensorClickedUpInThisFrame(SensorArea.E3);
            var isUsernameClearBtnClicked = InputManager.IsSensorClickedUpInThisFrame(SensorArea.A2);
            var isPasswordClearBtnClicked = InputManager.IsSensorClickedUpInThisFrame(SensorArea.A3);
            if(isUsernameInputClicked)
            {
                _eventSystem.SetSelectedGameObject(null!);
                _eventSystem.SetSelectedGameObject(_usernameInput.gameObject);
            }
            if(isUsernameClearBtnClicked)
            {
                _eventSystem.SetSelectedGameObject(null!);
                _eventSystem.SetSelectedGameObject(_usernameInput.gameObject);
                _usernameInput.text = string.Empty;
            }
            if (isPasswordInputClicked)
            {
                _eventSystem.SetSelectedGameObject(null!);
                _eventSystem.SetSelectedGameObject(_passwordInput.gameObject);
            }
            if (isPasswordClearBtnClicked)
            {
                _eventSystem.SetSelectedGameObject(null!);
                _eventSystem.SetSelectedGameObject(_passwordInput.gameObject);
                _passwordInput.text = string.Empty;
            }
        }

        async UniTaskVoid LoginProcessor()
        {
            const int AUTH_FLAG_REQUESTING = 0;
            const int AUTH_FLAG_WAIT_FOR_PERMIT = 1;
            const int AUTH_FLAG_ERROR = 2;

            var sceneSwitcher = MajInstances.SceneSwitcher;
            var hasCompletedRequiredPlayerLogin = false;
            for (var i = 0; i < _enabledEndpoints.Length; i++)
            {
                var endpoint = _enabledEndpoints[i];
                _loading.SetActive(false);
                Hint();
                var siteName = endpoint.Name;
                if(string.IsNullOrEmpty(siteName))
                {
                    siteName = endpoint.Url.Host;
                }
                _sceneTitle.text = $"Login to\n{siteName}";

                _qrCodeComponent.SetActive(true);
                _qrCodeLoading.SetActive(true);
                _usernameComponent.SetActive(true);
                _passwordComponent.SetActive(true);

                _qrCodeRawImage.texture = null!;
                _qrCodeRawImage.color = new Color(0.5f, 0.5f, 0.5f);
                _usernameInput.text = endpoint.RuntimeConfig.AuthUsername ?? string.Empty;
                _passwordInput.text = endpoint.RuntimeConfig.AuthPassword ?? string.Empty;
                await sceneSwitcher.FadeOutAsync();
 
                var authSessionTask = default(ValueTask<(string, AuthRequestResponse)>);
                var authCheckTask = default(ValueTask<EndpointResponse>?);
                var authProcessFlag = AUTH_FLAG_REQUESTING;
                var cts = new CancellationTokenSource();
                var authRequestId = string.Empty;
                var authCheckCooldownSec = 2d;
                authSessionTask = RegistryAuthSession(endpoint, cts.Token);
                while (true)
                {
                    _isReady = true;
                    _usernameInput.readOnly = false;
                    _passwordInput.readOnly = false;
                    var isRefreshQRCodeRequested = InputManager.IsSensorClickedUpInThisFrame(SensorArea.A7) ||
                                                   InputManager.IsSensorClickedUpInThisFrame(SensorArea.D7) ||
                                                   InputManager.IsSensorClickedUpInThisFrame(SensorArea.A6) ||
                                                   InputManager.IsSensorClickedUpInThisFrame(SensorArea.B7) ||
                                                   InputManager.IsSensorClickedUpInThisFrame(SensorArea.B6) ||
                                                   InputManager.IsSensorClickedUpInThisFrame(SensorArea.E7);
                    try
                    {
                        if (authProcessFlag == AUTH_FLAG_REQUESTING)
                        {
                            if(authSessionTask.IsCompletedSuccessfully)
                            {
                                var (location, authRsp) = authSessionTask.Result;
                                var qrCodeData = _qrGenerator.CreateQrCode(location, ECCLevel.Q);
                                var qrCode = new PngByteQRCode(qrCodeData);
                                var pngBytes = qrCode.GetGraphic(20,false);
                                var texture = new Texture2D(0, 0);
                                authRequestId = authRsp.RequestId;

                                texture.LoadImage(pngBytes);

                                _qrCodeRawImage.texture = texture;
                                _qrCodeLoading.SetActive(false);
                                _qrCodeErrorIcon.SetActive(false);
                                _qrCodeRawImage.color = Color.white;

                                authProcessFlag = AUTH_FLAG_WAIT_FOR_PERMIT;
                            }
                            else if(authSessionTask.IsCompleted)// Faulted or canceled
                            {
                                authProcessFlag = AUTH_FLAG_ERROR;
                                _qrCodeErrorIcon.SetActive(true);
                                _qrCodeLoading.SetActive(false);
                                MajDebug.LogException(authSessionTask.AsTask().Exception);
                            }
                        }
                        else if (authProcessFlag == AUTH_FLAG_WAIT_FOR_PERMIT)
                        {
                            if(authCheckTask is ValueTask<EndpointResponse> task)
                            {
                                if (task.IsCompleted && !task.IsCompletedSuccessfully)
                                {
                                    MajDebug.LogError("Auth check failed");
                                    MajDebug.LogException(task.AsTask().Exception);
                                }
                                else if(task.IsCompletedSuccessfully)
                                {
                                    var rsp = task.Result;
                                    if (rsp.StatusCode is HttpStatusCode.OK)
                                    {
                                        MajDebug.LogDebug("User has granted authorization");
                                        _loading.SetActive(true);
                                        _isReady = false;
                                        MajDebug.LogDebug("Checking login status");
                                        var getUserInfoTask = FetchUserInfomationAsync(endpoint);
                                        while(!getUserInfoTask.IsCompleted)
                                        {
                                            await UniTask.Yield();
                                        }
                                        MajDebug.LogInfo("Logged in");
                                        endpoint.RuntimeConfig.AuthMethod = NetAuthMethodOption.QRCode;
                                        var userInfo = (UserSummary?)null;
                                        var userScores = Array.Empty<MajNetAccountSongScore>();
                                        if(getUserInfoTask.IsCompletedSuccessfully)
                                        {
                                            userInfo = getUserInfoTask.Result.Summary;
                                            userScores = getUserInfoTask.Result.Scores;
                                        }
                                        ScoreManager.LoadOnlineScores(userScores, endpoint.Name);
                                        await UpdateApiEndpointRuntimeConfigAsync(endpoint, userInfo);
                                        await SyncSettingsFromRemoteAsync(endpoint);
                                        hasCompletedRequiredPlayerLogin |= endpoint.Role == EndpointRole.Player;
                                        break;
                                    }
                                    else if (rsp.StatusCode is HttpStatusCode.Accepted)
                                    {
                                        // Still waiting for user to authorize
                                        authCheckTask = null;
                                        MajDebug.LogDebug("No user has granted authorization");
                                    }
                                    else if(rsp.StatusCode is HttpStatusCode.NotFound)
                                    {
                                        MajDebug.LogDebug("The authorization session has expired.");
                                        isRefreshQRCodeRequested = true;
                                        authCheckTask = null;
                                    }
                                    else
                                    {
                                        MajDebug.LogError($"Auth check returned unexpected status code: {rsp.StatusCode}");
                                        authCheckTask = null;
                                    }
                                }
                            }
                            else
                            {
                                if(authCheckCooldownSec < 0)
                                {
                                    authCheckTask = Online.AuthCheckAsync(endpoint, authRequestId, cts.Token);
                                    authCheckCooldownSec = 2d;
                                    MajDebug.LogDebug("Checking the auth session");
                                }
                                else
                                {
                                    authCheckCooldownSec -= MajTimeline.UnscaledDeltaTime;
                                }                                
                            }
                        }

                        if (isRefreshQRCodeRequested && authProcessFlag is (AUTH_FLAG_WAIT_FOR_PERMIT or AUTH_FLAG_ERROR))
                        {
                            cts.Cancel();
                            cts = new();
                            authProcessFlag = AUTH_FLAG_REQUESTING;
                            _qrCodeErrorIcon.SetActive(false);
                            _qrCodeLoading.SetActive(true);
                            _qrCodeRawImage.texture = null!;
                            _qrCodeRawImage.color = new Color(0.5f, 0.5f, 0.5f);
                            if (!string.IsNullOrEmpty(authRequestId))
                            {
                                await RevokeAuthSession(endpoint, authRequestId);
                            }
                            authSessionTask = RegistryAuthSession(endpoint, cts.Token);
                        }
                        //cancel button
                        if (InputManager.IsSensorClickedUpInThisFrame(SensorArea.A5))
                        {
                            if (_requiresPlayerLogin && endpoint.Role == EndpointRole.Player)
                            {
                                Hint($"{siteName} login is required.", true);
                                continue;
                            }
                            cts.Cancel();
                            if (!string.IsNullOrEmpty(authRequestId))
                            {
                                await sceneSwitcher.FadeInAsync();
                                await RevokeAuthSession(endpoint, authRequestId);
                            }
                            _eventSystem.SetSelectedGameObject(null!);
                            _usernameInput.DeactivateInputField();
                            _passwordInput.DeactivateInputField();
                            break;
                        }
                        //login button
                        else if (InputManager.IsSensorClickedUpInThisFrame(SensorArea.A4) 
                            || (endpoint.AutoLogin == true
                            && SceneSwitcher.LastScene == MajScenes.Title
                            && !string.IsNullOrEmpty(endpoint.Username)
                            && !string.IsNullOrEmpty(endpoint.Password)))
                        {
                            _isReady = false;
                            Hint();
                            _usernameInput.readOnly = true;
                            _passwordInput.readOnly = true;

                            _eventSystem.SetSelectedGameObject(null!);
                            _usernameInput.DeactivateInputField();
                            _passwordInput.DeactivateInputField();

                            var username = _usernameInput.text;
                            var password = _passwordInput.text;
                            MajDebug.LogInfo("Trying to log in via Plain");
                            var task = Online.LoginAsync(endpoint, username, password);

                            _loading.SetActive(true);
                            while (!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }
                            _loading.SetActive(false);
                            if (!task.IsCompletedSuccessfully)
                            {
                                var e = task.AsTask().Exception;
                                MajDebug.LogException(e);
                                Hint(e.ToString(), true);
                                continue;
                            }
                            var rsp = task.Result;
                            if (!rsp.IsSuccessfully)
                            {
                                MajDebug.LogError($"Login failed:\nStatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                                var errMsg = string.Empty;
                                switch(rsp.ErrorCode)
                                {
                                    case HttpErrorCode.Timeout:
                                        errMsg = "MAJTEXT_LOGIN_CONNECT_TIMEOUT";
                                        break;
                                    case HttpErrorCode.InvalidRequest:
                                        errMsg = rsp.Message;
                                        break;
                                    case HttpErrorCode.Unreachable:
                                        errMsg = "MAJTEXT_LOGIN_CONNECT_UNREACHABLE";
                                        break;
                                    case HttpErrorCode.Unsuccessful:
                                        if (rsp.StatusCode is HttpStatusCode.Unauthorized)
                                        {
                                            errMsg = "MAJTEXT_ONLINE_USERNAME_OR_PASSWORD_INCORRECT";
                                        }
                                        else if (rsp.StatusCode is HttpStatusCode.MethodNotAllowed)
                                        {
                                            errMsg = "MAJTEXT_ONLINE_METHOD_NOT_ALLOWED";
                                        }
                                        else
                                        {
                                            errMsg = "MAJTEXT_LOGIN_UNKNOWN_ERROR";
                                        }
                                        break;
                                    default:
                                        errMsg = "MAJTEXT_LOGIN_UNKNOWN_ERROR";
                                        break;
                                }
                                Hint($"{"MAJTEXT_LOGIN_LOGIN_FAILED".i18n()}:\n{errMsg.i18n()}", true);
                                endpoint.AutoLogin = false;
                                continue;
                            }
                            else
                            {
                                MajDebug.LogInfo("Logged in");
                                Hint("MAJTEXT_LOGIN_LOGIN_SUCCESS".i18n(), false);
                                _loading.SetActive(true);
                                var getUserInfoTask = FetchUserInfomationAsync(endpoint);
                                if (!string.IsNullOrEmpty(authRequestId))
                                {
                                    await RevokeAuthSession(endpoint, authRequestId);
                                }
                                while(!getUserInfoTask.IsCompleted)
                                {
                                    await UniTask.Yield();
                                }
                                endpoint.RuntimeConfig.AuthMethod = NetAuthMethodOption.Plain;
                                endpoint.RuntimeConfig.AuthUsername = username;
                                endpoint.RuntimeConfig.AuthPassword = password;
                                var userInfo = (UserSummary?)null;
                                var userScores = Array.Empty<MajNetAccountSongScore>();
                                if (getUserInfoTask.IsCompletedSuccessfully)
                                {
                                    userInfo = getUserInfoTask.Result.Summary;
                                    userScores = getUserInfoTask.Result.Scores;
                                }
                                ScoreManager.LoadOnlineScores(userScores, endpoint.Name);
                                Hint();
                                _loading.SetActive(false);
                                await UpdateApiEndpointRuntimeConfigAsync(endpoint, userInfo);
                                await SyncSettingsFromRemoteAsync(endpoint);
                                hasCompletedRequiredPlayerLogin |= endpoint.Role == EndpointRole.Player;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        await UniTask.Yield();
                    }
                }
                await sceneSwitcher.FadeInAsync();
                _isReady = false;
            }
            if (_requiresPlayerLogin && !hasCompletedRequiredPlayerLogin)
            {
                Hint("GeoDance login is required.", true);
                _isReady = true;
                return;
            }
            EnterList();
        }
        void EnterList()
        {
            if(_isExited)
            {
                return;
            }
            _isExited = true;
            if(SceneSwitcher.LastScene == MajScenes.Title)
            {
                MajInstances.SceneSwitcher.SwitchScene("List", false);
                return;
            }
            RefreshListBackgroundAsync();
        }
        static async void RefreshListBackgroundAsync()
        {
            var sceneSwitcher = MajInstances.SceneSwitcher;
            await sceneSwitcher.FadeInAsync();
            sceneSwitcher.SwitchScene("Empty", false);
            await UniTask.Delay(400);
            var progress = new Progress<string>();
            progress.ProgressChanged += (o, e) =>
            {
                MajInstances.SceneSwitcher.SetLoadingText(e);
            };
            var task = SongStorage.RefreshAsync(progress);
            while (!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (!task.IsCompletedSuccessfully)
            {
                sceneSwitcher.SetLoadingText("MAJTEXT_ERR_SCAN_CHARTS_FAILED".i18n(), Color.red);
            }
            else
            {
                sceneSwitcher.SetLoadingText(string.Empty);
            }
            await UniTask.Delay(3000);
            sceneSwitcher.SwitchScene("List");
        }
        async UniTask SyncSettingsFromRemoteAsync(ApiEndpoint endpoint)
        {
            if (endpoint.Role != EndpointRole.Player)
            {
                return;
            }
            MajDebug.LogInfo("Syncing settings from remote...");
            _loading.SetActive(true);
            Hint("MAJTEXT_LOGIN_SYNCING_SETTINGS".i18n(), false);
            var settingsTask = Online.GetSettingsAsync(endpoint);
            while (!settingsTask.IsCompleted)
            {
                await UniTask.Yield();
            }
            _loading.SetActive(false);
            if (!settingsTask.IsCompletedSuccessfully || settingsTask.Result is null)
            {
                if (settingsTask.IsCompletedSuccessfully && settingsTask.Result is null)
                {
                    MajDebug.LogInfo("No remote settings found, uploading local settings");
                    UploadSettingsAsync(endpoint).Forget();
                }
                else
                {
                    MajDebug.LogWarning("Failed to sync settings from remote");
                }
                Hint();
                return;
            }
            var remoteSettings = settingsTask.Result;
            var localSettings = MajEnv.Settings;
            if (remoteSettings.Game is not null) ApplyGameOptions(localSettings.Game, remoteSettings.Game);
            if (remoteSettings.Judge is not null) ApplyJudgeOptions(localSettings.Judge, remoteSettings.Judge);
            if (remoteSettings.Display is not null) ApplyDisplayOptions(localSettings.Display, remoteSettings.Display);
            if (remoteSettings.Audio is not null) ApplyAudioOptions(localSettings.Audio, remoteSettings.Audio);
            if (remoteSettings.Debug is not null) ApplyDebugOptions(localSettings.Debug, remoteSettings.Debug);
            endpoint.RuntimeConfig.SettingsVersion = remoteSettings.Version;
            MajEnv.SuppressSettingsUpload = true;
            GameManager.RequestSave(this);
            MajEnv.SuppressSettingsUpload = false;
            MajDebug.LogInfo($"Settings synced from remote (version {remoteSettings.Version})");
            Hint();
        }
        static async UniTaskVoid UploadSettingsAsync(ApiEndpoint endpoint)
        {
            var settings = MajEnv.Settings;
            var request = new SettingsSyncRequest
            {
                Version = endpoint.RuntimeConfig.SettingsVersion,
                Game = settings.Game,
                Judge = settings.Judge,
                Display = settings.Display,
                Audio = settings.Audio,
                Debug = settings.Debug,
            };
            var rsp = await Online.PutSettingsAsync(endpoint, request);
            if (rsp is not null)
            {
                endpoint.RuntimeConfig.SettingsVersion = rsp.Version;
                MajDebug.LogInfo($"Settings uploaded (version {rsp.Version})");
            }
        }
        static void ApplyGameOptions(GameOptions target, GameOptions source)
        {
            target.TapSpeed = source.TapSpeed;
            target.TouchSpeed = source.TouchSpeed;
            target.SlideFadeInOffset = source.SlideFadeInOffset;
            target.BackgroundDim = source.BackgroundDim;
            target.StarRotation = source.StarRotation;
            target.BGInfo = source.BGInfo;
            target.TopInfo = source.TopInfo;
            target.TrackSkip = source.TrackSkip;
            target.FastRetry = source.FastRetry;
            target.Mirror = source.Mirror;
            target.Rotation = source.Rotation;
            target.SlideSkipping = source.SlideSkipping;
            target.Random = source.Random;
#if UNITY_ANDROID || UNITY_IOS
            target.ButtonRingForTouch = source.ButtonRingForTouch;
#endif
#if UNITY_STANDALONE
            target.RecordMode = source.RecordMode;
#endif
        }
        static void ApplyJudgeOptions(JudgeOptions target, JudgeOptions source)
        {
            target.AudioOffset = source.AudioOffset;
            target.JudgeOffset = source.JudgeOffset;
            target.AnswerOffset = source.AnswerOffset;
            target.TouchPanelOffset = source.TouchPanelOffset;
            target.Mode = source.Mode;
        }
        static void ApplyDisplayOptions(DisplayOptions target, DisplayOptions source)
        {
            target.Language = source.Language;
            target.Skin = source.Skin;
            target.DisplayCriticalPerfect = source.DisplayCriticalPerfect;
            target.DisplayBreakScore = source.DisplayBreakScore;
            target.FastLateType = source.FastLateType;
            target.NoteJudgeType = source.NoteJudgeType;
            target.TouchJudgeType = source.TouchJudgeType;
            target.SlideJudgeType = source.SlideJudgeType;
            target.BreakJudgeType = source.BreakJudgeType;
            target.BreakFastLateType = source.BreakFastLateType;
            target.SlideSortOrder = source.SlideSortOrder;
            target.OuterJudgeDistance = source.OuterJudgeDistance;
            target.InnerJudgeDistance = source.InnerJudgeDistance;
            target.DisplayHoldHeadJudgeResult = source.DisplayHoldHeadJudgeResult;
            target.TapScale = source.TapScale;
            target.HoldScale = source.HoldScale;
            target.TouchScale = source.TouchScale;
            target.SlideScale = source.SlideScale;
            target.TouchFeedback = source.TouchFeedback;
            target.MainScreenTransform = source.MainScreenTransform;
            target.MainScreenScale = source.MainScreenScale;
            target.MainScreenOffset = source.MainScreenOffset;
            target.MainScreenCachedScreenCenterY = source.MainScreenCachedScreenCenterY;
            target.SubDisplayOffset = source.SubDisplayOffset;
            target.SubDisplayScale = source.SubDisplayScale;
            target.RenderQuality = source.RenderQuality;
            target.FPSLimit = source.FPSLimit;
            target.SkipVideoDownload = source.SkipVideoDownload;
#if UNITY_STANDALONE
            target.Resolution = source.Resolution;
            target.Topmost = source.Topmost;
#endif
#if !(UNITY_ANDROID || UNITY_IOS)
            target.VSync = source.VSync;
#endif
        }
        static void ApplyAudioOptions(SoundOptions target, SoundOptions source)
        {
            target.ForceMono = source.ForceMono;
            target.Backend = source.Backend;
            var tv = target.Volume;
            var sv = source.Volume;
            tv.Global = sv.Global;
            tv.BGM = sv.BGM;
            tv.Track = sv.Track;
            tv.Answer = sv.Answer;
            tv.Tap = sv.Tap;
            tv.Ex = sv.Ex;
            tv.Break = sv.Break;
            tv.Slide = sv.Slide;
            tv.Touch = sv.Touch;
            tv.Hanabi = sv.Hanabi;
            tv.Voice = sv.Voice;
#if !(UNITY_ANDROID || UNITY_IOS)
            target.Wasapi = source.Wasapi;
            target.Asio = source.Asio;
            target.Channel = source.Channel;
#else
            target.Mobile = source.Mobile;
#endif
        }
        static void ApplyDebugOptions(DebugOptions target, DebugOptions source)
        {
            target.DisplaySensor = source.DisplaySensor;
            target.TouchSimulationRadius = source.TouchSimulationRadius;
            target.TouchAAreaExtraRadius = source.TouchAAreaExtraRadius;
            target.TouchBAreaExtraRadius = source.TouchBAreaExtraRadius;
            target.TouchCAreaExtraRadius = source.TouchCAreaExtraRadius;
            target.TouchDAreaExtraRadius = source.TouchDAreaExtraRadius;
            target.TouchEAreaExtraRadius = source.TouchEAreaExtraRadius;
            target.TouchRadiusAdjust = source.TouchRadiusAdjust;
            target.DisplayFPS = source.DisplayFPS;
            target.MenuOptionIterationSpeed = source.MenuOptionIterationSpeed;
            target.DisplayOffset = source.DisplayOffset;
            target.NoteAppearRate = source.NoteAppearRate;
            target.OffsetUnit = source.OffsetUnit;
            target.NoteFolding = source.NoteFolding;
            target.DJAutoPolicy = source.DJAutoPolicy;
            target.MaxQueuedFrames = source.MaxQueuedFrames;
            target.TapPoolCapacity = source.TapPoolCapacity;
            target.HoldPoolCapacity = source.HoldPoolCapacity;
            target.TouchPoolCapacity = source.TouchPoolCapacity;
            target.TouchHoldPoolCapacity = source.TouchHoldPoolCapacity;
            target.EachLinePoolCapacity = source.EachLinePoolCapacity;
            target.DebugLevel = source.DebugLevel;
#if UNITY_STANDALONE
            target.FullScreen = source.FullScreen;
            target.HideCursorInGame = source.HideCursorInGame;
#endif
        }
        async ValueTask<UserInfo> FetchUserInfomationAsync(ApiEndpoint endpoint, CancellationToken token = default)
        {
            var userInfo = await Online.GetUserInfoAsync(endpoint, token);
            var userScores = await Online.GetUserScoresAsync(endpoint, token);

            token.ThrowIfCancellationRequested();
            return new()
            {
                Summary = userInfo,
                Scores = userScores,
            };
        }
        async UniTask UpdateApiEndpointRuntimeConfigAsync(ApiEndpoint endpoint, UserSummary? userInfo)
        {
            var runtimeConfig = endpoint.RuntimeConfig;
            if (userInfo is not null)
            {
                MajDebug.LogInfo("Downloading user avatar...");
                var result = (UserSummary)userInfo;
                _loading.SetActive(true);
                Hint("MAJTEXT_LOGIN_DOWNLOADING_AVATAR".i18n(), false);
                var avatarTask = Online.GetUserIconAsync(endpoint, result.Username);
                while (!avatarTask.IsCompleted)
                {
                    await UniTask.Yield();
                }
                _loading.SetActive(false);
                if (avatarTask.IsCompletedSuccessfully && avatarTask.Result is not null)
                {
                    Hint();
                    runtimeConfig.Avatar = avatarTask.Result;
                    MajDebug.LogInfo("User avatar has been downloaded");
                }
                else
                {
                    Hint("MAJTEXT_LOGIN_DOWNLOADING_AVATAR_FAILED".i18n(), true);
                    MajDebug.LogInfo("Failed to download user avatar");
                }
                runtimeConfig.Username = result.Username;
            }
            else
            {
                runtimeConfig.Avatar = null;
                if(runtimeConfig.AuthMethod == NetAuthMethodOption.Plain)
                {
                    runtimeConfig.Username = runtimeConfig.Username;
                }
                else
                {
                    runtimeConfig.Username = "???";
                }
            }
        }
        async UniTask RevokeAuthSession(ApiEndpoint endpoint, string authId, CancellationToken token = default)
        {
            var revokeTask = Online.AuthRevokeAsync(endpoint, authId);
            while (!revokeTask.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (revokeTask.IsCompletedSuccessfully)
            {
                MajDebug.LogInfo("Successfully revoked the authorization session");
            }
            else if (revokeTask.IsFaulted)
            {
                MajDebug.LogError("Revoking the authorization session failed");
                MajDebug.LogException(revokeTask.AsTask().Exception);
            }
        }
        async ValueTask<(string, AuthRequestResponse)> RegistryAuthSession(ApiEndpoint endpoint, CancellationToken token = default)
        {
            await UniTask.SwitchToThreadPool();
            var rsp = default(EndpointResponse);
            try
            {
                rsp = await Online.RegisterAsync(endpoint, new()
                {
                    Name = "MajdataPlay Client",
                    Description = "MajdataPlay Client QR Code Authentication",
                }, token);
            }
            catch
            {
                MajDebug.LogError("Failed to register QR Code login session");
                throw;
            }
            if(!rsp.IsSuccessfully)
            {
                MajDebug.LogError("Failed to register QR Code login session");
                MajDebug.LogError($"StatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                throw _exception;
            }
            try
            {
                rsp = await Online.AuthRequestAsync(endpoint, token);
            }
            catch
            {
                MajDebug.LogError("Attempt to request authorization session failed");
                throw;
            }
            if (!rsp.IsDeserializable || rsp.StatusCode != HttpStatusCode.Created)
            {
                MajDebug.LogError("Attempt to request authorization session failed");
                MajDebug.LogError($"StatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                throw _exception;
            }
            var location = string.Empty;
            if (rsp.Headers.TryGetValue("Location", out var headers))
            {
                location = headers.FirstOrDefault() ?? string.Empty;
            }
            var e = default(Exception?);
            if (string.IsNullOrEmpty(location) || !rsp.TryDeserialize<AuthRequestResponse?>(out var authRsp, out e) || authRsp is null)
            {
                MajDebug.LogError($"The server returned an invalid response\nEndpoint: {endpoint.Url}\nStatusCode: {rsp.StatusCode}\nErrorCode: {rsp.ErrorCode}\nIsDeserializable: {rsp.IsDeserializable}\nHeaders:\n" + string.Join('\n', rsp.Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}")+ $"\nException: {e}"));
                throw _exception;
            }
            return (location, (AuthRequestResponse)authRsp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Hint(string hintText = "", bool isError = false)
        {
            if (!string.IsNullOrEmpty(hintText)) 
                _hintText.color = isError ? ErrorColor : SucceedColor;
            _hintText.text = hintText;
        }

        readonly struct AuthRequestResponse
        {
            public string RequestId { get; init; }
        }
        readonly struct UserInfo
        {
            public UserSummary? Summary { get; init; }
            public MajNetAccountSongScore[] Scores { get; init; }
        }
    }
}
