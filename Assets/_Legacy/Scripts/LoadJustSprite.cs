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
        switch (result)
        {
            case JudgeType.LatePerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.Perfect:
            case JudgeType.FastPerfect1:
            case JudgeType.FastPerfect2:
                break;
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                setFastGr();
                break;
            case JudgeType.FastGood:
                setFastGd();
                break;
            case JudgeType.LateGood:
                setLateGd();
                break;
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.LateGreat:
                setLateGr();
                break;
            default:
                setMiss();
                break;
        }
    }
    public int setR()
    {
        indexOffset = 0;
        refreshSprite();
        return _0curv1str2wifi;
    }

    public int setL()
    {
        indexOffset = 3;
        refreshSprite();
        return _0curv1str2wifi;
    }
    public void setFastGr()
    {
        judgeOffset = 6;
        refreshSprite();
    }
    public void setFastGd()
    {
        judgeOffset = 12;
        refreshSprite();
    }
    public void setLateGr()
    {
        judgeOffset = 18;
        refreshSprite();
    }
    public void setLateGd()
    {
        judgeOffset = 24;
        refreshSprite();
    }
    public void setMiss()
    {
        judgeOffset = 30;
        refreshSprite();
    }
    private void refreshSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("SkinManager").GetComponent<SkinManager>()
            .Just[_0curv1str2wifi + indexOffset + judgeOffset];
    }
}