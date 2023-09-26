using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControl;

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
            Debug.Log("HitDrill");
            //PlayerControl otherPlayer = GameManager.Instance.GetPlayerControl(collision.collider.transform.parent.gameObject);
            //player.OnDrilltoDrillHit(otherPlayer.transform);
            //otherPlayer.OnDrilltoDrillHit(player.transform);
            collision.collider.transform.parent.GetComponent<PlayerControl>().OnDrilltoDrillHit(player.transform);
            player.OnDrilltoDrillHit(collision.collider.transform.parent);
        }
        else if (collision.collider.CompareTag("Player"))
        {
            Vector2 forceApplied = (transform.position- collision.transform.position).normalized;
            player.RecoilOnHit(forceApplied);
            if(player.playerType.Equals(PlayerTypes.Input))
            {
                player.SendInfo(InfoType.PlHit, Mathf.Abs(player.playerID - 1).ToString() + forceApplied.x + "Y" + forceApplied.y);
            }

        }
    }
}
