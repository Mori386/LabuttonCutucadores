using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TweenManager;

public class PopUp : MonoBehaviour
{
    [SerializeField] private RectTransform Asset;
    public float duration = 0.6f;

    private void Start()
    {
       // Instance.PlayPopUp(Asset,duration);
    }

    void OnEnable()
    {
        Instance.PlayPopUp(Asset);
    }

    void OnDisable()
    {
        Instance.StopTween(Asset);
    }
}
