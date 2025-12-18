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
using System.Text;
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

        void Awake()
        {
            _apiEndpoints = MajEnv.Settings.Online.ApiEndpoints;
            _qrCodeRawImage = _qrCodeComponent.GetComponent<RawImage>();
            _eventSystem = GetComponent<EventSystem>();

            using var rentedApiEndpoints = new RentedList<ApiEndpoint>();
            for (var i = 0; i < _apiEndpoints.Length; i++)
            {
                var endpoint = _apiEndpoints[i];

                if (endpoint.AuthMethod is not (NetAuthMethodOption.None or NetAuthMethodOption.OAuth))
                {
                    rentedApiEndpoints.Add(endpoint);
                }
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
            var isUsernameInputClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.B8) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B7) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B1) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B2);
            var isPasswordInputClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.B6) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B5) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B4) ||
                                         InputManager.IsSensorClickedInThisFrame(SensorArea.B3);
            var isUsernameClearBtnClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.A2) ||
                                            InputManager.IsSensorClickedInThisFrame(SensorArea.B2) ||
                                            InputManager.IsSensorClickedInThisFrame(SensorArea.E2);
            var isPasswordClearBtnClicked = InputManager.IsSensorClickedInThisFrame(SensorArea.A3) ||
                                            InputManager.IsSensorClickedInThisFrame(SensorArea.B3) ||
                                            InputManager.IsSensorClickedInThisFrame(SensorArea.E4);
            if(isUsernameInputClicked)
            {
                _eventSystem.SetSelectedGameObject(_usernameInput.gameObject);
            }
            if(isUsernameClearBtnClicked)
            {
                _eventSystem.SetSelectedGameObject(_usernameInput.gameObject);
                _usernameInput.text = string.Empty;
            }
            if (isPasswordInputClicked)
            {
                _eventSystem.SetSelectedGameObject(_passwordInput.gameObject);
            }
            if (isPasswordClearBtnClicked)
            {
                _eventSystem.SetSelectedGameObject(_passwordInput.gameObject);
                _passwordInput.text = string.Empty;
            }
        }

        async UniTaskVoid LoginProcessor()
        {
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
                switch(endpoint.AuthMethod)
                {
                    case NetAuthMethodOption.Plain:
                        {
                            _qrCodeComponent.SetActive(false);
                            _usernameComponent.SetActive(true);
                            _passwordComponent.SetActive(true);
                            await sceneSwitcher.FadeOutAsync();
                            _usernameInput.text = endpoint.Username ?? string.Empty;
                            _passwordInput.text = endpoint.Password ?? string.Empty;

                        RETRY:
                            _isReady = true;
                            _usernameInput.readOnly = false;
                            _passwordInput.readOnly = false;
                            while(true)
                            {
                                if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
                                {
                                    goto CONTINUE;
                                }
                                else if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A4))
                                {
                                    break;
                                }
                                await UniTask.Yield();
                            }
                            _isReady = false;
                            _errText.text = string.Empty;
                            _usernameInput.readOnly = true;
                            _passwordInput.readOnly = true;

                            var username = _usernameInput.text;
                            var password = _passwordInput.text;
                            var task = Online.LoginAsync(endpoint, username, password);

                            _loading.SetActive(true);
                            while(!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }
                            _loading.SetActive(false);
                            if(!task.IsCompletedSuccessfully)
                            {
                                var e = task.AsTask().Exception;
                                MajDebug.LogException(e);
                                _errText.text = e.ToString();
                                goto RETRY;
                            }
                            var rsp = task.Result;
                            if(!rsp.IsSuccessfully)
                            {
                                MajDebug.LogError($"Login failed:\nStatusCode:{rsp.StatusCode}\nErrorCode:{rsp.ErrorCode}\nMessage:{rsp.Message}");
                                _errText.text = $"Login failed:\n{rsp.Message.i18n()}";
                                goto RETRY;
                            }
                        }
                        break;
                    case NetAuthMethodOption.QRCode:
                        {
                            _qrCodeComponent.SetActive(true);
                            _qrCodeLoading.SetActive(true);
                            _usernameComponent.SetActive(false);
                            _passwordComponent.SetActive(false);

                            _qrCodeRawImage.texture = null!;
                            _qrCodeRawImage.color = new Color(0.5f, 0.5f, 0.5f);
                            await sceneSwitcher.FadeOutAsync();
                            var task = Online.RegisterAsync(endpoint, new()
                            {
                                Name = "MajdataPlay Client",
                                Description = "MajdataPlay Client QR Code Authentication",
                            });
                            while (!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }
                            if (task.IsFaulted || !task.Result.IsSuccessfully)
                            {
                                _errText.text = "MAJTEXT_ONLINE_MACHINE_REGISTER_FAILED";
                                MajDebug.LogError("Failed to register QR Code login session");
                                if(task.IsFaulted)
                                {
                                    MajDebug.LogException(task.AsTask().Exception);
                                }
                                else
                                {
                                    var tmpRsp = task.Result;
                                    MajDebug.LogError($"StatusCode:{tmpRsp.StatusCode}\nErrorCode:{tmpRsp.ErrorCode}\nMessage:{tmpRsp.Message}");
                                }
                                while (true)
                                {
                                    if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
                                    {
                                        goto CONTINUE;
                                    }
                                    await UniTask.Yield();
                                }
                            }
                        RETRY_AUTH_REQ:
                            task = Online.AuthRequestAsync(endpoint);
                            while (!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }
                            if (task.IsFaulted || !task.Result.IsDeserializable || task.Result.StatusCode != HttpStatusCode.Created)
                            {
                                _errText.text = "MAJTEXT_ONLINE_AUTH_REQ_FAILED";
                                MajDebug.LogError("Attempt to request authorization session failed");
                                MajDebug.LogException(task.AsTask().Exception);
                                while (true)
                                {
                                    if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
                                    {
                                        goto CONTINUE;
                                    }
                                    else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A4))
                                    {
                                        goto RETRY_AUTH_REQ;
                                    }
                                    await UniTask.Yield();
                                }
                            }
                            var location = string.Empty;
                            var rsp = task.Result;
                            if (rsp.Headers.TryGetValue("Location", out var headers))
                            {
                                location = headers.FirstOrDefault() ?? string.Empty;
                            }

                            if (string.IsNullOrEmpty(location) || !rsp.TryDeserialize<AuthRequestResponse?>(out var authRsp) || authRsp is null)
                            {
                                _errText.text = "MAJTEXT_ONLINE_INVALID_RESPONSE";
                                MajDebug.LogError($"The server returned an invalid response\nEndpoint: {endpoint.Url}\nStatusCode: {rsp.StatusCode}\nErrorCode: {rsp.ErrorCode}\nIsDeserializable: {rsp.IsDeserializable}\nHeaders:\n" + string.Join('\n', rsp.Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}")));
                                while (true)
                                {
                                    if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
                                    {
                                        goto CONTINUE;
                                    }
                                    await UniTask.Yield();
                                }
                            }

                            var qrCodeData = _qrGenerator.CreateQrCode(location, ECCLevel.Q);
                            var qrCode = new PngByteQRCode(qrCodeData);
                            var pngBytes = qrCode.GetGraphic(20);
                            var texture = new Texture2D(0, 0);
                            var authRequestId = ((AuthRequestResponse)authRsp).RequestId;

                            texture.LoadImage(pngBytes);

                            _qrCodeRawImage.texture = texture;
                            _qrCodeLoading.SetActive(false);
                            _qrCodeRawImage.color = Color.white;
                        RETRY_AUTH_CHECK:
                            task = Online.AuthCheckAsync(endpoint, authRequestId);
                            while (!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }

                            if (task.IsFaulted)
                            {
                                MajDebug.LogError("Auth check failed");
                                MajDebug.LogException(task.AsTask().Exception);
                                goto RETRY_AUTH_CHECK;
                            }
                            rsp = task.Result;
                            if (rsp.StatusCode is HttpStatusCode.OK)
                            {
                                await Online.CheckLoginAsync(endpoint);
                                goto CONTINUE;
                            }
                            else if (rsp.StatusCode is HttpStatusCode.Accepted)
                            {
                                // Still waiting for user to authorize
                                await UniTask.Delay(2000);
                                goto RETRY_AUTH_CHECK;
                            }
                            else
                            {
                                MajDebug.LogError($"Auth check returned unexpected status code: {rsp.StatusCode}");
                                goto RETRY_AUTH_CHECK;
                            }
                        }
                    default:
                        throw new NotSupportedException();
                }
            CONTINUE:
                await sceneSwitcher.FadeInAsync();
                _isReady = false;
                continue;
            }
            sceneSwitcher.SwitchScene("List", false);
        }
        readonly struct AuthRequestResponse
        {
            public string RequestId { get; init; }
        }
    }
}
