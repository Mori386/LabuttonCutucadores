using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterData;
public class NetworkBetweenScenesManager : NetworkBehaviour, IAfterSpawned
{
    public string selfUserID;
    public bool spawned;
    public static NetworkBetweenScenesManager Instance;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_UserIDDictionary(string userID,string nickname)
    {
        userIDList.Add(userID);
        userIDToPlayerData.Add(userID, new PlayerData
        {
            username = nickname
            ,
            character = Character.Null
        });
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public override void Spawned()
    {
        base.Spawned();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void AfterSpawned()
    {
        spawned = true;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        spawned = false;
    }
    [Networked]
    [Capacity(4)]
    public NetworkLinkedList<NetworkString<_64>> userIDList { get; }
    [Networked]
    [Capacity(4)]
    public NetworkDictionary<NetworkString<_64>, PlayerData> userIDToPlayerData { get; }
    public CharacterData GetDataFromUserID(string userID)
    {
        if (userIDToPlayerData.TryGet(userID, out PlayerData playerData))
        {
            switch (playerData.character)
            {
                case Character.Escavador:
                default:
                    return BetweenScenesPlayerInfos.Instance.escavadorCharData;
                case Character.Minerador:
                    return BetweenScenesPlayerInfos.Instance.mineradorCharData;
                case Character.PaiEFilha:
                    return BetweenScenesPlayerInfos.Instance.PaiEFilhaCharData;
                case Character.Vovo:
                    return BetweenScenesPlayerInfos.Instance.VovoCharData;
            }
        }
        else
        {
            Debug.LogError("Error in search to find " + userID + " inCharacterID");
            return null;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All,Channel = RpcChannel.Reliable,InvokeLocal = true)]
    public void RPC_LockCharacter(string userID, Character character)
    {
        BPCharacter thisCharacterBP;
        switch (character)
        {
            default:
            case Character.Escavador:
                thisCharacterBP = CursorController.Instance.escavadorCharBP;
                break;
            case Character.Minerador:
                thisCharacterBP = CursorController.Instance.mineradorCharBP;
                break;
            case Character.PaiEFilha:
                thisCharacterBP = CursorController.Instance.paiEFilhaCharBP;
                break;
            case Character.Vovo:
                thisCharacterBP = CursorController.Instance.vovoCharBP;
                break;
        }
        if (userIDToPlayerData.TryGet(userID, out PlayerData myPlayerData))
        {
            thisCharacterBP.selectButton.interactable = false;
            myPlayerData.character = character;
            thisCharacterBP.usernameText.text = myPlayerData.username.ToString();
            for (int i = 0; i < thisCharacterBP.characterAnimator.Length; i++)
            {
                thisCharacterBP.characterAnimator[i].SetTrigger("isSelected");
            }
            StartCoroutine(ChangeTankMaterial(thisCharacterBP));
        }
    }
    public IEnumerator ChangeTankMaterial(BPCharacter bPCharacter)
    {
        float timer = 0f;
        float duration = 0.75f;
        Vector3 startRotationSpeed = bPCharacter.rotateObjectScript.rotate;
        Vector3 startScale = bPCharacter.rotateObjectScript.transform.localScale;
        while (timer < (duration / 3))
        {
            bPCharacter.rotateObjectScript.rotate = startRotationSpeed * Mathf.Lerp(1, 5, timer / (duration / 3));
            timer += Time.deltaTime;
            yield return null;
        }
        bPCharacter.rotateObjectScript.rotate = startRotationSpeed * 5f;

        timer = 0;
        while (timer < (duration / 3))
        {
            bPCharacter.rotateObjectScript.transform.localScale = startScale * Mathf.Lerp(1, 0.1f, timer / (duration / 3));
            timer += Time.deltaTime;
            yield return null;
        }
        bPCharacter.rotateObjectScript.transform.localScale = startScale * 0.1f;

        bPCharacter.onMatChangeParticle.Play();
        bPCharacter.drillBodyMeshRenderer.material = bPCharacter.defaultMaterial;
        bPCharacter.drillHeadMeshRenderer.material = bPCharacter.defaultMaterial;
        timer = 0;
        while (timer < (duration / 3))
        {
            bPCharacter.rotateObjectScript.rotate = startRotationSpeed * Mathf.Lerp(5, 1, timer / (duration / 3));
            bPCharacter.rotateObjectScript.transform.localScale = startScale * Mathf.Lerp(0.1f, 1, timer / (duration / 3));
            timer += Time.deltaTime;
            yield return null;
        }
        bPCharacter.rotateObjectScript.rotate = startRotationSpeed;
        bPCharacter.rotateObjectScript.transform.localScale = startScale;
    }
}
public struct PlayerData : INetworkStruct
{
    public NetworkString<_16> username;
    public Character character;
}
