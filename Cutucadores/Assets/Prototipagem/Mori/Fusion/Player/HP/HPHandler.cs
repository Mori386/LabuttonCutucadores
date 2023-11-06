using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool isDead { get; set; }

    bool isInitialized = false;

    const byte startingHP = 3;

    public TextMeshPro hpText;

    private void Start()
    {
        HP = startingHP;
        UpdateHpUI();
        isDead = false;
    }

    public void OnTakeDamage(byte damageAmount)
    {
        if(isDead) return;
        HP -= damageAmount;
        if (HP<=0)
        {
            isDead = true;
        }
    }
    public void UpdateHpUI()
    {
        hpText.text = HP.ToString();
    }
    static void OnHPChanged(Changed<HPHandler> changed)
    {
        changed.Behaviour.UpdateHpUI();
        Debug.Log(changed.Behaviour.HP);
    }
    static void OnStateChanged(Changed<HPHandler> changed)
    {

    }    
}
