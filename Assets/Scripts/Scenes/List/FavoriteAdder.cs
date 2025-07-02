using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.List
{
    public class FavoriteAdder : MonoBehaviour
    {
        Image _image;
        ISongDetail _song;
        public Sprite HeartAdd;
        public Sprite HeartRemove;
        void Start()
        {
            _image = GetComponent<Image>();
        }

        public void SetSong(ISongDetail song)
        {
            var isInFav = SongStorage.IsInMyFavorites(song);
            _song = song;
            _image.enabled = true;
            _image.sprite = isInFav ? HeartRemove : HeartAdd;
        }

        public void Hide()
        {
            _song = null;
            _image.enabled = false;
        }

        public void FavoratePressed()
        {
            if (_song is null) return;
            var isInFav = SongStorage.IsInMyFavorites(_song);
            if (isInFav)
            {
                SongStorage.RemoveFromMyFavorites(_song);
            }
            else
            {
                SongStorage.AddToMyFavorites(_song);
            }
            isInFav = SongStorage.IsInMyFavorites(_song);
            _image.sprite = isInFav ? HeartRemove : HeartAdd;
        }
    }
}