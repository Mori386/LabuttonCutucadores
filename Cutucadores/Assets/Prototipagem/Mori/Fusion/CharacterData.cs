using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterNameData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    [Header("------Character------")]
    public string characterName;
    public enum Character
    {
        Char0 = 0,
        Char1 = 1,
        Char2 = 2,
        Char3 = 3,
    }
    public Character character;

    [Header("------Stats------")]
    public InGameCharacterData characterData;

    [Header("------Model Info------"), Space(10)]
    public GameObject visualPrefab; 
}
[Serializable]
public class InGameCharacterData
{
    [Range(0,100)]public float weight = 20;
    public float acceleration = 10.0f;
    public float braking = 1.0f;
    public float maxSpeed = 50.0f;
    public float rotationSpeed = 15.0f;
}
