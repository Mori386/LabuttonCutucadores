using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerQuantityText;
    [SerializeField] private GameObject privateRoomSign;
    private Dictionary<string, SessionProperty> roomProperties = new();
    [SerializeField] private Button joinButton;

    private const string isRoomPrivate = "Private"; // 1 = true; 0 = false;
    private const string password = "Password";

    public void Setup(string roomName, int playerCount, Dictionary<string, SessionProperty> properties)
    {
        roomNameText.text = roomName;
        playerQuantityText.text = $"{playerCount}/4";
        roomProperties = properties;
        privateRoomSign.SetActive(roomProperties[isRoomPrivate] == 1);
        if (playerCount >= 4)
        {
            playerQuantityText.color = Color.red;
            joinButton.gameObject.SetActive(false);
        }
        else
        {
            joinButton.onClick.AddListener(JoinRoom);
        }
    }

    public void JoinRoom()
    {
        if (roomProperties[isRoomPrivate] == 1)
        {
            CursorController.Instance.ToInsertPasswordScreen(roomProperties[password], roomNameText.text);
        }
        else
        {
            CursorController.Instance.StartClient(roomNameText.text, roomProperties);
        }
    }
}
