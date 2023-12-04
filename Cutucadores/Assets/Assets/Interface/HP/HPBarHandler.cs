using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterData;

public class HPBarHandler : NetworkBehaviour
{
    public Sprite pfpEscavadeira, pfpMinerador, pfpPaiEFilha, pfpVovo;
    [Space]
    [SerializeField] PlayerHPBar p1HPBar;
    [SerializeField] PlayerHPBar p2HPBar;
    [SerializeField] PlayerHPBar p3HPBar;
    [SerializeField] PlayerHPBar p4HPBar;

    public static HPBarHandler Instance;

    public Dictionary<string, PlayerHPBar> userIDToPlayerHPBars = new Dictionary<string, PlayerHPBar>();
    private void Awake()
    {
        Instance = this;
    }
    public void LoadPlayerInfos()
    {
        NetworkBetweenScenesManager betweenScenesManager = NetworkBetweenScenesManager.Instance;
        int playersPlaced = 1;
        for (int i = 0; i < betweenScenesManager.userIDList.Count; i++)
        {
            if (betweenScenesManager.userIDToPlayerData.TryGet(betweenScenesManager.userIDList[i], out PlayerData playerData))
            {
                if (NetworkBetweenScenesManager.Instance.selfUserID == NetworkBetweenScenesManager.Instance.userIDList[i])
                {
                    //Self
                    p1HPBar.profilePicture.sprite = GetCharacterPfp(playerData.character);
                    p1HPBar.username.text = playerData.username.ToString();
                    userIDToPlayerHPBars.Add(NetworkBetweenScenesManager.Instance.userIDList[i].ToString(), p1HPBar);
                    p1HPBar.hpBar.SetActive(true);
                }
                else
                {
                    PlayerHPBar playerHPBar;
                    switch (playersPlaced)
                    {
                        default:
                        case 1:
                            playerHPBar = p2HPBar;
                            break;
                        case 2:
                            playerHPBar = p3HPBar;
                            break;
                        case 3:
                            playerHPBar = p4HPBar;
                            break;
                    }
                    playerHPBar.profilePicture.sprite = GetCharacterPfp(playerData.character);
                    playerHPBar.username.text = playerData.username.ToString();
                    userIDToPlayerHPBars.Add(NetworkBetweenScenesManager.Instance.userIDList[i].ToString(), playerHPBar);
                    playerHPBar.hpBar.SetActive(true);
                    playersPlaced++;
                }
            }
        }
    }
    public Sprite GetCharacterPfp(Character character)
    {
        switch (character)
        {
            default:
            case Character.Escavador:
                return pfpEscavadeira;
            case Character.Minerador:
                return pfpMinerador;
            case Character.PaiEFilha:
                return pfpPaiEFilha;
            case Character.Vovo:
                return pfpVovo;
        }
    }

}
[Serializable]
public struct PlayerHPBar
{
    public GameObject hpBar;
    [Header("Placement")]
    public RectTransform placementTransform;
    public TextMeshProUGUI placementText;
    [Header("Usarname")]
    public TextMeshProUGUI username;
    [Header("Profile Picture")]
    public Image profilePicture;
    [Header("HP")]
    public Image[] hpSlots;

    public void ChangeHpSlots(int hpAmount)
    {
        for (int i = 0; i < hpSlots.Length; i++)
        {
            if (hpAmount > 0)
            {
                hpSlots[i].gameObject.SetActive(true);
                hpAmount -= 1;
            }
            else hpSlots[i].gameObject.SetActive(false);
        }
    }

    [Header("Death Effect")]
    public Image deathEffect;
}
