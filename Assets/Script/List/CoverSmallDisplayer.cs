using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoverSmallDisplayer : MonoBehaviour
{
    public Image Cover;
    public Image LevelBackground;
    public TextMeshProUGUI levelText;
    private SongDetail bindedSong;
    private bool isRefreshed = false;
    // Start is called before the first frame update
    public void SetOpacity(float alpha)
    {
        Cover.color = new Color(Cover.color.r, Cover.color.g, Cover.color.b, alpha);
        LevelBackground.color = new Color(LevelBackground.color.r, LevelBackground.color.g, LevelBackground.color.b, alpha);
        levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, alpha);
        if (alpha > 0.5f)
        {
            ShowCover();
        }
    }

    public void SetLevelText(string text)
    {
        levelText.text = text;
    }
    public void SetCover(SongDetail detail)
    {
        bindedSong = detail;
    }

    public void ShowCover()
    {
        if (bindedSong != null)
        {
            if (!isRefreshed)
            {
                StartCoroutine(SetCoverAsync(bindedSong));
                isRefreshed = true;
            }
        }
    }

    IEnumerator SetCoverAsync(SongDetail detail)
    {
        var spriteTask = detail.GetSpriteAsync();
        //TODO:set the cover to be now loading?
        while (!spriteTask.IsCompleted)
        {
            yield return new WaitForEndOfFrame();
        }
        Cover.sprite = spriteTask.Result;
    }
}
