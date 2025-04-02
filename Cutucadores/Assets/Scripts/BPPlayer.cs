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
            Debug.Log($"BPPlayer: PlayerID {Runner.GetPlayerUserId(Object.InputAuthority)} added to dictionary.");
            NetworkBetweenScenesManager.Instance.selfUserID = Runner.GetPlayerUserId(Object.InputAuthority);
            //Coloca no dicionario o player id, o nome e a referencia dele
            NetworkBetweenScenesManager.Instance.Rpc_UserIDDictionary(Runner.GetPlayerUserId(Object.InputAuthority),CursorController.Instance.nicknameInputField.text, Object.InputAuthority);
            PlayerPrefs.SetString("nickname", CursorController.Instance.nicknameInputField.text);
            CursorController.Instance.carimbo = gameObject;
        }
    }
}
