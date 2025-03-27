using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FPSDisplayerIcon : MonoBehaviour
{
    public List<Sprite> Sprites;

    public void ChangeSpriteRender(int index)
    {
        if (Sprites == null)
        {
            Debug.LogError("未设置精灵列表");

            return;
        }
        if (GetComponent<SpriteRenderer>() is SpriteRenderer spriteRenderer)
        {
            spriteRenderer.sprite = Sprites.Skip(index).FirstOrDefault();
            return;
        }
        else
        {
            Debug.LogError("找不到精灵渲染器");
            return;
        }
    }

    void OnEnable()
    {
        StartCoroutine(CheckRecordState());
    }

    IEnumerator CheckRecordState()
    {
        while (gameObject.activeInHierarchy)
        {
            if (MajInstances.RecordHelper?.Recording ?? false)
            {
                ChangeSpriteRender(0);

            }
            else if (MajInstances.RecordHelper?.Connected??false)
            {
                ChangeSpriteRender(1);

            }
            else
            {
                ChangeSpriteRender(2);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    // Start is called before the first frame update
    
}
