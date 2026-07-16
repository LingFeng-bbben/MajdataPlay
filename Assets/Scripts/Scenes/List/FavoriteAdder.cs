using MajdataPlay.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.List
{
    public class FavoriteAdder : MonoBehaviour
    {
        Image _image;
        ISongDetail _song;
        public Sprite HeartAdd;
        public Sprite HeartRemove;

        [field: SerializeField, ReadOnlyField]
        public bool State { get; private set; }
        public float PressToAddTime { get; set; } = 0f;
        public float PressToRemoveTime { get; set; } = 0f;

        float _pressTimer = 0f;
        bool _isInFav = false;

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
            _isInFav = SongStorage.IsInMyFavorites(_song);
            State = false;
            _pressTimer = 0f;
        }

        public void Hide()
        {
            _song = null;
            _image.enabled = false;
        }
        public void SetState(bool state)
        {
            State = state;
        }
        public void FavoratePressed()
        {
            if (_song is null) return;
            if (_isInFav)
            {
                SongStorage.RemoveFromMyFavorites(_song);
            }
            else
            {
                SongStorage.AddToMyFavorites(_song);
            }
            _isInFav = SongStorage.IsInMyFavorites(_song);
            _image.sprite = _isInFav ? HeartRemove : HeartAdd;
        }
        void LateUpdate()
        {
            if(State)
            {
                _pressTimer += MajTimeline.DeltaTime;
            }
            else
            {
                _pressTimer -= MajTimeline.DeltaTime;
            }
            _pressTimer = Mathf.Max(_pressTimer, 0f);

            if (_isInFav && _pressTimer > PressToRemoveTime)
            {
                FavoratePressed();
                State = false;
                _pressTimer = 0f;
            }
            else if(!_isInFav && _pressTimer > PressToAddTime)
            {
                FavoratePressed();
                State = false;
                _pressTimer = 0f;
            }
        }
    }
}