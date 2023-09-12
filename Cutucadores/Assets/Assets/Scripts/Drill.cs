using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    [System.NonSerialized]public PlayerControl player;
    private void Start()
    {
        player = GetComponentInParent<PlayerControl>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Drill"))
        {
            collision.collider.GetComponent<Drill>().player.OnDrilltoDrillHit();
        }
    }
}
