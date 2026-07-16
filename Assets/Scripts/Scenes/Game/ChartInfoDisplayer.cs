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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Game
{
    public class ChartInfoDisplayer: MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI artist;
        public TextMeshProUGUI designer;
        public TextMeshProUGUI level;

        public Image coverImg;

        [SerializeField]
        [FormerlySerializedAs("loadingObj")]
        GameObject _loadingObj;

        public void SetMetadata(ISongDetail songDetail, ChartLevel currentLevel)
        {
            title.text = songDetail.Title;
            artist.text = songDetail.Artist;
            designer.text = songDetail.Designers[(int)currentLevel];
            level.text = songDetail.Levels[(int)currentLevel];
        }
        public void SetCover(Sprite cover)
        {
            coverImg.sprite = cover;
            _loadingObj.SetActive(false);
        }
    }
}
