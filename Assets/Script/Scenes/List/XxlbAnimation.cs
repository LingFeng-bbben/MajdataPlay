using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.List
{
    public class XxlbAnimation : MonoBehaviour
    {
        public static XxlbAnimation instance;
        Animator animator;
        private void Awake()
        {
            instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        public void PlayTouchAnimation() => animator.SetTrigger("Touch");
        public void PlayGoodAnimation() => animator.SetTrigger("Good");
        public void PlayBadAnimation() => animator.SetTrigger("Bad");
    }
}