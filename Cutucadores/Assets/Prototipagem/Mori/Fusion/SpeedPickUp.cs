using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickUp : MonoBehaviour
{
    private CapsuleCollider pickupCollider;

    [Header("Item")]
    public Transform itemVisual;
    public float itemRotationSpeed;
    public float itemYDelta;
    public float itemYDeltaFrequency;

    [Header("Arrows")]

    public Transform arrowVisual;
    private Transform[] individualArrows;
    public float arrowRotationSpeed;
    public float arrowYDelta;
    public float arrowYDeltaFrequency;

    [Header("PickupArea")]
    public Transform pickupAreaVisual;
    private void Awake()
    {
        pickupCollider = GetComponent<CapsuleCollider>();
        individualArrows = new Transform[arrowVisual.childCount];
        for(int i = 0;i<arrowVisual.childCount;i++)
        {
            individualArrows[i] = arrowVisual.GetChild(i);
        }
    }
    private void Start()
    {
        itemCoroutine = StartCoroutine(RotateAndFloatObject(itemVisual, itemRotationSpeed, itemYDeltaFrequency, itemYDelta));
        arrowCoroutine = StartCoroutine(RotateAndFloatObject(arrowVisual, arrowRotationSpeed, arrowYDeltaFrequency, arrowYDelta));
    }
    private Coroutine itemCoroutine;
    private Coroutine arrowCoroutine;
    public IEnumerator RotateAndFloatObject(Transform visualTransform,float rotationSpeed,float deltaFrequency, float YDelta)
    {
        Vector3 startingPosition = visualTransform.localPosition;
        while(true)
        {
            visualTransform.Rotate(0, rotationSpeed, 0, Space.Self);
            visualTransform.localPosition = new Vector3(startingPosition.x,
                startingPosition.y + Mathf.Abs(Mathf.Sin(Time.time * deltaFrequency)) * YDelta
                , startingPosition.z);
            yield return new WaitForFixedUpdate();
        }
    }

    public void GetPowerUp(Transform player)
    {
        //Deactivate collider
        pickupCollider.enabled = false;

        //Deactivate item visual
        StopCoroutine(itemCoroutine);
        itemCoroutine = null;
        itemVisual.gameObject.SetActive(false);

        //Deactivate pickup area
        pickupAreaVisual.gameObject.SetActive(false);

        //Deactivate arrow spinning
        StopCoroutine(arrowCoroutine);
        arrowCoroutine = null;

        StartCoroutine(TransferArrowsPowerUpToPlayer(player));
    }
    public IEnumerator TransferArrowsPowerUpToPlayer(Transform playerTransform)
    {
        float timer = 0;
        Vector3 arrowStartPos = arrowVisual.position;
        arrowCoroutine = StartCoroutine(RotateAndFloatObject(arrowVisual, arrowRotationSpeed*3f, arrowYDeltaFrequency, arrowYDelta));
        arrowVisual.localScale = Vector3.zero;
        arrowVisual.parent = playerTransform;
        arrowVisual.localPosition = new Vector3(0,4,0);
        while (timer<0.25f)
        {
            arrowVisual.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, timer / 0.25f);
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        timer = 0;
        while (timer < 0.25f)
        {
            arrowVisual.localScale = Vector3.Lerp(Vector3.one* 1.5f, Vector3.one * 1.75f, timer / 0.25f);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < 0.25f)
        {
            arrowVisual.localScale = Vector3.Lerp(Vector3.one * 1.75f, Vector3.zero, timer / 0.25f);
            timer += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(arrowCoroutine);
        arrowCoroutine = null;
        arrowVisual.gameObject.SetActive(false);
    }
    public IEnumerator RespawnTimer()
    {
        yield return null;
    }
}
