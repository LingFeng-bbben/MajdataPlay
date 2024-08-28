using MajdataPlay.Types;
using System;
using UnityEngine;
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
    Sprite[] judgeText;

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

    /// <summary>
    ///     加载判定文本的皮肤
    /// </summary>
    private void LoadSkin()
    {

        var customSkin = SkinManager.Instance;
        judgeText = customSkin.JudgeText;

        foreach (var judgeEffect in judgeEffects)
        {
            judgeEffect.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite =
                customSkin.JudgeText[0];
            judgeEffect.transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite =
                customSkin.JudgeText_Break;
        }
    }

    /// <summary>
    /// Tap, Hold, Star
    /// </summary>
    /// <param name="position"></param>
    /// <param name="isBreak"></param>
    /// <param name="judge"></param>
    public void PlayEffect(int position, bool isBreak,JudgeType judge = JudgeType.Perfect)
    {
        var pos = position - 1;

        switch (judge)
        {
            case JudgeType.LateGood:
            case JudgeType.FastGood:
                LightManager.Instance.SetButtonLight(Color.green, pos);
                judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[1];
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
                judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[2];
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
                judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[3];
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
                judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[4];
                ResetEffect(position);
                tapEffects[pos].SetActive(true);
                if (isBreak)
                {
                    tapAnimators[pos].speed = 0.9f;
                    tapAnimators[pos].SetTrigger("break");
                }               
                break;
            default:
                judgeEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[0];
                break;
        }

        if (isBreak && judge == JudgeType.Perfect)
            judgeAnimators[pos].SetTrigger("break");
        else
            judgeAnimators[pos].SetTrigger("perfect");
    }
    public void PlayTouchEffect(Transform touchTransform,SensorType sensorPos,JudgeType judgeResult = JudgeType.Perfect)
    {
        var pos = touchTransform.position;

        var obj = Instantiate(touchJudgeEffect, Vector3.zero, touchTransform.rotation); // Judge Text
        var _obj = Instantiate(touchJudgeEffect, Vector3.zero, touchTransform.rotation); // Fast/Late Text
        var effectObj = Instantiate(touchEffect, pos, touchTransform.rotation); // Hit Effect

        var judgeObj = obj.transform.GetChild(0);
        var flObj = _obj.transform.GetChild(0);

        if (sensorPos != SensorType.C)
        {
            judgeObj.transform.position = GetPosition(touchTransform.position ,-0.46f);
            flObj.transform.position = GetPosition(touchTransform.position ,-0.92f);
        }
        else
        {
            judgeObj.transform.position = new Vector3(0, -0.6f, 0);
            flObj.transform.position = new Vector3(0, -1.08f, 0);
        }
        judgeObj.GetChild(0).transform.rotation = GetRoation(pos,sensorPos);
        flObj.GetChild(0).transform.rotation = GetRoation(pos,sensorPos);

        var anim = obj.GetComponent<Animator>();
        var flAnim = _obj.GetComponent<Animator>();
        var effectAnim = effectObj.GetComponent<Animator>();
        switch (judgeResult)
        {
            case JudgeType.LateGood:
            case JudgeType.FastGood:
                judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[1];
                effectAnim.SetTrigger("good");
                break;
            case JudgeType.LateGreat:
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[2];
                effectAnim.SetTrigger("great");
                break;
            case JudgeType.LatePerfect2:
            case JudgeType.FastPerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.FastPerfect1:
                judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[3];
                effectAnim.SetTrigger("perfect");
                break;
            case JudgeType.Perfect:
                judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[4];
                effectAnim.SetTrigger("perfect");
                break;
            case JudgeType.Miss:
                judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText[0];
                Destroy(effectObj);
                break;
            default:
                break;
        }
            

        PlayFastLate(_obj, flAnim, judgeResult);

        anim.SetTrigger("touch");
    }
    
    /// <summary>
    /// Tap，Hold，Star
    /// </summary>
    /// <param name="position"></param>
    /// <param name="judge"></param>
    public void PlayFastLate(int position,JudgeType judge)
    {

        var customSkin = SkinManager.Instance;
        var pos = position - 1;
        if ((int)judge is (0 or 7))
        {
            fastLateEffects[pos].SetActive(false);
            return;
        }
        fastLateEffects[pos].SetActive(true);
        bool isFast = (int)judge > 7;
        if(isFast)
             fastLateEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = customSkin.FastText;
        else
            fastLateEffects[pos].transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = customSkin.LateText;
        fastLateAnims[pos].SetTrigger("perfect");

    }
    /// <summary>
    /// Touch，TouchHold
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="anim"></param>
    /// <param name="judge"></param>
    public void PlayFastLate(GameObject obj,Animator anim, JudgeType judge)
    {
        var customSkin = SkinManager.Instance;
        if ((int)judge is (0 or 7))
        {
            obj.SetActive(false);
            Destroy(obj);
            return;
        }
        obj.SetActive(true);
        bool isFast = (int)judge > 7;
        if (isFast)
            obj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = customSkin.FastText;
        else
            obj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = customSkin.LateText;
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