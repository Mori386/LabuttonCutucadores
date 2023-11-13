using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ExplosionHandler : NetworkBehaviour
{
    public static ExplosionHandler Instance;
    private void Awake()
    {
        Instance = this;
        lineRenderer.positionCount = pointsCount + 1;
    }
    [Header("-----Blast Wave-----")]
    public int pointsCount;
    public float maxRadius;
    public float speed;
    public float startWidth;
    [SerializeField]private LineRenderer lineRenderer;
    private void PlayBlastWave()
    {
        StartCoroutine(BlastWave());
    }
    private IEnumerator BlastWave()
    {
        float currentRadius = 0;
        while(currentRadius < maxRadius)
        {
            currentRadius += Time.deltaTime * speed;
            Draw(currentRadius);
            yield return null;
        }
    }
    private void Draw(float radius)
    {
        float angleBetweenPoints = 360f / pointsCount;
        for(int i =0; i<=pointsCount; i++)
        {
            float angle = i * angleBetweenPoints * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle),Mathf.Cos(angle),0);
            Vector3 position = direction * radius;
            lineRenderer.SetPosition(i, position);
        }
        lineRenderer.widthMultiplier = Mathf.Lerp(0, startWidth, 1- radius/maxRadius);
    }
    [Header("-----Blast Particle-----")]
    [SerializeField]private ParticleSystem[] blastParticleSystems;

    private void PlayParticle()
    {
        for(int i=0; i<blastParticleSystems.Length; i++)
        {
            blastParticleSystems[i].Play();
        }
    }
    public void Explode()
    {
        GameManager.Instance.ShakeCamera(GameManager.Instance.onBodyHitCameraShakeAmplitude * 2f);
        PlayParticle();
        PlayBlastWave();
    }

}
