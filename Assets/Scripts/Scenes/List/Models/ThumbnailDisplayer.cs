using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.List.Models
{
    public class ThumbnailDisplayer : CoverSmallDisplayer
    {
        [SerializeField]
        [FormerlySerializedAs("coverDisplayer")]
        Image _coverDisplayer;

        public void SetActive(bool state)
        {
            _coverDisplayer.enabled = state;
        }
        public void SetSongDetail(ISongDetail detail)
        {

        }
    }
}
