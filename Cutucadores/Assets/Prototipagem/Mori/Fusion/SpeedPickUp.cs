using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickUp : MonoBehaviour
{
    public Transform visual;
    public float rotationSpeed;
    public float yDelta;
    public float yDeltaFrequency;

    private Vector3 visualStartposition;
    private  void Awake()
    {
        visualStartposition = visual.position;
    }
    private void FixedUpdate()
    {
        visual.Rotate(0, rotationSpeed, 0,Space.Self);
        visual.position = new Vector3(visualStartposition.x,
            visualStartposition.y + Mathf.Abs(Mathf.Sin(Time.time* yDeltaFrequency)) * yDelta
            , visualStartposition.z);
    }
    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
    public IEnumerator LookAtCamera()
    {
        yield return null;
    }
}
