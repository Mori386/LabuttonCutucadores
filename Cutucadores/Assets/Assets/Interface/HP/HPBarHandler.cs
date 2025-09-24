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
    [SerializeField] private TMPro.TextMeshProUGUI matchDurationText;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance.matchEnded) return;

        float currentTime = GameManager.Instance.matchTimer - Time.timeSinceLevelLoad; // ou algum valor sincronizado
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        matchDurationText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
                        SortRanking();
                    }
                }
            }
           // rankingBackground.sizeDelta = new (358, (float)(40.874 + (playersPlaced * 100)));
        }
    }

    public void UpdateScore(PlayerRef playerRef, int newScore)
    {
        if (playerRefToPlayerHPBars.TryGetValue(playerRef, out PlayerHPBar playerHPBar))
        {
            playerHPBar.ChangeScore(newScore, this);
            SortRanking();
        }
    }

    private void SortRanking()
    {


        /*var sortedDisctionary = playerRefToPlayerHPBars.OrderBy(pair => Convert.ToInt16(pair.Value.kills)).ToDictionary(pair => pair.Key, pair => pair.Value);
        int count = 0;
        foreach (KeyValuePair<PlayerRef, PlayerHPBar> pair in sortedDisctionary)
        {
            pair.Value.hpBar.transform.SetSiblingIndex(count);
            pair.Value.position.text = $"{count++}.";
        }*/

        var sortedDictionary = playerRefToPlayerHPBars
              .OrderByDescending(pair => Convert.ToInt16(pair.Value.kills.text))
              .ToDictionary(pair => pair.Key, pair => pair.Value);

        int count = 0;
        foreach (KeyValuePair<PlayerRef, PlayerHPBar> pair in sortedDictionary)
        {
            var bar = pair.Value;
            bar.hpBar.transform.SetSiblingIndex(count);
            bar.position.text = $"{count + 1}.";

            if (count == 0) // primeiro colocado
            {
                bar.hpBar.transform.localScale = Vector3.one * 1.2f;

                // sprite, usar outra coisa get component nao
                RectTransform avatarRect = bar.profilePicture.GetComponent<RectTransform>();
                if (avatarRect != null)
                {
                    avatarRect.sizeDelta = new Vector2(120, 120);
                }

                // Username
                bar.username.fontSize = 20;
                bar.username.color = Color.white;

                // Kills
                bar.kills.fontSize = 60;
                bar.kills.color = Color.white;

                // Posicao
                bar.position.fontSize = 30;
                bar.position.color = Color.white;

            }
            else // Demais jogadores
            {
                // Reduz toda a barra
                bar.hpBar.transform.localScale = Vector3.one * 0.9f;

                RectTransform avatarRect = bar.profilePicture.GetComponent<RectTransform>();
                if (avatarRect != null)
                    avatarRect.sizeDelta = new Vector2(50, 50);

                bar.username.fontSize = 20;
                bar.username.fontSize = 20;
                bar.username.color = Color.gray;

                bar.kills.fontSize = 45;
                bar.kills.color = Color.gray;

                bar.position.fontSize = 20;
                bar.position.color = Color.white;

            }

            count++;
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
        canvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator ActivateWithDelay(CanvasGroup obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.alpha = 1f;
    }

    public PlayerRef GetPlayerWithMostKills()
    {
        return playerRefToPlayerHPBars
            .OrderByDescending(pair => int.Parse(pair.Value.kills.text))
            .First().Key;
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

    public void ChangeScore(int newScore, MonoBehaviour runner)
    {
        kills.text = newScore.ToString();

        if (runner != null)
            runner.StartCoroutine(AnimateProfilePicture());
    }

    private IEnumerator AnimateProfilePicture()
    {
        if (profilePicture == null) yield break;

        RectTransform rect = profilePicture.rectTransform;
        Vector3 originalScale = Vector3.one; 
        Quaternion originalRot = Quaternion.identity;

    
        for (int i = 0; i < 3; i++)
        {
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float scale = 1f + Mathf.Sin(t * Mathf.PI *3f) * 0.3f; // pulso
                float angle = Mathf.Sin(t * Mathf.PI * 4f) * 15f;       // chacoalhada
                rect.localScale = originalScale * scale;
                rect.localRotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }
        }


        rect.localScale = originalScale;
        rect.localRotation = originalRot;
    }
}

