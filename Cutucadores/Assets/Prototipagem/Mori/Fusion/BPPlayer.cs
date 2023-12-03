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
            NetworkBetweenScenesManager.Instance.userIDList.Add(Runner.UserId);
            NetworkBetweenScenesManager.Instance.userIDToPlayerData.Add(Runner.UserId, new PlayerData
            {
                username = CursorController.Instance.nicknameInputField.text
                ,
                character = Character.Null
            });
            Debug.Log(NetworkBetweenScenesManager.Instance.userIDToPlayerData[Runner.UserId]);
        }
    }
}
