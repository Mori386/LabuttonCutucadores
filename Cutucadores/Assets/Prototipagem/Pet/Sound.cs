using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Enfia tudo oq está abaixo no serialized
public class Sound // - É daqui que vem a letra S do foreach 
{
    public string name;

    public AudioClip clip;

    [Range(0f, 5f)] // Alcance minimo e maximo do volume e do tom
    public float volume;

    [Range(.1f, 3f)]
    public float pitch; // Tom

    public bool loop;




    [HideInInspector] // é publico mas não deve aparecer 
    public AudioSource source; // vamos chamar o Som em outros codigos através da vairiavel Source
}