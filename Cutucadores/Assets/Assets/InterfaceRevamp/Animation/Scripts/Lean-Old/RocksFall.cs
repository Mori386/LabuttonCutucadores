using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocksFall : MonoBehaviour
{
    public RectTransform[] rocks;   
    public float fallDistance = -800f; 
    public float minDuration = 0.5f;
    public float maxDuration = 1.5f;
    public float minDelay = 0f;
    public float maxDelay = 1f;
    public float shakeAmount = 20f; 

    void Start()
    {
        foreach (RectTransform rock in rocks)
        {
            StartFalling(rock);
        }
    }

    void StartFalling(RectTransform rock)
    {
        Vector3 startPos = rock.anchoredPosition;

        float delay = Random.Range(minDelay, maxDelay);
        float duration = Random.Range(minDuration, maxDuration);
        LeanTween.moveY(rock, startPos.y + fallDistance, duration)
            .setEaseInBack()
            .setDelay(delay)
            .setOnStart(() =>
            {

                LeanTween.moveX(rock, startPos.x + Random.Range(-shakeAmount, shakeAmount), duration * 0.3f)
                    .setEaseShake()
                    .setLoopClamp();
            })
            .setOnComplete(() =>
            {
                rock.anchoredPosition = startPos;
                StartFalling(rock);
            });
    }
}
