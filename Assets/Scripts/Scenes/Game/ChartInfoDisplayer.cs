using Cysharp.Threading.Tasks;
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

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;
        private void Start()
        {
            if (_gameInfo is null)
                return;
            var song = _gameInfo.Current;
            title.text = song.Title;
            artist.text = song.Artist;
            designer.text = song.Designers[(int)_gameInfo.CurrentLevel];
            level.text = _gameInfo.CurrentLevel.ToString() + " " + song.Levels[(int)_gameInfo.CurrentLevel];
            LoadCover(song).Forget();
        }

        async UniTask LoadCover(ISongDetail song)
        {
            coverImg.sprite = await song.GetCoverAsync(true);
        }
    }
}
