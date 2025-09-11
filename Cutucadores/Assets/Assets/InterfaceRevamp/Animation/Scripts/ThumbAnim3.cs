using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbAnim3 : MonoBehaviour
{
    [SerializeField] private RectTransform Asset;
    public float strength = 1.1f;
    public float duration = 0.6f;

    private void OnEnable()
    {
        TweenManager.Instance.PlayStretchY(Asset, strength, duration);

    }

    private void OnDisable()
    {
        TweenManager.Instance.StopTween(Asset);
    }   
 
}
