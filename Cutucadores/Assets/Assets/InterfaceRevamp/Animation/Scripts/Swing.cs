using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour
{
    [SerializeField] private RectTransform Asset;
    public float strength = 10f;
    public float speed = 1.2f;

    void Start()
    {
      SwingAsset();
    }
    void SwingAsset()
    {
       Asset.localRotation = Quaternion.identity;

        LeanTween.rotateZ(Asset.gameObject, strength, speed)
            .setEaseInOutSine()
            .setLoopPingPong(-1); 
    }
}
