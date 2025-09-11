using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbAnim2 : MonoBehaviour
{
    public RectTransform thumb;  
    public float upDistance = 100f;
    public float upDuration = 0.5f;
    public float stayDuration = 1f;
    public float downDuration = 0.5f;
    public float stayDownDuration = 1f;

    void Start()
    {
        PlayLoop();
    }

    void PlayLoop()
    {
        Vector3 startPos = thumb.anchoredPosition;
        LeanTween.moveY(thumb, startPos.y + upDistance, upDuration)
            .setEaseOutBack()
            .setOnComplete(() =>
            {

                LeanTween.delayedCall(stayDuration, () =>
                {
                    LeanTween.moveY(thumb, startPos.y - 200f, downDuration)
                        .setEaseInBack()
                        .setOnComplete(() =>
                        {
                            LeanTween.delayedCall(stayDownDuration, () =>
                            {
                                thumb.anchoredPosition = startPos; 
                                PlayLoop(); 
                            });
                        });
                });
            });
    }
}
