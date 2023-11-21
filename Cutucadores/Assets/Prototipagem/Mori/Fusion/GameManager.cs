using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }
    public CinemachineVirtualCamera virtualCamera;
    [HideInInspector]public CinemachineBasicMultiChannelPerlin virtualCameraNoiseChannel;
    public Transform[] playerSpawnpoints;
    public ParticleSystem onDrillHitParticlePrefab,onBodyHitParticlePrefab;

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
    public void PlayOnBodyHitParticle(Vector3 position)
    {
        onBodyHitParticlePrefab.transform.position = position;
        onBodyHitParticlePrefab.Play();
    }
    public void PlayOnDrillHitParticle(Vector3 position)
    {
        onDrillHitParticlePrefab.transform.position = position;
        onDrillHitParticlePrefab.Play();
    }
    public void ShakeCamera(float amplitude)
    {
        if(shakeCameraCoroutine != null)
        {
            StopCoroutine(shakeCameraCoroutine);
        }
        shakeCameraCoroutine = StartCoroutine(ShakeCameraTimer(0.25f, amplitude));
    }
    public Coroutine shakeCameraCoroutine;
    public IEnumerator ShakeCameraTimer(float duration,float amplitude)
    {
        float timer = 0f;
        while (timer<duration)
        {
            virtualCameraNoiseChannel.m_AmplitudeGain = Mathf.Lerp(amplitude, 0, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        virtualCameraNoiseChannel.m_AmplitudeGain = 0;
        shakeCameraCoroutine = null;
    }

    [HideInInspector]public List<NetworkCharacterDrillController> playersControllers = new List<NetworkCharacterDrillController>();
    public void CheckIfThereIsWinner()
    {
        int totalPlayersAlive = 0;
        for(int i = 0; i < playersControllers.Count; i++)
        {
            if(!playersControllers[i].hpHandler.isDead) totalPlayersAlive++;
        }
        if(totalPlayersAlive <=1)
        {
            Debug.Log("Alguem ganhou");
        }
    }
}
