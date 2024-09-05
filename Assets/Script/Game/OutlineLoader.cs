using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = SkinManager.Instance.SelectedSkin.Outline;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
