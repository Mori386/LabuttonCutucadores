using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class MapLoader : MonoBehaviour
{
    public static MapLoader Instance;
    public int mapIndex;
    public string sceneName;
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    /*public static IEnumerator Load(string sceneName, int mapInt)
    {
        Instance.mapIndex = mapInt;
        Instance.sceneName = sceneName;
        yield return null;
        if (Instance.Runner.IsServer)
        {
            Instance.Runner.SetActiveScene(sceneName);
        }
    }
    protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
    {
        if (prevScene != SceneRef.None)
        {
            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            yield return SceneManager.LoadSceneAsync(mapIndex, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(0);
            sceneObjects = FindNetworkObjects(SceneManager.GetActiveScene(), disable: false);

            yield return null;
            finished(sceneObjects);
        }
        else
        {
            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            sceneObjects = FindNetworkObjects(SceneManager.GetActiveScene(), disable: false);

            yield return null;
            finished(sceneObjects);
        }
        if (Instance.Runner.IsServer)
        {
            RPC_LoadClients();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_LoadClients()
    {
        if (!Instance.Runner.IsServer)
        {
            Instance.Runner.SetActiveScene(sceneName);
        }
    }*/
}
