using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BridgeStepScript : MonoBehaviour
{
    public float delta = 3.5f;  // Amount to move left and right from the start point
    public float speed = 2.0f;
    private Vector3 startPos;
    private bool stop;

    public bool Stop { get => stop; set => stop = value; }

    void Start()
    {
        startPos = transform.position;
        Stop = true;
    }

    void Update()   
    {
        if (!Stop)
        {
            Vector3 v = startPos;
            v.x += delta * Mathf.Sin(Time.time * speed);
            transform.position = v;
        }
    }
}
