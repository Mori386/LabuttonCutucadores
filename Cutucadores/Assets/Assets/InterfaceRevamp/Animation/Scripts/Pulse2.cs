using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse2 : MonoBehaviour
{
    public RectTransform Asset;
    public float stregth = 1.1f;
    public float time = 0.4f;
    void Start()
    {
        LeanTween.scale(Asset, Vector3.one * stregth, time)
    .setEase(LeanTweenType.easeInOutSine)
    .setLoopPingPong();
    }

}
