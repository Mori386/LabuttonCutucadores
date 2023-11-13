using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickUp : NetworkBehaviour
{
    private CapsuleCollider pickupCollider;

    [Header("Respawn")]
    public readonly float respawnTime = 15f;
    [Header("Item")]
    public Transform itemVisual;
    public float itemRotationSpeed;
    public float itemYDelta;
    public float itemYDeltaFrequency;

    [Header("PickupArea")]
    public Transform pickupAreaVisual;

    [Networked] TickTimer respawnTickTimer { get; set;}
    private void Awake()
    {
        pickupCollider = GetComponent<CapsuleCollider>();
    }
    private void Start()
    {
        itemCoroutine = StartCoroutine(RotateAndFloatObject(itemVisual, itemRotationSpeed, itemYDeltaFrequency, itemYDelta));
    }
    public override void Spawned()
    {
        base.Spawned();
        itemCoroutine = StartCoroutine(RotateAndFloatObject(itemVisual, itemRotationSpeed, itemYDeltaFrequency, itemYDelta));
    }
    private Coroutine itemCoroutine;
    public IEnumerator RotateAndFloatObject(Transform visualTransform, float rotationSpeed, float deltaFrequency, float YDelta)
    {
        Vector3 startingPosition = visualTransform.localPosition;
        while (true)
        {
            visualTransform.Rotate(0, rotationSpeed, 0, Space.Self);
            visualTransform.localPosition = new Vector3(visualTransform.localPosition.x,
                startingPosition.y + Mathf.Abs(Mathf.Sin(Time.time * deltaFrequency)) * YDelta
                , visualTransform.localPosition.z);
            yield return new WaitForFixedUpdate();
        }
    }

    public void GetPowerUp(Transform player)
    {
        //Deactivate collider
        pickupCollider.enabled = false;
        itemVisual.gameObject.SetActive(false);
        pickupAreaVisual.gameObject.SetActive(false);
        respawnTickTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
    }

    public override void FixedUpdateNetwork()
    {
       if(respawnTickTimer.Expired(Runner))
        {
            Respawn();
        }
    }
    public void Respawn()
    {
        itemVisual.gameObject.SetActive(true);
        pickupAreaVisual.gameObject.SetActive(true);
        pickupCollider.enabled = true;
    }
}
