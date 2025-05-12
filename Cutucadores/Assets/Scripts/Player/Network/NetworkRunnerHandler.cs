using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

public class NetworkRunnerHandler : MonoBehaviour
{
    static public NetworkRunnerHandler Instance;
    public NetworkRunner networkRunnerPrefab;

    private NetworkRunner networkRunner;
    private NetworkRunnerReceiver networkRunnerReceiver;

    private NetworkSceneManagerDefault sceneManager;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        DontDestroyOnLoad(gameObject);
        if(networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunnerReceiver = networkRunner.GetComponent<NetworkRunnerReceiver>();
            networkRunner.name = "Network Runner";
        }
        if (sceneManager == null)
        {
            if (networkRunner.TryGetComponent(out NetworkSceneManagerDefault sceneManagerDefault))
                sceneManager = sceneManagerDefault;
            else
                sceneManager = networkRunner.AddComponent<NetworkSceneManagerDefault>();
        }
    }
    public Task StartLobby()
    {
        networkRunner.Shutdown();
        return networkRunner.JoinSessionLobby(SessionLobby.Custom, "Lobby");
    }

    public Task StartNetworkRunner(string sessionName,GameMode gamemode, Dictionary<string, SessionProperty> sessionProperties)
    {
        return InitializeNetworkRunner(networkRunner, gamemode, NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null, sessionName, sceneManager, sessionProperties);
    }
    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address,SceneRef scene, Action<NetworkRunner> initialized,string sessionName,INetworkSceneManager sceneManager, Dictionary<string, SessionProperty> sessionProperties)
    {
        runner.ProvideInput = true;
        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            Initialized = initialized,
            SceneManager = sceneManager,
            SessionProperties = sessionProperties,
        });

    }
    public void ShutdownNetworkRunner()
    {
        networkRunner.Shutdown();
    }
}
