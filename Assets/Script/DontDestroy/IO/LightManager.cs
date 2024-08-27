using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static LightManager Instance;
    public bool useDummy = true;
    public SpriteRenderer[] DummyLights;
    private void Awake()
    {
        Instance = this; 
        DontDestroyOnLoad(gameObject);
        DummyLights = gameObject.GetComponentsInChildren<SpriteRenderer>();
    }
    void Start()
    {
        StartCoroutine(DebugLights());
    }

    public void SetAllLight(Color lightColor)
    {
        if(useDummy)
        {
            foreach(var light in DummyLights)
            {
                light.color = lightColor;
            }
        }
    }

    public void SetButtonLight(Color lightColor,int button)
    {
        if (useDummy)
        {
            DummyLights[button-1].color = lightColor;
        }
    }

    IEnumerator DebugLights()
    {
        while (true)
        {
            SetAllLight(Color.red);
            yield return new WaitForSeconds(1);
            SetAllLight(Color.green);
            yield return new WaitForSeconds(1);
            SetAllLight(Color.blue);
            yield return new WaitForSeconds(1);
            for (int i = 1; i < 9; i++)
            {
                SetButtonLight(Color.red, i);
                yield return new WaitForSeconds(0.2f);
            }
            for (int i = 1; i < 9; i++)
            {
                SetButtonLight(Color.green, i);
                yield return new WaitForSeconds(0.2f);
            }
            for (int i = 1; i < 9; i++)
            {
                SetButtonLight(Color.blue, i);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
