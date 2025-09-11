using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TweenManager;


public class Swing : MonoBehaviour
{
    [SerializeField] private RectTransform Asset;
    public float angle = 10f;
    public float duration = 1.2f;

    private void Start()
    {
        Instance.PlaySwing(Asset,angle,duration);

    }
    void OnDisable()
    {
        Instance.StopTween(Asset);
    }
}
