using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Fusion;
using System.Linq;
using System;

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
    //public int killTarget = 10;
    public float safeZoneSize = 90;

    [SerializeField] private float matchDuration = 60f;
    public float matchTimer;
    public bool matchEnded = false;
    public bool matchInitialized = false;

    public AudioSource gameplayMusic;

    public readonly float onBodyHitCameraShakeAmplitude = 20f;
    public readonly float onDrillHitCameraShakeAmplitude = 15f;

    [HideInInspector] public List<NetworkCharacterDrillController> playersControllers = new List<NetworkCharacterDrillController>();
    private void Awake()
    {
        ParticleSystem particleSpawned;
        particleSpawned = Instantiate(onDrillHitParticlePrefab, null);
        onDrillHitParticlePrefab = particleSpawned;

        particleSpawned = Instantiate(onBodyHitParticlePrefab, null);
        onBodyHitParticlePrefab = particleSpawned;

        Instance = this;
        Debug.Log("Awake GameManager");
    }
    private void Start()
    {
        Debug.Log("Started GameManager");
        matchTimer = Time.timeSinceLevelLoad + matchDuration;
        matchInitialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!matchInitialized)
            return;
        if (!matchEnded && Time.timeSinceLevelLoad >= matchTimer)
        {
            matchEnded = true;
            Debug.Log("Match Ended by Time");
            RPC_CheckForEndOfMatch(); // ou seu método para encerrar a partida
        }
    }
    public override void Spawned()
    {
        base.Spawned();
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        virtualCameraNoiseChannel = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        int count = 0;
        foreach (GameObject spawnpoint in GameObject.FindGameObjectsWithTag("Spawnpoint"))
        {
            playerSpawnpoints[count] = spawnpoint.transform;
            count++;
        }
        Debug.Log("Spawned GameManager");

        if (Runner.IsServer)
        {
            matchTimer = matchDuration;
            matchEnded = false;
            matchInitialized = true;
        }
    }
    void IAfterSpawned.AfterSpawned()
    {
        Debug.Log("AfterSpawned GameManager");
    }

    #region Play Audios
    public virtual void PlayDrillHitAudio(Vector3 position)
    {
        int randomAudioID;
        randomAudioID = UnityEngine.Random.Range(0, onHitPlayerAudios.Length);
        while (randomAudioID == lastPlayedAudio) randomAudioID = UnityEngine.Random.Range(0, onHitPlayerAudios.Length);
        onHitAudioSource.transform.position = position;
        onHitAudioSource.PlayOneShot(onHitPlayerAudios[randomAudioID]);
    }
    #endregion

    #region Play Particles
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
    #endregion

    #region Shake Camera
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
    #endregion

    #region Check Dead Players

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_CheckForEndOfMatch()

    {
        if (Runner.ActivePlayers.Count() <= 1)
        {
            RPC_AnnounceWinner(Runner.ActivePlayers.First());
            return;
        }

        /*  if(Runner.ActivePlayers.Count() <= 1)
          {
              DefineWinner(1, Runner.ActivePlayers.First());
              return;
          }
          int playersWithTargetedKills = 0;
          NetworkObject playerAlive = null;
          foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in NetworkBetweenScenesManager.Instance.userIDToPlayerData)
          {
              if (HPBarHandler.Instance.playerRefToPlayerHPBars.TryGetValue(pair.Value.playerRef, out PlayerHPBar player) && Convert.ToInt32(player.kills.text) >= killTarget)
              {
                  Debug.Log($"{pair.Value.username} {pair.Value.playerRef} is elegible to win, searching for their networkObject.");
                  if (Runner.TryGetPlayerObject(pair.Value.playerRef, out playerAlive))
                  {
                      playersWithTargetedKills++;
                      Debug.Log($"Found network object: {playerAlive.Runner.UserId}");
                  }
                  else
                  {
                      Debug.Log($"Could not find network object.");
                  }
              }
          }*/
        var sortedPlayers = HPBarHandler.Instance.playerRefToPlayerHPBars
      .OrderByDescending(pair => Convert.ToInt16(pair.Value.kills))
      .ToList();

        if (sortedPlayers.Count > 0)
        {
            PlayerRef winnerRef = HPBarHandler.Instance.GetPlayerWithMostKills();
            RPC_AnnounceWinner(winnerRef);
        }

        /* Debug.Log($"{playersWithTargetedKills} players with {killTarget} kills or more.");
         //Define winner based on players alive
         if (playersWithTargetedKills <= 1)
         {
             PlayerRef playerRef;
             if (playersWithTargetedKills > 0)
             {
                 playerRef = playerAlive.InputAuthority;
                 Debug.Log($"Winner: {playerAlive.InputAuthority}");
             }
             else
             {
                 playerRef = PlayerRef.None;
                 Debug.Log($"Players eligible: {playersWithTargetedKills}");
             }
             playerAlive.RemoveInputAuthority();
             DefineWinner(playersWithTargetedKills,playerRef);
         }*/

    }
    #endregion

    #region Define Winner
   /* public void DefineWinner(int totalPlayersAlive,PlayerRef playerAlive)
    {
        if (totalPlayersAlive <= 0)
        {
            //If tie define all players as losers
            foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in NetworkBetweenScenesManager.Instance.userIDToPlayerData)
            {
                WinScreenHandler.Instance.RPC_DefineLoser(pair.Value.character);
            }
            WinScreenHandler.Instance.RPC_StartWinScreen("Empate!!!");
        }
        else
        {
            //If there isnt a tie, define the survivor as the winner
            string playerName = "Unfound";
            foreach (KeyValuePair<NetworkString<_256>, PlayerData> pair in NetworkBetweenScenesManager.Instance.userIDToPlayerData)
            {
                if (Runner.GetPlayerUserId(playerAlive) == pair.Key)
                {
                    playerName = pair.Value.username.ToString();
                    WinScreenHandler.Instance.RPC_DefineWinner(pair.Value.character);
                }
                else WinScreenHandler.Instance.RPC_DefineLoser(pair.Value.character);
            }
            WinScreenHandler.Instance.RPC_StartWinScreen(playerName+" Venceu!!!");
        }
    }*/
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_AnnounceWinner(PlayerRef winner)
    {
        string playerName = "Unfound";

        // winner
        foreach (var pair in NetworkBetweenScenesManager.Instance.userIDToPlayerData)
        {
            if (pair.Value.playerRef == winner)
            {
                playerName = pair.Value.username.ToString();
                WinScreenHandler.Instance.RPC_DefineWinner(pair.Value.character);
                break;
            }
        }

        // loser
        foreach (var pair in NetworkBetweenScenesManager.Instance.userIDToPlayerData)
        {
            if (pair.Value.playerRef != winner)
            {
                WinScreenHandler.Instance.RPC_DefineLoser(pair.Value.character);
            }
        }

        WinScreenHandler.Instance.RPC_StartWinScreen($"{playerName}");
        HPBarHandler.Instance.ManageUIToEndGame();
    }

    #endregion
}
