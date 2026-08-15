using System.Collections;
using Cysharp.Threading.Tasks;
using MajdataPlay.Scenes.Game.Notes.Skins;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay
{
    public class LoadSubBg : MajBehaviour
    {
        Image _img;
        SkinManager _skinManager;
        protected override void Awake()
        {
            base.Awake();
            _img = GetComponent<Image>();
            WaitSkinLoadedAsync().Forget();
        }
        void OnDestroy()
        {
            if (_skinManager is not null)
            {
                _skinManager.OnSkinChanged -= OnSkinChanged;
            }
        }
        async UniTask WaitSkinLoadedAsync()
        {
            var cancellationToken = destroyCancellationToken;
            while (MajInstances.SkinManager?.IsInited != true)
            {
                if (await UniTask.Yield(cancellationToken).SuppressCancellationThrow())
                {
                    return;
                }
            }
            _skinManager = MajInstances.SkinManager;
            if (_skinManager is null)
            {
                return;
            }
            _img.sprite = _skinManager.SelectedSkin.SubDisplay;
            _img.color = Color.white;
            _skinManager.OnSkinChanged += OnSkinChanged;
        }
        void OnSkinChanged(SkinManager sender, CustomSkin newSkin)
        {
            _img.sprite = sender.SelectedSkin.SubDisplay;
            _img.color = Color.white;
        }
    }
}
