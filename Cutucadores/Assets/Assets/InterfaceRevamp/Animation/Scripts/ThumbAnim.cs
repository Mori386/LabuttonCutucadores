using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbAnim : MonoBehaviour
{
    
   public float rotacao = 10f;   
   public float duracao = 0.5f;  

    void Start()
    {
        Balancar();
    }

    private void Balancar()
    {
        LeanTween.rotateZ(gameObject, rotacao, duracao)
            .setEaseInOutSine()
            .setLoopPingPong();
    }
}
