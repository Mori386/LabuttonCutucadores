using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleHandler : NetworkBehaviour
{
    public AudioSource fallAudioSource;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent<NetworkCharacterDrillController>(out NetworkCharacterDrillController drillController))
            {
                drillController.FallInHole(transform.position);
                PlayFallAudio();
            }
        }
    }
    public virtual void PlayFallAudio()
    {
        fallAudioSource.Play();
    }
}
