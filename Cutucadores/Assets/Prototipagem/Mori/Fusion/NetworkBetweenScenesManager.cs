using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CharacterData;
public class NetworkBetweenScenesManager : NetworkBehaviour, IAfterSpawned
{
    public string selfUserID;
    public bool spawned;
    public static NetworkBetweenScenesManager Instance;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable,InvokeLocal = true)]
    public void Rpc_LoadMap(string mapName, int mapIndex)
    {
        MapLoader.Instance.mapIndex = mapIndex;
        StartCoroutine(MapLoader.Load(mapName));
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
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
            userIDToPlayerData.Set(userID, myPlayerData);
            StartCoroutine(ChangeTankMaterial(thisCharacterBP));
        }
        RPC_CheckForPlayerReady();
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_CheckForPlayerReady()
    {
        int playerInSession = userIDToPlayerData.Count;
        int playersReady=0;
        if (playerInSession > 1)
        {
            for (int i = 0; i < userIDList.Count; i++)
            {
                if (userIDToPlayerData.TryGet(userIDList[i], out PlayerData myPlayerData))
                {
                    if (myPlayerData.character != Character.Null) playersReady++;
                }
            }
            Debug.Log(playersReady);
            CursorController.Instance.hostStartGameButton.gameObject.SetActive(playersReady >= playerInSession);
        }
        else CursorController.Instance.hostStartGameButton.gameObject.SetActive(false);
    }
}
public struct PlayerData : INetworkStruct
{
    public NetworkString<_16> username;
    public Character character;
    public NetworkBool loaded;
}
