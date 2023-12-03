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
        else Instance = this;
    }
    public static IEnumerator Load(string sceneName)
    {
        Scene oldScene = SceneManager.GetActiveScene();

        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while(! ao.isDone && ao != null)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }    
    protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
    {
        
        List<NetworkObject> sceneObjects = new List<NetworkObject>();
        yield return SceneManager.LoadSceneAsync(mapIndex, LoadSceneMode.Single);
        Scene loadedScene = SceneManager.GetSceneByBuildIndex(mapIndex);
        sceneObjects = FindNetworkObjects(loadedScene, disable: false);

        yield return null;
        finished(sceneObjects);
    }
}
