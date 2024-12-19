using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Game
{
    public class ChartInfoDisplayer: MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI artist;
        public TextMeshProUGUI designer;
        public TextMeshProUGUI level;

        public Image coverImg;

        private void Start()
        {
            var song = SongStorage.WorkingCollection.Current;
            var gameManager = MajInstances.GameManager;
            title.text = song.Title;
            artist.text = song.Artist;
            designer.text = song.Designers[(int)gameManager.SelectedDiff];
            level.text = gameManager.SelectedDiff.ToString() + " " + song.Levels[(int)gameManager.SelectedDiff];
            LoadCover(song).Forget();
        }

        async UniTask LoadCover(SongDetail song)
        {
            var task = song.GetSpriteAsync();
            while (!task.IsCompleted)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            coverImg.sprite = task.Result;
        }
    }
}
