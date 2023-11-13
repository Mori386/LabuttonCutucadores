using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkVisualHandler : NetworkBehaviour
{
    NetworkMecanimAnimator mecanimAnimator;
    NetworkCharacterDrillController characterDrillController;
    ExplosionHandler explosionHandler;

    public Transform drillVisual;

    private void Awake()
    {
        mecanimAnimator = GetComponent<NetworkMecanimAnimator>();
        characterDrillController = GetComponent<NetworkCharacterDrillController>();
        explosionHandler = GetComponentInChildren<ExplosionHandler>();
    }
    void Start()
    {
        //Load character visual
    }
    public void OndDeath()
    {
        explosionHandler.Explode();
    }
    public override void Spawned()
    {
        base.Spawned();
        StartCoroutine(RotateDrillCoroutine());
    }
    [Networked]
    [HideInInspector]public float rotationDirection { get; set; }
    public void RotateDrill()
    {
        drillVisual.Rotate((2.5f + rotationDirection * characterDrillController.Velocity.magnitude / 40f), 0, 0, Space.Self);
    }

    public IEnumerator RotateDrillCoroutine()
    {
        while (true)
        {
            RotateDrill();
            yield return new WaitForFixedUpdate();
        }
    }
}
