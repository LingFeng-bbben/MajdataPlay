using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteEffectManager : MonoBehaviour
    {
        public GameObject touchEffect;
        public GameObject touchJudgeEffect;
        public GameObject perfectEffect; // TouchHold

        NoteEffectPool effectPool;
        GameObject fireworkEffect;
        Animator fireworkEffectAnimator;


        private readonly Animator[] fastLateAnims = new Animator[8];
        private readonly GameObject[] fastLateEffects = new GameObject[8];
        JudgeTextSkin judgeSkin;

        // Start is called before the first frame update
        private void Awake()
        {

        }
        void Start()
        {
            fireworkEffect = GameObject.Find("FireworkEffect");
            fireworkEffectAnimator = fireworkEffect.GetComponent<Animator>();

            effectPool = GetComponent<NoteEffectPool>();
        }
        public void PlayFireworkEffect(in Vector3 position)
        {
            fireworkEffectAnimator.SetTrigger("Fire");
            fireworkEffect.transform.position = position;
        }
        /// <summary>
        /// Tap, Hold, Star
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isBreak"></param>
        /// <param name="judge"></param>
        public void PlayEffect(int position, in JudgeResult judgeResult)
        {
            effectPool.Play(judgeResult, position);
        }
        public void PlayTouchEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            effectPool.Play(judgeResult, sensorPos);
        }
        public void PlayTouchHoldEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            effectPool.PlayTouchHoldEffect(judgeResult, sensorPos);
        }

        /// <summary>
        /// Tap，Hold，Star
        /// </summary>
        /// <param name="position"></param>
        /// <param name="judge"></param>
        public void PlayFastLate(int position, in JudgeResult judgeResult)
        {
            var pos = position - 1;
            var canPlay = CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.FastLateType, judgeResult);

            if (!canPlay || judgeResult.IsMiss)
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
        public static bool CheckJudgeDisplaySetting(in JudgeDisplayType setting, in JudgeResult judgeResult)
        {
            var result = judgeResult.Result;
            var resultValue = (int)result;
            var absValue = Math.Abs(7 - resultValue);

            return setting switch
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
        public void PlayFastLate(GameObject obj, Animator anim, in JudgeResult judgeResult)
        {
            var customSkin = SkinManager.Instance;
            var canPlay = CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.FastLateType, judgeResult);
            if (!canPlay || judgeResult.IsMiss)
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
            effectPool.Reset(position);
        }
        Vector3 GetPosition(Vector3 position, float distance)
        {
            var d = position.magnitude;
            var ratio = MathF.Max(0, d + distance) / d;
            return position * ratio;
        }
        Quaternion GetRoation(Vector3 position, SensorType sensorPos)
        {
            if (sensorPos == SensorType.C)
                return Quaternion.Euler(Vector3.zero);
            var d = Vector3.zero - position;
            var deg = 180 + Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

            return Quaternion.Euler(new Vector3(0, 0, -deg));
        }
    }
}