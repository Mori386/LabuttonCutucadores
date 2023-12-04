using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Fusion;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour, IAfterSpawned
{
    public static GameManager Instance { get; private set; }
    public GameObject playerPrefab;
    public CinemachineVirtualCamera virtualCamera;
    [HideInInspector] public CinemachineBasicMultiChannelPerlin virtualCameraNoiseChannel;
    public Transform[] playerSpawnpoints;
    public ParticleSystem onDrillHitParticlePrefab, onBodyHitParticlePrefab;
    [Space]
    public AudioSource onHitAudioSource;
    public AudioClip[] onHitPlayerAudios;
    private int lastPlayedAudio = -1;

    public AudioSource gameplayMusic;

    public readonly float onBodyHitCameraShakeAmplitude = 20f;
    public readonly float onDrillHitCameraShakeAmplitude = 15f;
    private void Awake()
    {
        ParticleSystem particleSpawned;
        particleSpawned = Instantiate(onDrillHitParticlePrefab, null);
        onDrillHitParticlePrefab = particleSpawned;
        particleSpawned = Instantiate(onBodyHitParticlePrefab, null);
        onBodyHitParticlePrefab = particleSpawned;

        Instance = this;
    }
    private void Start()
    {
        virtualCameraNoiseChannel = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    public override void Spawned()
    {
        base.Spawned();
        Debug.Log("Spawned");
    }
    void IAfterSpawned.AfterSpawned()
    {
        
    }
    public void LoadPlayerInfo()
    {
        if (NetworkBetweenScenesManager.Instance.userIDToPlayerData.TryGet(NetworkBetweenScenesManager.Instance.selfUserID, out PlayerData myPlayerData))
        {
            NetworkBetweenScenesManager.Instance.RPC_SetPlayerLoaded(NetworkBetweenScenesManager.Instance.selfUserID);
            NetworkBetweenScenesManager.Instance.RPC_CheckForPlayerLoaded();
            Debug.Log(NetworkBetweenScenesManager.Instance.userIDToPlayerData.Get(NetworkBetweenScenesManager.Instance.selfUserID).username+" "+
                NetworkBetweenScenesManager.Instance.userIDToPlayerData.Get(NetworkBetweenScenesManager.Instance.selfUserID).loaded);
        }

    }
    public virtual void PlayDrillHitAudio(Vector3 position)
    {
        int randomAudioID;
        randomAudioID = Random.Range(0, onHitPlayerAudios.Length);
        while (randomAudioID == lastPlayedAudio) randomAudioID = Random.Range(0, onHitPlayerAudios.Length);
        onHitAudioSource.transform.position = position;
        onHitAudioSource.PlayOneShot(onHitPlayerAudios[randomAudioID]);
    }
    public void PlayOnBodyHitParticle(Vector3 position)
    {
        PlayDrillHitAudio(position);
        onBodyHitParticlePrefab.transform.position = position;
        onBodyHitParticlePrefab.Play();
    }
    public void PlayOnDrillHitParticle(Vector3 position)
    {
        PlayDrillHitAudio(position);
        onDrillHitParticlePrefab.transform.position = position;
        onDrillHitParticlePrefab.Play();
    }
    public void ShakeCamera(float amplitude)
    {
        if (shakeCameraCoroutine != null)
        {
            StopCoroutine(shakeCameraCoroutine);
        }
        shakeCameraCoroutine = StartCoroutine(ShakeCameraTimer(0.25f, amplitude));
    }
    public Coroutine shakeCameraCoroutine;
    public IEnumerator ShakeCameraTimer(float duration, float amplitude)
    {
        float timer = 0f;
        while (timer < duration)
        {
            virtualCameraNoiseChannel.m_AmplitudeGain = Mathf.Lerp(amplitude, 0, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        virtualCameraNoiseChannel.m_AmplitudeGain = 0;
        shakeCameraCoroutine = null;
    }

    [HideInInspector] public List<NetworkCharacterDrillController> playersControllers = new List<NetworkCharacterDrillController>();


    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_CheckForPlayersDead()
    {
        int totalPlayersAlive = 0;
        NetworkCharacterDrillController playerAlive = null;
        for (int i = 0; i < playersControllers.Count; i++)
        {
            if (!playersControllers[i].hpHandler.isDead)
            {
                totalPlayersAlive++;
                playerAlive= playersControllers[i];
            }
        }
        if (totalPlayersAlive <= 1)
        {
            PlayerRef playerRef;
            if (totalPlayersAlive > 0) playerRef = playerAlive.Object.InputAuthority;
            else playerRef = PlayerRef.None;
            Debug.Log(Runner.GetPlayerUserId(playerRef));
            playerAlive.Object.RemoveInputAuthority();
            DefineWinner(totalPlayersAlive,playerRef);
            Debug.Log("Alguem ganhou");
        }
    }
    public void DefineWinner(int totalPlayersAlive,PlayerRef playerAlive)
    {
        if (totalPlayersAlive <= 0)
        {
            for(int i = 0;i < NetworkBetweenScenesManager.Instance.userIDList.Count;i++)
            {
                if (NetworkBetweenScenesManager.Instance.userIDToPlayerData.TryGet(NetworkBetweenScenesManager.Instance.userIDList[i],out PlayerData playerData))
                {
                    WinScreenHandler.Instance.RPC_DefineLoser(playerData.character);
                }
            }
            WinScreenHandler.Instance.RPC_StartWinScreen("Empate!!!");
        }
        else
        {
            string playerName = "Unfound";
            for (int i = 0; i < NetworkBetweenScenesManager.Instance.userIDList.Count; i++)
            {
                if (NetworkBetweenScenesManager.Instance.userIDToPlayerData.TryGet(NetworkBetweenScenesManager.Instance.userIDList[i], out PlayerData playerData))
                {
                    if(Runner.GetPlayerUserId(playerAlive) == NetworkBetweenScenesManager.Instance.userIDList[i])
                    {
                        playerName = playerData.username.ToString();
                        WinScreenHandler.Instance.RPC_DefineWinner(playerData.character);
                    }
                    else WinScreenHandler.Instance.RPC_DefineLoser(playerData.character);
                }
            }
            WinScreenHandler.Instance.RPC_StartWinScreen(playerName+" Venceu!!!");
        }
    }
}
