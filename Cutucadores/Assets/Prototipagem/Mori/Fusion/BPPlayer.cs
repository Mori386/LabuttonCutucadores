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
    public void OnPlayerLeave()
    {
        if (Object.HasInputAuthority)
        {
            NetworkBetweenScenesManager.Instance.RPC_UnlockCharacter(Runner.UserId);
            NetworkBetweenScenesManager.Instance.Rpc_RemoveUserID(Runner.UserId);
        }
    }
}
