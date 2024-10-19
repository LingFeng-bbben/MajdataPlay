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
        // Start is called before the first frame update
        void Start()
        {
            CoverListDisplayer.SetDirList(SongStorage.Collections);
            CoverListDisplayer.SetSongList();
            //LightManager.Instance.SetAllLight(Color.white);
            LightManager.Instance.SetButtonLight(Color.green, 3);
            LightManager.Instance.SetButtonLight(Color.red, 4);
            LightManager.Instance.SetButtonLight(Color.blue, 2);
            LightManager.Instance.SetButtonLight(Color.blue, 5);
            LightManager.Instance.SetButtonLight(Color.yellow, 6);
            LightManager.Instance.SetButtonLight(Color.yellow, 1);
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
                return;
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
                        CoverListDisplayer.SlideList(1);
                        break;
                    case SensorType.A6:
                        CoverListDisplayer.SlideList(-1);
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
                            LightManager.Instance.SetButtonLight(Color.white, 4);
                            SongStorage.WorkingCollection.Index = 0;
                        }
                        break;
                    case SensorType.A4:
                        if (CoverListDisplayer.IsDirList)
                        {
                            CoverListDisplayer.SetSongList();
                            LightManager.Instance.SetButtonLight(Color.red, 4);
                        }
                        else
                        {
                            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                            MajInstances.AudioManager.StopSFX(SFXSampleType.SELECT_SONG);
                            MajInstances.AudioManager.StopSFX(SFXSampleType.SELECT_BGM);
                            MajInstances.SceneSwitcher.SwitchScene("Game");
                        }
                        break;
                    case SensorType.A7:
                        MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                        MajInstances.SceneSwitcher.SwitchScene("Setting");
                        break;
                }
            }
        }
    }
}