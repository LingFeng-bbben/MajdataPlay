using MajdataPlay.Net;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay
{
    public class UserInfoDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("username_text")]
        TextMeshProUGUI _usernameDisplayer;

        [SerializeField]
        [FormerlySerializedAs("usericon")]
        Image _userAvatarDisplayer;

        [SerializeField]
        [FormerlySerializedAs("defaultAvatar")]
        Sprite _defaultAvatar;

        [SerializeField]
        [FormerlySerializedAs("Loading_icon")]
        GameObject _loadingIcon;

        [SerializeField]
        [FormerlySerializedAs("Error_icon")]
        GameObject _errorIcon;

        ApiEndpoint? _currentApiEndpoint;

        void Update()
        {
            if(_currentApiEndpoint is null)
            {
                return;
            }
            Refresh();
        }

        public void DisplayUserInfo(ApiEndpoint apiEndpoint)
        {
            _currentApiEndpoint = apiEndpoint;
        }

        public void DisplayFromSong(ISongDetail song)
        {
            if (song is not OnlineSongDetail)
            {
                gameObject.SetActive(false);
                return;
            }
            var serverInfo = ((OnlineSongDetail)song).ServerInfo;
            DisplayUserInfo(serverInfo);     
        }

        void Refresh()
        {
            var runtimeConfig = _currentApiEndpoint!.RuntimeConfig;
            if (runtimeConfig.AuthMethod == NetAuthMethodOption.None || !runtimeConfig.IsLoggedIn)
            {
                _usernameDisplayer.text = "Guest";
                _userAvatarDisplayer.sprite = _defaultAvatar;
                _currentApiEndpoint = null;
            }
            else
            {
                _userAvatarDisplayer.sprite = runtimeConfig.Avatar ?? _defaultAvatar;
                if (runtimeConfig.Avatar is null)
                {
                    _errorIcon.SetActive(true);
                }
                _usernameDisplayer.text = runtimeConfig.Username;
            }
        }
    }
}
