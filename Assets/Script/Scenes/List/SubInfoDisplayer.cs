using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MajdataPlay.Types;
using Cysharp.Threading.Tasks;
using MajdataPlay.Net;
using System.Text.Json;

namespace MajdataPlay.List
{
    public class SubInfoDisplayer : MonoBehaviour
    {
        public TMP_Text id_text;
        public TMP_Text good_text;
        public TMP_Text CommentText;
        public GameObject CommentBox;
        public GameObject llxb;

        // Start is called before the first frame update
        public void RefreshContent(SongDetail song)
        {
            if (song.IsOnline)
            {
                id_text.text = "ID: " + song.OnlineId;
                StopAllCoroutines();
                StartCoroutine(GetOnlineInteraction(song));
                llxb.SetActive(true);
            }
            else
            {
                id_text.text = "";
                good_text.text = "";
                CommentBox.SetActive(false);
                llxb.SetActive(false);
            }
        }

        IEnumerator GetOnlineInteraction(SongDetail song)
        {
            var client = HttpTransporter.ShareClient;
            var interactUrl = song.ApiEndpoint.Url + "/maichart/" + song.OnlineId + "/interact";
            var task = client.GetStringAsync(interactUrl);
            while (!task.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }
            var intjson = task.Result;

            var list = JsonSerializer.Deserialize<MajNetSongInteract>(intjson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            good_text.text = "²¥: " + list.Plays + " ÔÞ: " + list.Likes.Length + " ÆÀ: " + list.Comments.Length;

            CommentBox.SetActive(true);
            foreach (var comment in list.Comments)
            {
                var text = comment.Sender.Username + "Ëµ£º\n" + comment.Content + "\n";
                CommentText.text = text;
                yield return new WaitForSeconds(5f);
            }
            CommentBox.SetActive(false);
        }
    }
}