using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance { get; private set; }

    public bool paused = false;
    [SerializeField] private GameObject uiGO;
    [SerializeField] private Button characterSelectionButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        if (NetworkBetweenScenesManager.Instance.Runner.IsServer)
            characterSelectionButton.interactable = true;
        else
            characterSelectionButton.interactable = false;
    }

    private void Update()
    {
        if (!NetworkBetweenScenesManager.Instance.isInGameplay || WinScreenHandler.Instance.gameEnded)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiGO.SetActive(!uiGO.activeInHierarchy);
            paused = uiGO.activeInHierarchy;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ReturnToCharacterSelection()
    {
        NetworkBetweenScenesManager.Instance.PostGameReset(true);
        NetworkBetweenScenesManager.Instance.LoadSceneToHost(0);
    }

    public void ExitMatch()
    {
        NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
    }
}
