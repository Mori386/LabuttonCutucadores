using Fusion;
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

    //Armazena o wheelVolume(0 a 1) e aciona o a funcao ao ter ser valor mudado pelo NetworkCharacterDrillController
    [Networked(OnChanged = nameof(ChangeWheelAudioSourceVolume)), HideInInspector] public float wheelVolume { get; set; }
    public float defaultAudioSourceVolumeMultiplier;
    static public void ChangeWheelAudioSourceVolume(Changed<AudioPlayerHandler> changed)
    {
        if (changed.Behaviour.wheelAudioSource != null)
        {
            changed.Behaviour.DefineAudioValues();
        }
    }
    public void DefineAudioValues()
    {
        wheelAudioSource.volume = wheelVolume * defaultAudioSourceVolumeMultiplier * 0.5f;

        //Reduz o som do motor inversamente proporcional ao da roda para evitar clutter de audio
        motorAudioSource.volume = (0.5f + 0.5f * (1 - wheelVolume)) * defaultAudioSourceVolumeMultiplier;
    }
    private void Awake()
    {
    }
    public override void Spawned()
    {
        base.Spawned();
        wheelVolume = 0;
        wheelAudioSource.volume = 0;
        motorAudioSource.volume = 1 * defaultAudioSourceVolumeMultiplier;
    }
}
