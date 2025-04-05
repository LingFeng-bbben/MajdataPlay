using MajdataPlay.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MajdataPlay
{
    public class RecordHelperStatusDisplayer : MonoBehaviour
    {
        public List<Sprite> Sprites;
        float _frameTimer = 1;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ChangeSpriteRender(int index)
        {
            if (Sprites == null)
            {
                Debug.LogError("No Sprites");
                return;
            }

            if (GetComponent<SpriteRenderer>() is { } spriteRenderer)
            {
                spriteRenderer.sprite = Sprites.Skip(index).FirstOrDefault();
                return;
            }
            else
            {
                Debug.LogError("No Sprite Render");
                return;
            }
        }

        void LateUpdate()
        {
            var delta = Time.deltaTime;
            if (_frameTimer <= 0 && gameObject.activeInHierarchy)
            {
                if (MajInstances.RecordHelper?.Recording ?? false)
                {
                    ChangeSpriteRender(0);
                }
                else if (MajInstances.RecordHelper?.Connected ?? false)
                {
                    ChangeSpriteRender(1);
                }
                else
                {
                    ChangeSpriteRender(2);
                }

                _frameTimer = 1;
            }
            else
                _frameTimer -= delta;
        }
    }
}
