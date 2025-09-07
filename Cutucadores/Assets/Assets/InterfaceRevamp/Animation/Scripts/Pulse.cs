using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform Asset;
    public float stregth = 1.1f;
    public float time = 0.4f;
    private LTDescr tween;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tween != null) LeanTween.cancel(Asset);
        tween = LeanTween.scale(Asset, Vector3.one * stregth, time)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tween != null) LeanTween.cancel(Asset);
        LeanTween.scale(Asset, Vector3.one, 0.2f)
            .setEase(LeanTweenType.easeOutBack);
    }
}
