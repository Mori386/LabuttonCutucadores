using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    public static Spectator Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void StartFollowCameraNextPlayer()
    {
        if(FollowPlayerCheckCoroutine == null) FollowPlayerCheckCoroutine = StartCoroutine(FollowPlayerCheck());
    }
    public Coroutine FollowPlayerCheckCoroutine;
    public IEnumerator FollowPlayerCheck()
    {
        while (true)
        {
            ChangeCameraFollow();
            yield return new WaitForSeconds(1f);
        }
    }
    public void ChangeCameraFollow()
    {
        for(int i = 0; i < GameManager.Instance.playersControllers.Count; i++)
        {
            if (!GameManager.Instance.playersControllers[i].hpHandler.isDead)
            {
                GameManager.Instance.virtualCamera.Follow = GameManager.Instance.playersControllers[i].transform;
                GameManager.Instance.virtualCamera.LookAt = GameManager.Instance.playersControllers[i].transform;
            }
        }
    }
}
