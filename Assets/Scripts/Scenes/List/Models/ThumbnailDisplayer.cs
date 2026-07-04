using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.List.Models
{
    public class ThumbnailDisplayer : CoverSmallDisplayer
    {
        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }
        public void SetSongDetail(ISongDetail detail)
        {

        }
    }
}
