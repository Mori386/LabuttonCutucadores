using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterData;

public class HPBarHandler : MonoBehaviour
{
    public Sprite pfpEscavadeira, pfpMinerador, pfpPaiEFilha, pfpVovo;
    [Space]
    [SerializeField] PlayerHPBar p1HPBar;
    [SerializeField] PlayerHPBar p2HPBar;
    [SerializeField] PlayerHPBar p3HPBar;
    [SerializeField] PlayerHPBar p4HPBar;

    [SerializeField] private RectTransform rankingBackground;

    public static HPBarHandler Instance;

    public Dictionary<PlayerRef, PlayerHPBar> playerRefToPlayerHPBars = new Dictionary<PlayerRef, PlayerHPBar>();
    public bool loaded;
    [SerializeField] private CanvasGroup canvasGroup;
    private void Awake()
    {
        Instance = this;
    }
    public void LoadPlayerInfos()
    {
        if (!loaded)
        {
            loaded = true;
            NetworkBetweenScenesManager betweenScenesManager = NetworkBetweenScenesManager.Instance;
            int playersPlaced = 1;
            for (int i = 0; i < betweenScenesManager.userIDList.Count; i++)
            {
                if (betweenScenesManager.userIDToPlayerData.TryGet(betweenScenesManager.userIDList[i], out PlayerData playerData))
                {
                    if (betweenScenesManager.selfUserID == betweenScenesManager.userIDList[i])
                    {
                        //Self
                        p1HPBar.profilePicture.sprite = GetCharacterPfp(playerData.character);
                        p1HPBar.username.text = playerData.username.ToString();
                        playerRefToPlayerHPBars.Add(playerData.playerRef, p1HPBar);
                        p1HPBar.hpBar.SetActive(true);
                        StartCoroutine(ActivateWithDelay(canvasGroup, 4f));
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
                        playerRefToPlayerHPBars.Add(playerData.playerRef, playerHPBar);
                        playerHPBar.hpBar.SetActive(true);
                        playersPlaced++;
                        StartCoroutine(ActivateWithDelay(canvasGroup, 4f));
                    }
                }
            }
           // rankingBackground.sizeDelta = new (358, (float)(40.874 + (playersPlaced * 100)));
        }
    }

    public void UpdateScore(PlayerRef playerRef,int newScore)
    {
        if(playerRefToPlayerHPBars.TryGetValue(playerRef, out PlayerHPBar playerHPBar))
        {
            playerHPBar.ChangeScore(newScore);
            SortRanking();
            if (newScore >= GameManager.Instance.killTarget)
            {
                GameManager.Instance.RPC_CheckForEndOfMatch();
            }
        }
    }

    private void SortRanking()
    {
        var sortedDisctionary = playerRefToPlayerHPBars.OrderBy(pair => Convert.ToInt16(pair.Value.kills)).ToDictionary(pair => pair.Key, pair => pair.Value);
        int count = 0;
        foreach (KeyValuePair<PlayerRef, PlayerHPBar> pair in sortedDisctionary)
        {
            pair.Value.hpBar.transform.SetSiblingIndex(count);
            pair.Value.position.text = $"{count++}.";
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

    public void ManageUIToEndGame()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0.0f;
    }

    private IEnumerator ActivateWithDelay(CanvasGroup obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.alpha = 1f;
    }


}
[Serializable]
public struct PlayerHPBar
{
    public GameObject hpBar;
    [Header("Position")]
    public TextMeshProUGUI position;
    [Header("Username")]
    public TextMeshProUGUI username;
    [Header("Profile Picture")]
    public Image profilePicture;
    [Header("Kills")]
    public TextMeshProUGUI kills;

    public void ChangeScore(int newScore)
    {
        kills.text = newScore.ToString();
    }
}
