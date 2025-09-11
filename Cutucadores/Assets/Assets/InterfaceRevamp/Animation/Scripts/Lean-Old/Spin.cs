using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public RectTransform asset;
    public float travelTime = 3f;       
    public float height = 200f;         
    public float rotationSpeed = 360f;  
    public float offscreenOffset = 200f;

    private Vector2 startPos;
    private Vector2 endPos;

    void Start()
    {
        float screenWidth = Screen.width + offscreenOffset;
        startPos = new Vector2(-screenWidth, asset.anchoredPosition.y);
        endPos = new Vector2(screenWidth, asset.anchoredPosition.y);

        StartThrow();
    }

    void StartThrow()
    {
        asset.anchoredPosition = startPos;

        LeanTween.value(gameObject, 0f, 1f, travelTime)
            .setEaseLinear()
            .setOnUpdate((float t) =>
            {
               
                float x = Mathf.Lerp(startPos.x, endPos.x, t);
                float y = Mathf.Lerp(startPos.y, endPos.y, t)
                          + Mathf.Sin(t * Mathf.PI) * height;

                asset.anchoredPosition = new Vector2(x, y);
                asset.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            })
            .setOnComplete(() =>
            {
                StartThrow();
            });
    }
}
