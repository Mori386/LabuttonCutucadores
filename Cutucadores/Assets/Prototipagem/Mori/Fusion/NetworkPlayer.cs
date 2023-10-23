using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {  get;  set; }
    private void Start()
    {
        
    }

    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            Local = this;
            GameManager.Instance.VirtualCamera.Follow = transform;
            GameManager.Instance.VirtualCamera.LookAt = transform;
            Debug.Log("Spawned local player");
        }
        else Debug.Log("Spawned remote player");
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
