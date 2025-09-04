using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TanksAnim : MonoBehaviour

{
    [SerializeField] private GameObject leftTank;
    [SerializeField] private GameObject rightTank;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private float moveTime = 1f;

    private void Start()
    {
        LeanTween.moveLocalX(
            leftTank,
            leftTank.transform.localPosition.x + moveDistance,
            moveTime
        ).setEaseInOutSine().setLoopPingPong();

        LeanTween.moveLocalX(
            rightTank,
            rightTank.transform.localPosition.x - moveDistance,
            moveTime
        ).setEaseInOutSine().setLoopPingPong();
    }
}
