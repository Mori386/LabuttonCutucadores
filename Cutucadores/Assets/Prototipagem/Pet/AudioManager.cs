using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds; // Um array de aúdio, isso permite utilizar uma boa quantidade de aúdio através de uma única função

    //public static AudioManager instance;
    //internal readonly object currentlyPlaying;

    void Awake()
    {
        //Caso haja bug de duplicidade de audiomanager, descomentar as linhas abaixo.

        /* if (instance != null)
         {
             Destroy(gameObject);
             return;
         }

         instance = this;

         DontDestroyOnLoad(gameObject);*/

        foreach (Sound s in sounds) // Foreach é para percorrer as posições de memória do array. podemos dizer que a Letra S são os indices
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;


        }
    }

    public void Play(string name) // Caso o nome do aúdio ñ seja encontrado, vai dar um super hiper mega bug. Se atentar ao nome pré-definido do Som
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }
}