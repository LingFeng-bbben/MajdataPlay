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
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QRCoder.QRCodeGenerator;

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
        ApiEndpoint[] _apiEndpoints = Array.Empty<ApiEndpoint>();
        ApiEndpoint[] _enabledEndpoints = Array.Empty<ApiEndpoint>();

        readonly static QRCodeGenerator _qrGenerator = new ();

        void Awake()
        {
            _apiEndpoints = MajEnv.Settings.Online.ApiEndpoints;
            _qrCodeRawImage = _qrCodeComponent.GetComponent<RawImage>();

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

        async UniTaskVoid LoginProcessor()
        {
            var sceneSwitcher = MajInstances.SceneSwitcher;
            for (var i = 0; i < _enabledEndpoints.Length; i++)
            {
                var endpoint = _enabledEndpoints[i];
                _loading.SetActive(false);
                _errText.text = string.Empty;
                switch(endpoint.AuthMethod)
                {
                    case NetAuthMethodOption.Plain:
                        {
                            _qrCodeComponent.SetActive(false);
                            _usernameComponent.SetActive(true);
                            _passwordComponent.SetActive(true);

                            _usernameInput.text = endpoint.Username;
                            _passwordInput.text = endpoint.Password;

                        RETRY:
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
                            _usernameInput.readOnly = true;
                            _passwordInput.readOnly = true;

                            var username = _usernameInput.text;
                            var password = _passwordInput.text;
                            var task = Online.LoginAsync(endpoint, username, password).AsValueTask();

                            _loading.SetActive(true);
                            while(!task.IsCompleted)
                            {
                                await UniTask.Yield();
                            }
                            _loading.SetActive(false);
                            if(!task.IsCompletedSuccessfully)
                            {
                                _errText.text = "";
                                goto RETRY;
                            }
                        }
                        break;
                    case NetAuthMethodOption.QRCode:
                        {
                            _qrCodeComponent.SetActive(true);
                            _usernameComponent.SetActive(false);
                            _passwordComponent.SetActive(false);

                            


                            var qrCodeData = _qrGenerator.CreateQrCode("https://majdata.net", ECCLevel.Q);
                            var qrCode = new PngByteQRCode(qrCodeData);
                            var pngBytes = qrCode.GetGraphic(20);
                            var texture = new Texture2D(0, 0);
                            
                            texture.LoadImage(pngBytes);
                            
                            _qrCodeRawImage.texture = texture;
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            CONTINUE:
                continue;
            }
        }
    }
}
