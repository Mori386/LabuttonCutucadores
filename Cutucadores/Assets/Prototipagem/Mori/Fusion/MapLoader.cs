using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLoader : NetworkSceneManagerBase
{
    public static MapLoader Instance;
    public int mapIndex;
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public static IEnumerator Load(string sceneName, int mapInt)
    {
        //Scene oldScene = SceneManager.GetActiveScene();

        //AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        //while (!ao.isDone && ao != null)
        //{
        //    yield return null;
        //}
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        Instance.mapIndex = mapInt;
        yield return null;
        Instance.Runner.SetActiveScene(sceneName);
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
    }
}
