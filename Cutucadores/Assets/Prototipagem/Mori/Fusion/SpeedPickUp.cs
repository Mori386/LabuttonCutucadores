using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickUp : MonoBehaviour
{
    private CapsuleCollider pickupCollider;

    [Header("Respawn")]
    public float respawnTime = 15f;
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
        for (int i = 0; i < arrowVisual.childCount; i++)
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
        StartCoroutine(TransferArrowsPowerUpToPlayer(player));
    }
    public IEnumerator TransferArrowsPowerUpToPlayer(Transform playerTransform)
    {
        float interval = 0.125f;
        float timer = 0;
        Vector3 arrowOriginalPos = arrowVisual.position;
        Vector3 itemOriginalPos = itemVisual.position;
        arrowVisual.parent = playerTransform;
        itemVisual.parent = playerTransform;
        //Item and arrow go to player
        while (timer < interval)
        {
            itemVisual.localPosition = Vector3.Lerp(itemVisual.localPosition, Vector3.zero, timer / interval);
            arrowVisual.localPosition = Vector3.Lerp(arrowVisual.localPosition, new Vector3(0, 4, 1.4f), timer / interval);
            arrowVisual.localScale = Vector3.Lerp(arrowVisual.localScale, Vector3.one * 2f, timer / interval);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        //Deactivate item visual rotation
        StopCoroutine(itemCoroutine);
        itemCoroutine = null;
        itemVisual.gameObject.SetActive(false);

        //Deactivate pickup area
        pickupAreaVisual.gameObject.SetActive(false);

        //Deactivate arrow spinning
        StopCoroutine(arrowCoroutine);
        arrowCoroutine = StartCoroutine(RotateAndFloatObject(arrowVisual, arrowRotationSpeed * 3f, arrowYDeltaFrequency, arrowYDelta));
        yield return new WaitForSeconds(interval*3);
        while (timer < interval)
        {
            arrowVisual.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one * 2.25f, timer / interval);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < interval)
        {
            arrowVisual.localScale = Vector3.Lerp(Vector3.one * 2.25f, Vector3.zero, timer / interval);
            timer += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(arrowCoroutine);
        arrowCoroutine = null;
        arrowVisual.gameObject.SetActive(false);

        itemVisual.parent = transform;
        arrowVisual.parent = transform;

        arrowVisual.position = arrowOriginalPos;
        itemVisual.position = itemOriginalPos;

        StartCoroutine(RespawnTimer());
    }
    public IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnTime);
        arrowVisual.localScale = Vector3.one;
        arrowVisual.gameObject.SetActive(true);
        itemVisual.gameObject.SetActive(true);
        pickupAreaVisual.gameObject.SetActive(true);
        itemCoroutine = StartCoroutine(RotateAndFloatObject(itemVisual, itemRotationSpeed, itemYDeltaFrequency, itemYDelta));
        arrowCoroutine = StartCoroutine(RotateAndFloatObject(arrowVisual, arrowRotationSpeed, arrowYDeltaFrequency, arrowYDelta));
        yield return new WaitForSeconds(0.5f);
        pickupCollider.enabled = true;
    }
}
