using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterData;

public class BPPlayer : NetworkBehaviour , IAfterSpawned
{
    void IAfterSpawned.AfterSpawned()
    {
        if (Object.HasInputAuthority)
        {
            NetworkBetweenScenesManager.Instance.Rpc_UserIDDictionary(Runner.UserId,CursorController.Instance.nicknameInputField.text);
            //Debug.Log(NetworkBetweenScenesManager.Instance.userIDToPlayerData[Runner.UserId]);
            CursorController.Instance.carimbo = gameObject;
            NetworkRunnerReceiver.Instance.thisBpPlayer = this;
        }
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        if (Object.InputAuthority == null)
        {
            NetworkBetweenScenesManager.Instance.RPC_UnlockCharacter(NetworkBetweenScenesManager.Instance.selfUserID);
            NetworkBetweenScenesManager.Instance.Rpc_RemoveUserID(NetworkBetweenScenesManager.Instance.selfUserID);
        }
    }
}
