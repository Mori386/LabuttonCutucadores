using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] sounds; // Um array de a�dio, isso permite utilizar uma boa quantidade de a�dio atrav�s de uma �nica fun��o
    public enum SoundType
    {
        Repulsion, // Quando broca atinge o alvo

        DrillOn, // barulho de motor
        DrillOff, // broca desligando
        Drill, // broca 

        //OBS para a broca:

        /*if (moveInput > 0 || moveInput < 0)
       {
         AudioManager.Instance.Play(AudioManager.SoundType.DrillOn);
         AudioManager.Instance.Stop(AudioManager.SoundType.DrillOff);
         AudioManager.Instance.Stop(AudioManager.SoundType.Drill);
       }
        else
       {
         AudioManager.Instance.Stop(AudioManager.SoundType.DrillOn);
         AudioManager.Instance.Play(AudioManager.SoundType.DrillOff);
         AudioManager.Instance.Play(AudioManager.SoundType.Drill);

       }*/

        button, // bot�o padr�o
        Start, // Iniciar instancia
        Energy, //ganho de energia e/ou boost
        FallHole, // caiu no buraco

}
//public static AudioManager instance;
//internal readonly object currentlyPlaying;

void Awake()
{
 Instance = this;

foreach (Sound s in sounds) // Foreach � para percorrer as posi��es de mem�ria do array. podemos dizer que a Letra S s�o os indices
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
public void Play(SoundType soundType) // Caso o nome do a�dio � seja encontrado, vai dar um super hiper mega bug. Se atentar ao nome pr�-definido do Som
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