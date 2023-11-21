using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : NetworkBehaviour
{
    NetworkCharacterDrillController networkCharacterController;
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
                            networkCharacterController.Knockback(collision.GetContact(0).point, true);
                        }
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
                            Debug.Log("Hit Player");
                            transform.root.GetComponent<HPHandler>().OnTakeDamage(1);
                            networkCharacterController.Knockback(collision.GetContact(0).point, false);
                        }
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
        }
    }
}
