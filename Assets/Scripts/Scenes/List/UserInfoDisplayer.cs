using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MajdataPlay.Net;

namespace MajdataPlay
{
    public class UserInfoDisplayer : MonoBehaviour
    {
        const string GUEST_TEXT = "Guest";
        const string GUEST_LOGIN_TEXT = "Guest\nTouch to login";

        public TMP_Text username_text;
        public Image usericon;
        public GameObject Loading_icon;
        public GameObject Error_icon;

        public bool IsGuest { get; private set; } = true;

        RectTransform _rectTransform = null!;

        void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPoint, eventCamera);
        }
        
        public void DisplayUserInfo(ApiEndpoint apiEndpoint)
        {
            Loading_icon.SetActive(false);
            Error_icon.SetActive(false);

            var runtimeConfig = apiEndpoint.RuntimeConfig;
            IsGuest = runtimeConfig.AuthMethod == NetAuthMethodOption.None;
            if(IsGuest)
            {
                username_text.text = GUEST_LOGIN_TEXT;
                usericon.sprite = null;
            }
            else
            {
                usericon.sprite = runtimeConfig.Avatar;
                if (runtimeConfig.Avatar is null)
                {
                    Error_icon.SetActive(true);
                }
                username_text.text = string.IsNullOrEmpty(runtimeConfig.Username) ? GUEST_TEXT : runtimeConfig.Username;
            }
        }

        public void DisplayFromSong(ISongDetail song)
        {
            if (song is not OnlineSongDetail)
            {
                this.gameObject.SetActive(false);
                return;
            }
            var serverInfo = ((OnlineSongDetail)song).ServerInfo;
            DisplayUserInfo(serverInfo);     
        }
    }
}
