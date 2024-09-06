using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using System;
using UnityEngine;
using UnityEngine.UIElements;
#nullable enable
public class NoteEffectManager : MonoBehaviour
{
    public GameObject touchEffect;
    public GameObject touchJudgeEffect;
    public GameObject perfectEffect; // TouchHold

    private readonly Animator[] judgeAnimators = new Animator[8];
    private readonly GameObject[] judgeEffects = new GameObject[8];
    private readonly Animator[] tapAnimators = new Animator[8];
    private readonly Animator[] greatAnimators = new Animator[8];
    private readonly Animator[] goodAnimators = new Animator[8];

    private readonly GameObject[] tapEffects = new GameObject[8];
    private readonly GameObject[] greatEffects = new GameObject[8];
    private readonly GameObject[] goodEffects = new GameObject[8];

    private readonly Animator[] fastLateAnims = new Animator[8];
    private readonly GameObject[] fastLateEffects = new GameObject[8];
    JudgeTextSkin judgeSkin;

    // Start is called before the first frame update
    private void Awake()
    {
        var tapEffectParent = transform.GetChild(0).gameObject;
        var judgeEffectParent = transform.GetChild(1).gameObject;
        var greatEffectParent = transform.GetChild(2).gameObject;
        var goodEffectParent = transform.GetChild(3).gameObject;
        var flParent = transform.GetChild(4).gameObject;

        for (var i = 0; i < 8; i++)
        {
            judgeEffects[i] = judgeEffectParent.transform.GetChild(i).gameObject;
            judgeAnimators[i] = judgeEffects[i].GetComponent<Animator>();

            fastLateEffects[i] = flParent.transform.GetChild(i).gameObject;
            fastLateAnims[i] = fastLateEffects[i].GetComponent<Animator>();

            goodEffects[i] = goodEffectParent.transform.GetChild(i).gameObject;
            greatAnimators[i] = goodEffects[i].GetComponent<Animator>();
            goodEffects[i].SetActive(false);

            greatEffects[i] = greatEffectParent.transform.GetChild(i).gameObject;
            greatAnimators[i] = greatEffects[i].GetComponent<Animator>();
            greatEffects[i].SetActive(false);

            tapEffects[i] = tapEffectParent.transform.GetChild(i).gameObject;
            tapAnimators[i] = tapEffects[i].GetComponent<Animator>();
            tapEffects[i].SetActive(false);

        }

        LoadSkin();
    }
    void Start()
    {
        //var originPosition = NoteDrop.GetPositionFromDistance(4.8f, 1);
        var pos = 4.3f * GameManager.Instance.Setting.Display.OuterJudgeDistance;
        var flPos = pos - 0.66f;

        var judgeEffectParent = transform.GetChild(1);
        var flParent = transform.GetChild(4);

        for(int i = 0; i < 8; i++)
        {
            judgeEffectParent.GetChild(i).GetChild(0).transform.localPosition = new Vector3(0, pos, 0);
            flParent.GetChild(i).GetChild(0).transform.localPosition = new Vector3(0, flPos, 0);
        }
    }

    /// <summary>
    ///     加载判定文本的皮肤
    /// </summary>
    private void LoadSkin()
    {

        var customSkin = SkinManager.Instance;
        judgeSkin = customSkin.GetJudgeTextSkin();

        foreach (var judgeEffect in judgeEffects)
        {
            judgeEffect.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite =
                customSkin.SelectedSkin.JudgeText[0];
            judgeEffect.transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite =
                customSkin.SelectedSkin.CriticalPerfect_Break;
        }
    }

