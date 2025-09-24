using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    [SerializeField] private RectTransform Asset;
    public float strength = 1.1f;
    public float duration = 0.6f;

    public void Start()
    {
        TweenManager.Instance.PlayShake(Asset, strength, duration);
    }
    private void OnEnable()
    {
        if (Asset == null)
            Asset = GetComponent<RectTransform>();

        if (TweenManager.Instance != null)
            TweenManager.Instance.PlayShake(Asset, strength, duration);

    }

    private void OnDisable()
    {
        TweenManager.Instance.StopTween(Asset);
    }
}
