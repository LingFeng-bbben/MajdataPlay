using Assets.Scripts.Interfaces;
using System;
using UnityEngine;

public class BreakShineController : MonoBehaviour
{
    public IFlasher parent;

    SpriteRenderer spriteRenderer;
    GamePlayManager gamePlayManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if(parent is not null && parent.CanShine())
        {
            var extra = Math.Max(Mathf.Sin(gamePlayManager.GetFrame() * 0.17f) * 0.5f, 0);
            spriteRenderer.material.SetFloat("_Brightness", 0.95f + extra);
        }
    }
    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gamePlayManager = GamePlayManager.Instance;
    }
}
