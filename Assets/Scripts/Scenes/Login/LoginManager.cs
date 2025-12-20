using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        InputField _usernameInput;
        [SerializeField]
        InputField _passwordInput;
        [SerializeField]
        GameObject _button4;

        [SerializeField]
        GameObject _loading;
        [SerializeField]
        GameObject _errTextObject;
        [SerializeField]
        TextMeshProUGUI _errText;

        RawImage _qrCodeRawImage = null!;
        EventSystem _eventSystem = null!;
        ApiEndpoint[] _apiEndpoints = Array.Empty<ApiEndpoint>();
        ApiEndpoint[] _enabledEndpoints = Array.Empty<ApiEndpoint>();

        bool _isReady = false;
        readonly static QRCodeGenerator _qrGenerator = new ();
        readonly static Exception _exception = new();

        void Awake()
        {
            _apiEndpoints = MajEnv.Settings.Online.ApiEndpoints;
            _qrCodeRawImage = _qrCodeComponent.GetComponent<RawImage>();
            _eventSystem = GetComponent<EventSystem>();

            using var rentedApiEndpoints = new RentedList<ApiEndpoint>();
            for (var i = 0; i < _apiEndpoints.Length; i++)
            {
                var endpoint = _apiEndpoints[i];

                rentedApiEndpoints.Add(endpoint);
            }
            _enabledEndpoints = rentedApiEndpoints.ToArray();
            if (_enabledEndpoints.Length == 0)
            {
                MajInstances.SceneSwitcher.SwitchScene("List");
                return;
            }
            _loading.SetActive(false);
            _errText.text = string.Empty;
            LoginProcessor().Forget();
        }
        void Update()
        {
            if(!_isReady)
            {
                return;
            }
            var isUsernameInputClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.B2) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B1) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.E2);
            var isPasswordInputClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.B3);
            var isUsernameClearBtnClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.A2);
            var isPasswordClearBtnClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.A3);
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
            for (var i = 0; i < _enabledEndpoints.Length; i++)
            {
                var endpoint = _enabledEndpoints[i];
                _loading.SetActive(false);
                _errText.text = string.Empty;
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
                _usernameInput.text = endpoint.Username ?? string.Empty;
                _passwordInput.text = endpoint.Password ?? string.Empty;
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
                    var isRefreshQRCodeRequested = InputManager.IsSensorClickedInThisFrame(SensorArea.A7) ||
                                                   InputManager.IsSensorClickedInThisFrame(SensorArea.D7) ||
                                                   InputManager.IsSensorClickedInThisFrame(SensorArea.A6) ||
                                                   InputManager.IsSensorClickedInThisFrame(SensorArea.B7) ||
                                                   InputManager.IsSensorClickedInThisFrame(SensorArea.B6) ||
                                                   InputManager.IsSensorClickedInThisFrame(SensorArea.E7);
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
                                _qrCodeRawImage.color = Color.white;

                                authProcessFlag = AUTH_FLAG_WAIT_FOR_PERMIT;
                            }
                            else if(authSessionTask.IsCompleted)// Faulted or canceled
                            {
                                authProcessFlag = AUTH_FLAG_ERROR;
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
                                        var checkLoginTask = Online.CheckLoginAsync(endpoint);
                                        while(!checkLoginTask.IsCompleted)
                                        {
                                            await UniTask.Yield();
                                        }
                                        MajDebug.LogInfo("Logged in");
                                        endpoint.AuthMethod = NetAuthMethodOption.QRCode;
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
                        if (InputManager.IsSensorClickedInThisFrame(SensorArea.B5) ||
                            InputManager.IsSensorClickedInThisFrame(SensorArea.E6))
                        {
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
                        else if (InputManager.IsSensorClickedInThisFrame(SensorArea.B4) ||
                                 InputManager.IsSensorClickedInThisFrame(SensorArea.E4))
                        {
                            _isReady = false;
                            _errText.text = string.Empty;
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
                                _errText.text = e.ToString();
                                continue;
                            }
                            var rsp = task.Result;
                            if (!rsp.IsSuccessfully)
                            {
                                MajDebug.LogError($"Login failed:\nStatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                                _errText.text = $"Login failed:\n{rsp.Message.i18n()}";
                                continue;
                            }
                            else
                            {
                                MajDebug.LogInfo("Logged in");
                                var checkLoginTask = Online.CheckLoginAsync(endpoint);
                                if (!string.IsNullOrEmpty(authRequestId))
                                {
                                    await RevokeAuthSession(endpoint, authRequestId);
                                }
                                while(!checkLoginTask.IsCompleted)
                                {
                                    await UniTask.Yield();
                                }
                                endpoint.AuthMethod = NetAuthMethodOption.Plain;
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
            sceneSwitcher.SwitchScene("List", false);
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

            if (string.IsNullOrEmpty(location) || !rsp.TryDeserialize<AuthRequestResponse?>(out var authRsp) || authRsp is null)
            {
                MajDebug.LogError($"The server returned an invalid response\nEndpoint: {endpoint.Url}\nStatusCode: {rsp.StatusCode}\nErrorCode: {rsp.ErrorCode}\nIsDeserializable: {rsp.IsDeserializable}\nHeaders:\n" + string.Join('\n', rsp.Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}")));
                throw _exception;
            }
            return (location, (AuthRequestResponse)authRsp);
        }
        readonly struct AuthRequestResponse
        {
            public string RequestId { get; init; }
        }
    }
}
