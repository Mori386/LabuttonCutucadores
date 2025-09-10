using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : NetworkBehaviour
{
    NetworkCharacterDrillController networkCharacterController;
    private HPHandler attacker = null;
    private HPHandler thisHPHandler;
    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterDrillController>();
        thisHPHandler = transform.root.GetComponent<HPHandler>();
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
                        HPHandler attackedHPHandler = collision.transform.root.GetComponent<HPHandler>();
                        if (Object.HasStateAuthority)
                        {
                            networkCharacterController.Knockback(collision.GetContact(0).point, true);
                        }
                        StartFallChecker(attackedHPHandler);
                        attackedHPHandler.RPC_OnHitTaken(thisHPHandler, collision.GetContact(0).point);
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
                        GameManager.Instance.PlayOnBodyHitParticle(collision.GetContact(0).point);
                        break;
                    case "Drill":
                        //If other player drill hit this player drill
                        if (Object.HasStateAuthority)
                        {                    
                            networkCharacterController.Knockback(collision.GetContact(0).point, true);
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
                break;
            case "TunnelBarricade":
                if (selfCollider.CompareTag("Drill"))
                    collision.transform.parent.GetComponent<ParticleSystem>().Play();
                    GameManager.Instance.PlayOnDrillHitParticle(collision.GetContact(0).point);
                    collision.gameObject.SetActive(false);
                    if (Object.HasStateAuthority)
                    {                    
                        networkCharacterController.Knockback(collision.GetContact(0).point, true);
                    }
                    if (Object.HasInputAuthority)
                    {
                        GameManager.Instance.ShakeCamera(GameManager.Instance.onDrillHitCameraShakeAmplitude);
                    }
                break;
            case "TunnelEntrance":
                if (Object.HasStateAuthority)
                {
                    networkCharacterController.EnterTunnel(collision.transform, collision.gameObject.GetComponent<TunnelHandler>().destination);
                }
                break;
        }
    }

    public void HandleFall()
    {
        Debug.Log("Fall");
        if (attacker != null)
        {
            attacker.IncreaseScore(1);
            attacker = null;
        }
        else
        {
            thisHPHandler.DecreaseScore(1);
        }
    }

    public void StartFallChecker(HPHandler attacker)
    {
        StopAllCoroutines();
        this.attacker = null;
        StartCoroutine(FallChecker(attacker));
    }

    private IEnumerator FallChecker(HPHandler attacker)
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
