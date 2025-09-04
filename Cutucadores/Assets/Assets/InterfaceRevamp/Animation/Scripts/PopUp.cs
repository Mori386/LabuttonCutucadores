using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public RectTransform Asset;

    void OnEnable()
    {
        Pop(Asset, 0.15f);
    }
    void Pop(RectTransform botao, float delay)
    {
        botao.localScale = Vector3.zero;

        LeanTween.scale(botao, Vector3.one, 0.6f)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutBack)

            .setOnComplete(() => LeanTween.scale(botao, Vector3.one * 1.1f, 0.2f)
    .setLoopPingPong(1));

    }
}
