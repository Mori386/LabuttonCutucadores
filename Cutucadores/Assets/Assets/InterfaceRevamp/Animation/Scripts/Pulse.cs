using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform Asset;
    private LTDescr tween;

    void Awake()
    {
        Asset = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tween != null) LeanTween.cancel(Asset);
        tween = LeanTween.scale(Asset, Vector3.one * 1.1f, 0.4f)
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
