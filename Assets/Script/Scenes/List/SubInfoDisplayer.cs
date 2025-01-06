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
        public GameObject commentPrefab;
        List<GameObject> comments = new List<GameObject>();
        // Start is called before the first frame update
        public void RefreshContent(SongDetail song)
        {
            if (song.IsOnline)
            {
                id_text.text = "ID: " + song.OnlineId;
                StopAllCoroutines();
                foreach (var obj in comments)
                {
                    Destroy(obj);
                }
                comments.Clear();
                StartCoroutine(GetOnlineInteraction(song));
            }
            else
            {
                id_text.text = "";
                good_text.text = "";
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

            foreach (var comment in list.Comments)
            {
                var text = comment.Sender.Username + "\n" + comment.Content + "\n";
                var obj = Instantiate(commentPrefab, gameObject.transform);
                comments.Add(obj);
                obj.GetComponent<Text>().text = text;
                yield return new WaitForSeconds(2f);
            }

        }
    }
}