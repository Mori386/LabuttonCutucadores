using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.StartCoroutine(GameManager.Instance.LoadPlayerInfo());
    }

}
