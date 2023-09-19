using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] sounds; // Um array de aúdio, isso permite utilizar uma boa quantidade de aúdio através de uma única função
    public enum SoundType
    {
        Drill,
        DrillHit,

    }
    //public static AudioManager instance;
    //internal readonly object currentlyPlaying;

    void Awake()
    {
         Instance = this;

        foreach (Sound s in sounds) // Foreach é para percorrer as posições de memória do array. podemos dizer que a Letra S são os indices
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }
    public Sound GetSound(SoundType soundType)
    {
        if (sounds[(int)soundType] != null)
        {
            return sounds[(int)soundType];
        }
        else
        {
            Debug.LogError("Sound file not found, Integer "+ (int)soundType+" doesnt exist");
            return null;
        }
    }
    public void Play(SoundType soundType) // Caso o nome do aúdio ñ seja encontrado, vai dar um super hiper mega bug. Se atentar ao nome pré-definido do Som
    {
        Sound s = GetSound(soundType);
        s.source.Play();
    }

    public void Stop(SoundType soundType)
    {
        Sound s = GetSound(soundType);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }
}