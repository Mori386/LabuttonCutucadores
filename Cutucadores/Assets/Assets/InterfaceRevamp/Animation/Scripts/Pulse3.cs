using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pulse3 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform Asset;
    public float strength = 1.1f;
    public float duration = 0.6f;

    public void Start()
    {
        TweenManager.Instance.PlayStretchY(Asset, strength, duration);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        TweenManager.Instance.PlayStretchY(Asset, strength, duration);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TweenManager.Instance.StopTween(Asset);
    }
}
