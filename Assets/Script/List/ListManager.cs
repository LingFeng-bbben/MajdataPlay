using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ListManager : MonoBehaviour
{
    public CoverListDisplayer CoverListDisplayer;
    // Start is called before the first frame update
    void Start()
    {
        CoverListDisplayer.SlideToList(GameManager.Instance.selectedIndex);
        CoverListDisplayer.SlideToDifficulty((int)GameManager.Instance.SelectedDiff);
        AudioManager.Instance.PlaySFX("SelectSong.wav");
        IOManager.Instance.BindAnyArea(OnAreaDown);

    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (!e.IsClick)
            return;
        if(!e.IsButton)
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
                    AudioManager.Instance.PlaySFX("DontTouchMe.wav");
                    XxlbAnimation.instance.PlayTouchAnimation();
                    break;
            }
        }
        else
        {
            switch(e.Type)
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
                case SensorType.A4:
                    SceneManager.LoadSceneAsync(2);
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        IOManager.Instance.UnbindAnyArea(OnAreaDown);
    }
}
