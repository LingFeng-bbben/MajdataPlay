﻿using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public bool ifDestroy;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (ifDestroy) Destroy(gameObject);
    }
}