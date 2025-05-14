using UnityEngine;

public class SceneLoaderCheck : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject winScreenPrefab;
    [SerializeField] private GameObject[] boostsArray;

    void Start()
    {
        if (!NetworkBetweenScenesManager.Instance.Runner.IsServer)
            return;
        if (!NetworkBetweenScenesManager.Instance.GameManagerSpawned)
        {
            NetworkBetweenScenesManager.Instance.Runner.Spawn(gameManagerPrefab);
            NetworkBetweenScenesManager.Instance.GameManagerSpawned = true;
        }
        if (!NetworkBetweenScenesManager.Instance.winScreenSpawned)
        {
            NetworkBetweenScenesManager.Instance.Runner.Spawn(winScreenPrefab);
            NetworkBetweenScenesManager.Instance.winScreenSpawned = true;
            foreach (GameObject boost in boostsArray)
                NetworkBetweenScenesManager.Instance.Runner.Spawn(boost);
        }
    }
}
