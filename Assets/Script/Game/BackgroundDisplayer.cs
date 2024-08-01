using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class BackgroundDisplayer : MonoBehaviour
{
    SpriteRenderer sr;
    // Start is called before the first frame update
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
    }

    public void SetBackground(Sprite bg)
    {
        sr.sprite = bg;
        var scale = 1080f / bg.texture.width;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetBackgroundDim(float dim)
    {
        sr.color = new Color(dim, dim, dim);
    }

}
