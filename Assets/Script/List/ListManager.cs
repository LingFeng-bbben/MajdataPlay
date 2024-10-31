using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MajdataPlay.List
{
    public class ListManager : MonoBehaviour
    {
        public CoverListDisplayer CoverListDisplayer;

        int _delta = 0;
        float _pressTime = 0;
        bool _isPressed = false;
        
        // Start is called before the first frame update
        void Start()
        {
            CoverListDisplayer.SetDirList(SongStorage.Collections);
            CoverListDisplayer.SetSongList();
            //MajInstances.LightManager.SetAllLight(Color.white);
            MajInstances.LightManager.SetButtonLight(Color.green, 3);
            MajInstances.LightManager.SetButtonLight(Color.red, 4);
            MajInstances.LightManager.SetButtonLight(Color.blue, 2);
            MajInstances.LightManager.SetButtonLight(Color.blue, 5);
            MajInstances.LightManager.SetButtonLight(Color.yellow, 6);
            MajInstances.LightManager.SetButtonLight(Color.yellow, 1);
            CoverListDisplayer.SlideToDifficulty((int)MajInstances.GameManager.SelectedDiff);
            if (!MajInstances.AudioManager.GetSFX(SFXSampleType.SELECT_BGM).IsPlaying)
            {
                MajInstances.AudioManager.PlaySFX(SFXSampleType.SELECT_BGM, true);
                MajInstances.AudioManager.PlaySFX(SFXSampleType.SELECT_SONG);
            }
            MajInstances.InputManager.BindAnyArea(OnAreaDown);
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
            {
                switch(e.Type)
                {
                    case SensorType.A6:
                    case SensorType.A3:
                        _isPressed = false;
                        _delta = 0;
                        _pressTime = 0;
                        break;
                }
                return;
            }
            if (!e.IsButton)
            {
                switch (e.Type)
                {
                    case SensorType.A1:
                        CoverListDisplayer.SlideList(1);
                        break;
                    case SensorType.D2:
                        CoverListDisplayer.SlideList(2);
                        break;
                    case SensorType.A2:
                        CoverListDisplayer.SlideList(3);
                        break;
                    case SensorType.D3:
                        CoverListDisplayer.SlideList(4);
                        break;
                    case SensorType.A3:
                        CoverListDisplayer.SlideList(5);
                        break;
                    case SensorType.D4:
                        CoverListDisplayer.SlideList(6);
                        break;
                    case SensorType.A8:
                        CoverListDisplayer.SlideList(-1);
                        break;
                    case SensorType.D8:
                        CoverListDisplayer.SlideList(-2);
                        break;
                    case SensorType.A7:
                        CoverListDisplayer.SlideList(-3);
                        break;
                    case SensorType.D7:
                        CoverListDisplayer.SlideList(-4);
                        break;
                    case SensorType.A6:
                        CoverListDisplayer.SlideList(-5);
                        break;
                    case SensorType.D6:
                        CoverListDisplayer.SlideList(-6);
                        break;
                    // xxlb
                    case SensorType.A4:
                    case SensorType.A5:
                    case SensorType.D5:
                        MajInstances.AudioManager.PlaySFX(SFXSampleType.DONT_TOUCH_ME);
                        XxlbAnimation.instance.PlayTouchAnimation();
                        break;
                }
            }
            else
            {
                switch (e.Type)
                {
                    case SensorType.A3:
                        if (_isPressed)
                            return;
                        CoverListDisplayer.SlideList(1);
                        _isPressed = true;
                        _delta = 1;
                        AutoSlide().Forget();
                        break;
                    case SensorType.A6:
                        if (_isPressed)
                            return;
                        CoverListDisplayer.SlideList(-1);
                        _isPressed = true;
                        _delta = -1;
                        AutoSlide().Forget();
                        break;
                    case SensorType.A8:
                        CoverListDisplayer.SlideDifficulty(-1);
                        break;
                    case SensorType.A1:
                        CoverListDisplayer.SlideDifficulty(1);
                        break;
                    case SensorType.A2:
                        MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                        MajInstances.SceneSwitcher.SwitchScene("SortFind");
                        break;
                    case SensorType.A5:
                        if (CoverListDisplayer.IsChartList)
                        {
                            CoverListDisplayer.SetDirList(SongStorage.Collections);
                            MajInstances.LightManager.SetButtonLight(Color.white, 4);
                            SongStorage.WorkingCollection.Index = 0;
                        }
                        break;
                    case SensorType.A4:
                        if (CoverListDisplayer.IsDirList)
                        {
                            CoverListDisplayer.SetSongList();
                            MajInstances.LightManager.SetButtonLight(Color.red, 4);
                        }
                        else
                        {
                            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.AudioManager.StopSFX(SFXSampleType.SELECT_SONG);
                            MajInstances.AudioManager.StopSFX(SFXSampleType.SELECT_BGM);
                            MajInstances.SceneSwitcher.SwitchScene("Game",false);
                        }
                        break;
                    case SensorType.A7:
                        MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                        MajInstances.SceneSwitcher.SwitchScene("Setting");
                        break;
                }
            }
        }
        async UniTaskVoid AutoSlide()
        {
            while(_isPressed)
            {
                if ( _pressTime < 0.4f)
                {
                    _pressTime += Time.deltaTime;
                    await UniTask.Yield();
                    continue;
                }
                CoverListDisplayer.SlideList(_delta);
                await UniTask.Delay(100);
            }
        }
    }
}