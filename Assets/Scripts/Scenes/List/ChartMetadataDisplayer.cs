using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay.Scenes.List
{
    public class ChartMetadataDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("onlineIdText")]
        TextMeshProUGUI _onlineIdDisplayer;

        [SerializeField]
        [FormerlySerializedAs("charter")]
        TextMeshProUGUI _charter;

        [SerializeField]
        [FormerlySerializedAs("title")]
        TextMeshProUGUI _title;

        [SerializeField]
        [FormerlySerializedAs("artist")]
        TextMeshProUGUI _artist;

        [SerializeField]
        [FormerlySerializedAs("estiDisplayer")]
        TextMeshProUGUI _estiDisplayer;

        [SerializeField]
        [FormerlySerializedAs("peakDensityDisplayer")]
        TextMeshProUGUI _peakDensityDisplayer;

        [SerializeField]
        [FormerlySerializedAs("avgDensityDisplayer")]
        TextMeshProUGUI _avgDensityDisplayer;

        [SerializeField]
        [FormerlySerializedAs("bpmDisplayer")]
        TextMeshProUGUI _bpmDisplayer;

        [SerializeField]
        [FormerlySerializedAs("durationDisplayer")]
        TextMeshProUGUI _durationDisplayer;

        public void SetMetadataFromSongDetail(ISongDetail songDetail, ChartLevel level)
        {
            if (songDetail is OnlineSongDetail onlineSongDetail)
            {
                _onlineIdDisplayer.text = $"ID: {onlineSongDetail.Id}";
            }
            else
            {
                _onlineIdDisplayer.text = string.Empty;
            }
            _title.text = songDetail.Title;
            _artist.text = songDetail.Artist;
            _charter.text = songDetail.Designers[(int)level];
        }
    }
}
