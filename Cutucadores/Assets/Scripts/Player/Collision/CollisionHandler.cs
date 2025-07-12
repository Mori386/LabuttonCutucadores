using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : NetworkBehaviour
{
    NetworkCharacterDrillController networkCharacterController;
    private Collision attacker = null;
    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterDrillController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collider selfCollider = collision.GetContact(0).thisCollider;
        switch (collision.collider.tag)
        {
            case "Player":
                //Contact with other player body
                switch (selfCollider.tag)
                {
                    case "Player":
                        break;
                    case "Drill":
                        //Contact with other player body with this player drill
                        if (Object.HasStateAuthority)
                        {
                            transform.root.GetComponent<HPHandler>().IncreaseScore(1);
                            networkCharacterController.Knockback(collision.GetContact(0).point, true);
                            StartFallChecker(collision);
                        }
                        //Se for o player que bateu aplica um shake de tela
                        if (Object.HasInputAuthority)
                        {
                            GameManager.Instance.ShakeCamera(GameManager.Instance.onBodyHitCameraShakeAmplitude);
                        }
                        break;
                }
                break;
            case "Drill":
                //Contact with other player drill
                switch (selfCollider.tag)
                {
                    case "Player":
                        //If other player drill hit this player body
                        if (Object.HasStateAuthority)
                        {
                            transform.root.GetComponent<HPHandler>().OnHitTaken();
                        }
                        GameManager.Instance.PlayOnBodyHitParticle(collision.GetContact(0).point);
                        break;
                    case "Drill":
                        //If other player drill hit this player drill
                        if (Object.HasStateAuthority)
                        {                    
                            networkCharacterController.Knockback(collision.GetContact(0).point, true);
                            StartFallChecker(collision);
                        }
                        GameManager.Instance.PlayOnDrillHitParticle(collision.GetContact(0).point);
                        if (Object.HasInputAuthority)
                        {
                            GameManager.Instance.ShakeCamera(GameManager.Instance.onDrillHitCameraShakeAmplitude);
                        }
                        break;
                }
                break;
            case "Speed":
                break;
            case "Fall":
                //HandleFall();
                break;
        }
    }

    public void HandleFall()
    {
        Debug.Log("Fall");
        if (attacker != null)
        {
            attacker.transform.root.GetComponent<HPHandler>().IncreaseScore(1);
            attacker = null;
        }
        else
        {
            transform.root.GetComponent<HPHandler>().DecreaseScore(1);
        }
    }

    private void StartFallChecker(Collision attacker)
    {
        StopAllCoroutines();
        this.attacker = null;
        StartCoroutine(FallChecker(attacker));
    }

    private IEnumerator FallChecker(Collision attacker)
    {
        this.attacker = attacker;
        float count = 0;
        while (count < 2.4f)
        {
            count += Time.deltaTime;
            yield return null;
        }
        this.attacker = null;
    }
}
