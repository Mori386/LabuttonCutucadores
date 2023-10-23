using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }
    public CinemachineVirtualCamera VirtualCamera;
    public Transform[] playerSpawnpoints;
    private void Awake()
    {
        Instance = this;
        
    }
}
