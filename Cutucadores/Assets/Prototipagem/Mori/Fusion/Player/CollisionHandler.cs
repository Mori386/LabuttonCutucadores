using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Collider selfCollider = collision.GetContact(0).thisCollider;

        switch (selfCollider.tag)
        {
            case "Player":
                //Collisions made with playerPart

                break;
            case "Drill":
                //Collisions made with DrillPart
                switch (collision.collider.tag)
                {
                    case "Player":
                        Debug.Log("Hit Player");
                        if (Object.HasStateAuthority)
                        {
                            Debug.Log("Hit Player");
                            collision.transform.root.GetComponent<HPHandler>().OnTakeDamage(1);
                        }
                        break;
                    case "Drill":
                        if (Object.HasStateAuthority)
                        {
                            Debug.Log("Hit Drill");
                        }
                        break;
                }
                break;
        }
    }
}
