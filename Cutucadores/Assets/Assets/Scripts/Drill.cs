using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    private PlayerControl player;
    private void Start()
    {
        player = GetComponentInParent<PlayerControl>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Drill"))
        {

        }
    }
}
