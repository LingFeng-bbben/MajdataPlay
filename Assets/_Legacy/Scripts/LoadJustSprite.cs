using MajdataPlay.Types;
using UnityEngine;

public class LoadJustSprite : MonoBehaviour
{
    public int _0curv1str2wifi;

    public int indexOffset;
    public int judgeOffset = 0;

    // Start is called before the first frame update
    private void Start()
    {
        //gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("SkinManager").GetComponent<CustomSkin>().Just[_0curv1str2wifi + 3];
        //setR();
    }

    // Update is called once per frame
    private void Update()
    {

    }
    public void SetResult(JudgeType result)
    {
        var displayCP = GameManager.Instance.Setting.Display.DisplayCriticalPerfect;
        switch (result)
        {
            case JudgeType.LatePerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.Perfect:
            case JudgeType.FastPerfect1:
            case JudgeType.FastPerfect2:
                if (displayCP)
                    SetJustCP();
                else
                    SetJustP();
                break;
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                SetFastGr();
                break;
            case JudgeType.FastGood:
                SetFastGd();
                break;
            case JudgeType.LateGood:
                SetLateGd();
                break;
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.LateGreat:
                SetLateGr();
                break;
            default:
                SetMiss();
                break;
        }
    }
    public int SetR()
    {
        indexOffset = 0;
        refreshSprite();
        return _0curv1str2wifi;
    }
    public int SetL()
    {
        indexOffset = 3;
        refreshSprite();
        return _0curv1str2wifi;
    }
    public void SetJustCP()
    {
        judgeOffset = 0;
        refreshSprite();
    }
    public void SetJustP()
    {
        judgeOffset = 6;
        refreshSprite();
    }
    public void SetFastP()
    {
        judgeOffset = 12;
        refreshSprite();
    }
    public void SetFastGr()
    {
        judgeOffset = 18;
        refreshSprite();
    }
    public void SetFastGd()
    {
        judgeOffset = 24;
        refreshSprite();
    }
    public void SetLateP()
    {
        judgeOffset = 30;
        refreshSprite();
    }
    public void SetLateGr()
    {
        judgeOffset = 36;
        refreshSprite();
    }
    public void SetLateGd()
    {
        judgeOffset = 42;
        refreshSprite();
    }
    public void SetMiss()
    {
        judgeOffset = 48;
        refreshSprite();
    }
    private void refreshSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = SkinManager.Instance.Just[_0curv1str2wifi + indexOffset + judgeOffset];
    }
}