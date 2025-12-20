using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using System.Threading;
using MajdataPlay.Utils;
using System;
using UnityEngine.Networking;
using System.Net;

namespace MajdataPlay
{
    public class UserInfoDisplayer : MonoBehaviour
    {
        public TMP_Text username_text;
        public Image usericon;
        public GameObject Loading_icon;
        public GameObject Error_icon;
        
        public void DisplayUserInfo(ApiEndpoint apiEndpoint)
        {
            _displayUserInfoInternal(apiEndpoint).Forget();
        }

        public void DisplayFromSong(ISongDetail song)
        {
            if (song is not OnlineSongDetail)
            {
                this.gameObject.SetActive(false);
            }

            var serverInfo = ((OnlineSongDetail)song).ServerInfo;
            _displayUserInfoInternal(serverInfo).Forget();
        }

        async UniTaskVoid _displayUserInfoInternal(ApiEndpoint apiEndpoint, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    Loading_icon.SetActive(true);
                    var userInfoTask = Online.CheckLoginAsync(apiEndpoint,token);
                    while (!userInfoTask.IsCompleted)
                    {
                        await UniTask.Yield();
                    }
                    if (userInfoTask.Result is null) return;
                    var userInfo = userInfoTask.Result.Value;

                    await UniTask.SwitchToMainThread();
                    username_text.text = userInfo.Username;

                    var url = apiEndpoint.Url + "/account/Icon?username=" + userInfo.Username;
                    print(url);
                    UnityWebRequest m_webrequest = UnityWebRequestTexture.GetTexture(url);
                    var req = m_webrequest.SendWebRequest();

                    while (!req.isDone)
                    {
                        await UniTask.Yield();
                    }
                    print(m_webrequest.result);
                    // 检查下载是否成功
                    if (m_webrequest.result != UnityWebRequest.Result.Success)
                    {
                        // 打印错误信息
                        Debug.LogError("Failed to download image");
                    }
                    else
                    {
                        // 从下载处理器获取纹理
                        Texture2D tex = ((DownloadHandlerTexture)m_webrequest.downloadHandler).texture;
                        Sprite createSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        usericon.sprite = createSprite;
                    }


                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to load user info: " + ex.Message);
                    Error_icon.SetActive(true);
                }
                finally
                {
                    Loading_icon.SetActive(false);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
