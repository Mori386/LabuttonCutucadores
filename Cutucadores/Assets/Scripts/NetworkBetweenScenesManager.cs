using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CharacterData;
public class NetworkBetweenScenesManager : NetworkBehaviour, IAfterSpawned
{
    //Singleton
    public static NetworkBetweenScenesManager Instance;
    [Networked] public NetworkBool isInGameplay { get; set; }
    [Networked] public NetworkBool GameManagerSpawned { get; set; }
    [Networked] public NetworkBool winScreenSpawned { get; set; }
    [Networked] public bool PlayersGOSpawned { get; set; }

    public string selfUserID;
    public bool spawned;
    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this);
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
    public void PostGameReset()
    {
        isInGameplay = false;
        GameManagerSpawned = false;
        winScreenSpawned = false;
        PlayersGOSpawned = false;
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            PlayerData resetData = pair.Value;
            resetData.character = Character.Null;
            resetData.loaded = false;
            resetData.isDead = false;
            userIDToPlayerData.Set(pair.Key, resetData);
        }
    }

    #region Character Select
    //Select and deselect characters
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
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
            StartCoroutine(ChangeTankMaterial(thisCharacterBP, thisCharacterBP.defaultMaterial));
        }
        else Debug.Log("Exception: " + userID);
        RPC_CheckForPlayerReady();
    }

    public void UnlockCharacter(PlayerRef userID)
    {
        if (Instance.GameManagerSpawned)
            return;
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            if (pair.Value.playerRef == userID)
            {
                if (pair.Value.character == Character.Null)
                    return;
                BPCharacter thisCharacterBP;
                switch (pair.Value.character)
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
                if (thisCharacterBP.selectButton != null)
                {
                    thisCharacterBP.selectButton.interactable = true;
                    thisCharacterBP.usernameText.text = "Nome do jogador";
                    StartCoroutine(ChangeTankMaterial(thisCharacterBP, thisCharacterBP.BpEffectMaterial));
                }
                PlayerData player = pair.Value;
                player.character = Character.Null;
                userIDToPlayerData.Set(pair.Key, player);
            }
        }
        if (userIDToPlayerData[selfUserID].character != Character.Null)
        {
            CursorController.Instance.escavadorCharBP.selectButton.interactable = false;
            CursorController.Instance.mineradorCharBP.selectButton.interactable = false;
            CursorController.Instance.paiEFilhaCharBP.selectButton.interactable = false;
            CursorController.Instance.vovoCharBP.selectButton.interactable = false;
        }
        RPC_CheckForPlayerReady();
    }

    //Change from blueprint material to real material 
    public IEnumerator ChangeTankMaterial(BPCharacter bPCharacter, Material newMat)
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
        bPCharacter.drillBodyMeshRenderer.material = newMat;
        bPCharacter.drillHeadMeshRenderer.material = newMat;
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

    //Every time someone locks in a character it checks if all players has locked there characters unlock the play button
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_CheckForPlayerReady()
    {
        int playerInSession = userIDToPlayerData.Count;
        int playersReady = 0;
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
    public CharacterData GetDataFromUserID(string userID)
    {
        //Get character data from playerData
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
    #endregion

    #region User Info Storage
    [Networked]
    [Capacity(4)]
    public NetworkLinkedList<NetworkString<_256>> userIDList { get; }
    [Networked]
    [Capacity(4)]
    public NetworkDictionary<NetworkString<_256>, PlayerData> userIDToPlayerData { get; }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void Rpc_UserIDDictionary(string userID, string nickname, PlayerRef playerReference)
    {
        if (userIDList.Contains(userID)) return;
        userIDList.Add(userID);
        userIDToPlayerData.Add(userID, new PlayerData
        {
            username = nickname,
            character = Character.Null,
            playerRef = playerReference,
            loaded = false
        });
    }
    public void RemoveUserID(PlayerRef userID)
    {
        Debug.Log($"Attempting to remove ID {userID}");
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            if (pair.Value.playerRef == userID)
            {
                PlayerData disconnectedData = pair.Value;
                disconnectedData.isDead = true;
                userIDToPlayerData.Set(pair.Key, disconnectedData);
                Debug.Log($"Setting {pair.Value.username} as dead (isDead = {userIDToPlayerData[pair.Key].isDead}) and removing {userID} from userIDList and userIDToPlayerData.");
                userIDToPlayerData.Remove(pair.Key);
                userIDList.Remove(pair.Key);
                if (Instance.GameManagerSpawned && Runner.IsServer)
                    GameManager.Instance.RPC_CheckForPlayersDead();
                return;
            }
        }
    }
    public void SetPlayerAsDead(PlayerRef userID)
    {
        Debug.Log($"Attempting to remove ID {userID}");
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            if (pair.Value.playerRef == userID)
            {
                PlayerData disconnectedData = pair.Value;
                disconnectedData.isDead = true;
                userIDToPlayerData.Set(pair.Key, disconnectedData);
                Debug.Log($"Setting {pair.Value.username} as dead (isDead = {userIDToPlayerData[pair.Key].isDead})");
                if (Runner.IsServer)
                    GameManager.Instance.RPC_CheckForPlayersDead();
                return;
            }
        }
    }
    #endregion

    #region Load Map
    public void LoadSceneToHost(int mapIndex)
    {
        if (Runner.IsServer)
        {
            NetworkRunnerHandler.Instance.ManageRoomVisibility(mapIndex > 0 ? 0 : 1);
            var sceneManager = Runner.SceneManager as NetworkSceneManagerDefault;
            sceneManager.LoadSceneAsync(mapIndex, new LoadSceneParameters(LoadSceneMode.Single), (_) => RPC_LoadSceneToClients(mapIndex));
            if (mapIndex > 0) canvas.SetActive(true);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = false)]
    private void RPC_LoadSceneToClients(SceneRef scene)
    {
        if (!Runner.IsServer)
        {
            var sceneManager = Runner.SceneManager as NetworkSceneManagerDefault;
            sceneManager.LoadSceneAsync(scene, new LoadSceneParameters(LoadSceneMode.Single), null);
            if (scene > 0) canvas.SetActive(true);
        }
    }
    
    public void SetPlayerLoaded(string userID)
    {
        if (!Runner.IsServer || PlayersGOSpawned) return;
        if (userIDToPlayerData.TryGet(userID, out PlayerData myPlayerData))
        {
            Debug.Log($"Setting {myPlayerData.username} to loaded.");
            PlayerData thisPlayerData = myPlayerData;
            thisPlayerData.loaded = true;
            userIDToPlayerData.Set(userID, thisPlayerData);
        }
        else return;
        Debug.Log($"Checking if all players are loaded.");
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            Debug.Log($"{pair.Value.username} {(pair.Value.loaded? "loaded.": "not loaded.")}");
            if (!pair.Value.loaded) return;
        }
        Debug.Log($"Every player is loaded. Spawning game objects.");
        PlayersGOSpawned = true;
        int playerNumber = 0;
        foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in userIDToPlayerData)
        {
            NetworkObject playerObj = Runner.Spawn(GameManager.Instance.playerPrefab, GameManager.Instance.playerSpawnpoints[playerNumber].position, GameManager.Instance.playerSpawnpoints[playerNumber].rotation, pair.Value.playerRef);
            Runner.SetPlayerObject(pair.Value.playerRef, playerObj);
            Debug.Log($"{pair.Value.username} game object spawned.");
            playerNumber++;
        }
        StartCoroutine(HostCountdown());
    }

    //Dá pra substituir isso aqui pra chamar uma animação ou algo mais bonitinho, deixei de placeholder
    private IEnumerator HostCountdown()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (i > 0 && i <= 3)
                RPC_UpdateCountdownUI(i.ToString());
            else if (i == 4)
                RPC_UpdateCountdownUI("Vai!");
            else if (i == 5)
            {
                RPC_UpdateCountdownUI("");
                RPC_ManageCanvas(false);
                isInGameplay = true;
            }
            yield return new WaitForSeconds(1);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
    private void RPC_ManageCanvas(bool state)
    {
        canvas.SetActive(state);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
    private void RPC_UpdateCountdownUI(string text)
    {
        countdownText.text = text;
    }
    #endregion
}
public struct PlayerData : INetworkStruct
{
    public PlayerRef playerRef;
    public NetworkString<_16> username;
    public Character character;
    public NetworkBool loaded;
    public NetworkBool isDead;
}
