using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    [SerializeField] private GameObject[] pedras;  
    [SerializeField] private float shakeStrength = 3f; 
    [SerializeField] private float shakeTime = 0.5f;  

    private void Start()
    {
        foreach (var pedra in pedras)
        {
            ShakePedra(pedra);
        }
    }

    private void ShakePedra(GameObject pedra)
    {
        Vector3 originalPos = pedra.transform.localPosition;

        LeanTween.moveLocalX(
            pedra,
            originalPos.x + Random.Range(-shakeStrength, shakeStrength),
            shakeTime
        ).setEaseInOutSine()
         .setLoopPingPong()
         .setOnComplete(() =>
         {
             ShakePedra(pedra);
         });
    }
}
