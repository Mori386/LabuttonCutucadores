using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class NetworkRunnerHandler : MonoBehaviour
{
    static public NetworkRunnerHandler Instance;
    public NetworkRunner networkRunnerPrefab;

    private NetworkRunner networkRunner;
    private NetworkRunnerReceiver networkRunnerReceiver;
    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    public Task StartNetworkRunner(string sessionName,GameMode gamemode)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunnerReceiver = networkRunner.GetComponent<NetworkRunnerReceiver>();
        networkRunner.name = "Network Runner";
        return InitializeNetworkRunner(networkRunner, gamemode, NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null, sessionName);
    }
    void Start()
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner";

        Task clientTask = InitializeNetworkRunner(networkRunner,GameMode.AutoHostOrClient,NetAddress.Any(),SceneManager.GetActiveScene().buildIndex,null,"TestRoom");
        Debug.Log($"Server NetworkRunner started.");
    }
    
    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address,SceneRef scene, Action<NetworkRunner> initialized,string sessionName)
    {
        INetworkSceneManager sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        runner.ProvideInput = true;
        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            Initialized = initialized,
            SceneManager = sceneManager
        });

    }
}