    /// <summary>
    /// Tap, Hold, Star
    /// </summary>
    /// <param name="position"></param>
    /// <param name="isBreak"></param>
    /// <param name="judge"></param>
    public void PlayEffect(int position,in JudgeResult judgeResult)
    {
        var pos = position - 1;
        var textRenderer = judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        var breakTextRenderer = judgeEffects[pos].transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>();

        var isBreak = judgeResult.IsBreak;
        var result = judgeResult.Result;
        var canPlay = CheckEffectSetting(GameManager.Instance.Setting.Display.NoteJudgeType, judgeResult);

        switch (result)
        {
            case JudgeType.LateGood:
            case JudgeType.FastGood:
                LightManager.Instance.SetButtonLight(Color.green, pos);
                textRenderer.sprite = judgeSkin.Good;
                ResetEffect(position);
                if(isBreak)
                {
                    tapEffects[pos].SetActive(true);
                    tapAnimators[pos].speed = 0.9f;
                    tapAnimators[pos].SetTrigger("bGood");
                }
                else
                    goodEffects[pos].SetActive(true);
                break;
            case JudgeType.LateGreat:
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                LightManager.Instance.SetButtonLight(new Color(1,0.54f,1f), pos);
                textRenderer.sprite = judgeSkin.Great;
                ResetEffect(position);
                if (isBreak)
                {
                    tapEffects[pos].SetActive(true);
                    tapAnimators[pos].speed = 0.9f;
                    tapAnimators[pos].SetTrigger("bGreat");
                }
                else
                {
                    greatEffects[pos].SetActive(true);
                    greatEffects[pos].gameObject.GetComponent<Animator>().SetTrigger("great");
                }
                break;
            case JudgeType.LatePerfect2:
            case JudgeType.FastPerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.FastPerfect1:
                LightManager.Instance.SetButtonLight(new Color(0.99f, 0.99f, 0.717f), pos);
                textRenderer.sprite = judgeSkin.Perfect;
                ResetEffect(position);
                tapEffects[pos].SetActive(true);
                if (isBreak)
                {
                    tapAnimators[pos].speed = 0.9f;
                    tapAnimators[pos].SetTrigger("break");
                }
                break;
            case JudgeType.Perfect:
                LightManager.Instance.SetButtonLight(new Color(0.99f, 0.99f, 0.717f), pos);
                if(GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
                {
                    textRenderer.sprite = judgeSkin.CriticalPerfect;
                    breakTextRenderer.sprite = judgeSkin.CP_Break;
                }
                else
                {
                    textRenderer.sprite = judgeSkin.Perfect;
                    breakTextRenderer.sprite = judgeSkin.P_Break;
                }
                ResetEffect(position);
                tapEffects[pos].SetActive(true);
                if (isBreak)
                {
                    tapAnimators[pos].speed = 0.9f;
                    tapAnimators[pos].SetTrigger("break");
                }               
                break;
            default:
                textRenderer.sprite = judgeSkin.Miss;
                break;
        }

        if (!canPlay)
            return;
        if (isBreak && result == JudgeType.Perfect)
            judgeAnimators[pos].SetTrigger("break");
        else
            judgeAnimators[pos].SetTrigger("perfect");
    }
    public void PlayTouchEffect(Transform touchTransform,SensorType sensorPos,in JudgeResult judgeResult)
    {
        var pos = touchTransform.position;
        var result = judgeResult.Result;

        var obj = Instantiate(touchJudgeEffect, Vector3.zero, touchTransform.rotation); // Judge Text
        var _obj = Instantiate(touchJudgeEffect, Vector3.zero, touchTransform.rotation); // Fast/Late Text
        var effectObj = Instantiate(touchEffect, pos, touchTransform.rotation); // Hit Effect

        var judgeObj = obj.transform.GetChild(0);
        var flObj = _obj.transform.GetChild(0);

        if (sensorPos != SensorType.C)
        {
            var distance = -0.46f * (2 - GameManager.Instance.Setting.Display.InnerJudgeDistance);

            judgeObj.transform.position = GetPosition(touchTransform.position , distance);
            flObj.transform.position = GetPosition(touchTransform.position , distance - 0.48f);
        }
        else
        {
            var distance = -0.6f;
            judgeObj.transform.position = new Vector3(0, distance, 0);
            flObj.transform.position = new Vector3(0, distance - 0.48f, 0);
        }
        judgeObj.GetChild(0).transform.rotation = GetRoation(pos,sensorPos);
        flObj.GetChild(0).transform.rotation = GetRoation(pos,sensorPos);

        var anim = obj.GetComponent<Animator>();
        var flAnim = _obj.GetComponent<Animator>();
        var effectAnim = effectObj.GetComponent<Animator>();
        var textRenderer = judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        switch (result)
        {
            case JudgeType.LateGood:
            case JudgeType.FastGood:
                textRenderer.sprite = judgeSkin.Good;
                effectAnim.SetTrigger("good");
                break;
            case JudgeType.LateGreat:
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                textRenderer.sprite = judgeSkin.Great;
                effectAnim.SetTrigger("great");
                break;
            case JudgeType.LatePerfect2:
            case JudgeType.FastPerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.FastPerfect1:
                textRenderer.sprite = judgeSkin.Perfect;
                effectAnim.SetTrigger("perfect");
                break;
            case JudgeType.Perfect:
                if (GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
                    textRenderer.sprite = judgeSkin.CriticalPerfect;
                else
                    textRenderer.sprite = judgeSkin.Perfect;
                effectAnim.SetTrigger("perfect");
                break;
            case JudgeType.Miss:
                textRenderer.sprite = judgeSkin.Miss;
                Destroy(effectObj);
                break;
            default:
                break;
        }
        var canPlay = CheckEffectSetting(GameManager.Instance.Setting.Display.TouchJudgeType, judgeResult);

        PlayFastLate(_obj, flAnim, judgeResult);

        if (canPlay)
            anim.SetTrigger("touch");
        else
            Destroy(obj);
    }
    
    /// <summary>
    /// Tap，Hold，Star
    /// </summary>
    /// <param name="position"></param>
    /// <param name="judge"></param>
    public void PlayFastLate(int position, in JudgeResult judgeResult)
    {
        var pos = position - 1;
        var canPlay = CheckFastLateSetting(judgeResult);

        if (!canPlay)
        {
            fastLateEffects[pos].SetActive(false);
            return;
        }
        
        var textRenderer = fastLateEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();

        fastLateEffects[pos].SetActive(true);
        if (judgeResult.IsFast)
            textRenderer.sprite = judgeSkin.Fast;
        else
            textRenderer.sprite = judgeSkin.Late;
        fastLateAnims[pos].SetTrigger("perfect");

    }
    bool CheckEffectSetting(JudgeDisplayType effectSetting, in JudgeResult judgeResult)
    {
        var result = judgeResult.Result;
        var resultValue = (int)result;
        var absValue = Math.Abs(7 - resultValue);

        return effectSetting switch
        {
            JudgeDisplayType.All => true,
            JudgeDisplayType.BelowCP => resultValue != 7,
            JudgeDisplayType.BelowP => absValue > 2,
            JudgeDisplayType.BelowGR => absValue > 5,
            JudgeDisplayType.All_BreakOnly => judgeResult.IsBreak,
            JudgeDisplayType.BelowCP_BreakOnly => absValue != 0 && judgeResult.IsBreak,
            JudgeDisplayType.BelowP_BreakOnly => absValue > 2 && judgeResult.IsBreak,
            JudgeDisplayType.BelowGR_BreakOnly => absValue > 5 && judgeResult.IsBreak,
            _ => false
        };
    }
    bool CheckFastLateSetting(in JudgeResult judgeResult)
    {
        var flSetting = GameManager.Instance.Setting.Display.FastLateType;
        var result = judgeResult.Result;
        var resultValue = (int)result;
        var absValue = Math.Abs(7 - resultValue);


        if (resultValue is 0 || 
            flSetting is JudgeDisplayType.Disable ||
            judgeResult.Diff == 0)
            return false;

        return flSetting switch
        {
            JudgeDisplayType.All => true,
            JudgeDisplayType.BelowCP => resultValue != 7,
            JudgeDisplayType.BelowP => absValue > 2,
            JudgeDisplayType.BelowGR => absValue > 5,
            JudgeDisplayType.All_BreakOnly => judgeResult.IsBreak,
            JudgeDisplayType.BelowCP_BreakOnly => absValue != 0 && judgeResult.IsBreak,
            JudgeDisplayType.BelowP_BreakOnly => absValue > 2 && judgeResult.IsBreak,
            JudgeDisplayType.BelowGR_BreakOnly => absValue > 5 && judgeResult.IsBreak,
            _ => false
        };
    }
    /// <summary>
    /// Touch，TouchHold
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="anim"></param>
    /// <param name="judge"></param>
    public void PlayFastLate(GameObject obj,Animator anim, in JudgeResult judgeResult)
    {
        var customSkin = SkinManager.Instance;
        var canPlay = CheckFastLateSetting(judgeResult);
        if (!canPlay)
        {
            obj.SetActive(false);
            Destroy(obj);
            return;
        }

        obj.SetActive(true);
        var textRenderer = obj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();

        if (judgeResult.IsFast)
            textRenderer.sprite = customSkin.SelectedSkin.FastText;
        else
            textRenderer.sprite = customSkin.SelectedSkin.LateText;
        anim.SetTrigger("touch");

    }
    public void ResetEffect(int position)
    {
        tapEffects[position - 1].SetActive(false);
        greatEffects[position - 1].SetActive(false);
        goodEffects[position - 1].SetActive(false);
    }
    Vector3 GetPosition(Vector3 position,float distance)
    {
        var d = position.magnitude;
        var ratio = MathF.Max(0, d + distance) / d;
        return position * ratio;
    }
    Quaternion GetRoation(Vector3 position,SensorType sensorPos)
    {
        if (sensorPos == SensorType.C)
            return Quaternion.Euler(Vector3.zero);
        var d = Vector3.zero - position;
        var deg = 180 + Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

        return Quaternion.Euler(new Vector3(0, 0, -deg));
    }
}