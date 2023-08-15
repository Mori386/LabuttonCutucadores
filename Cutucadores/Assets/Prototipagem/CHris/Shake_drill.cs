using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake_drill : MonoBehaviour
{
    Rigidbody2D rb; 
    [SerializeField] private float intervalo = 0.25f;
    [SerializeField] private float posicao = 0.25f;
    private float shake_multiplier;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        shake = StartCoroutine(Shake());
    }
    Coroutine shake;
    private IEnumerator Shake()
    {
        while (true)
        {
            if(Mathf.Abs(rb.velocity.x)> 0 && Mathf.Abs(rb.velocity.y) > 0)
            {
                shake_multiplier = 1;
            }
            else
            {   
                shake_multiplier = 0.5f;
            }
            transform.localPosition = new Vector3(Random.Range(-posicao,posicao) * shake_multiplier, Random.Range(-posicao, posicao)* shake_multiplier, 0);
            yield return new WaitForSeconds(intervalo);
        }
    }
}
