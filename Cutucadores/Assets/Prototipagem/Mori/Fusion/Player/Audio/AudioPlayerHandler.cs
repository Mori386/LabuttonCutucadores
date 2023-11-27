using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerHandler : NetworkBehaviour
{
    public AudioSource motorAudioSource;
    public AudioSource wheelAudioSource;
    public void ToggleAudioSources(bool state)
    {
        motorAudioSource.enabled = state;
        wheelAudioSource.enabled = state;
    }

    [Networked(OnChanged = nameof(ChangeWheelAudioSourceVolume)), HideInInspector] public float wheelVolume { get; set; }
    [HideInInspector]public float defaultVolumeMultiplier;
    static public void ChangeWheelAudioSourceVolume(Changed<AudioPlayerHandler> changed)
    {
        if (changed.Behaviour.wheelAudioSource != null) changed.Behaviour.wheelAudioSource.volume = changed.Behaviour.wheelVolume* changed.Behaviour.defaultVolumeMultiplier;
    }

    private void Awake()
    {
        defaultVolumeMultiplier = wheelAudioSource.volume;
    }
    public override void Spawned()
    {
        base.Spawned();
        defaultVolumeMultiplier = wheelAudioSource.volume;
        wheelVolume = 0;
        wheelAudioSource.volume = 0;
    }
}
