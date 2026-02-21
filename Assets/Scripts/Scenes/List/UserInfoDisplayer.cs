using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MajdataPlay.Net;
using MajdataPlay.Settings;

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
            var runtimeConfig = apiEndpoint.RuntimeConfig;
            if(runtimeConfig.AuthMethod == NetAuthMethodOption.None)
            {
                username_text.text = "Guest";
            }
            else
            {
                usericon.sprite = runtimeConfig.Avatar;
                if (runtimeConfig.Avatar is null)
                {

                    Error_icon.SetActive(true);
                }
                username_text.text = runtimeConfig.Username;
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
