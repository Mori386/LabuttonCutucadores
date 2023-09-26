using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEditor.PackageManager;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerMenuCard
{
    public GameObject cardGameObject;
    public enum Visuals
    {
        Orange,
        Green
    }
    public Visuals visualSelected;
    public void ChangeVisual(int changeIndexDifference)
    {
        if ((int)visualSelected + changeIndexDifference > Enum.GetValues(typeof(Visuals)).Cast<int>().Max())
        {
            visualSelected = 0;
        }
        else if ((int)visualSelected + changeIndexDifference < 0)
        {
            visualSelected = (Visuals)Enum.GetValues(typeof(Visuals)).Cast<int>().Max();
        }
        else
        {
            visualSelected += changeIndexDifference;
        }

    }
    public Image visualPreview;

    public TextMeshProUGUI nicknameText;

    [SerializeField] private TextMeshProUGUI ipText;
    public void ChangeIPText(string newIP)
    {
        ipText.text = "IP:" + "\n" + newIP;
    }

}
public class MenuControl : MonoBehaviour
{
    //[SerializeField] private GameObject DefaultMenu, JoinMenu, HostMenu;
    [SerializeField] private TMP_InputField nickname;
    [Header("Client"), SerializeField] private TMP_InputField ServerToJoinIPAdress;

    public PlayerMenuCard[] clientPlayersMenuCards = new PlayerMenuCard[4];

    [Space, SerializeField] private GameObject onServerScreen;
    [SerializeField] private GameObject onJoinScreen;
    //Host Screen
    [Header("Host")] public PlayerMenuCard[] hostPlayersMenuCards = new PlayerMenuCard[4];
    [Space, SerializeField] private TextMeshProUGUI serverIP;
    private bool matchStart;



    Thread ReceiveDataThreadHost;
    public void OnHostMenuEnter()
    {
        Multiplayer.isHost = true;
        serverIP.text = "IP:"+Multiplayer.GetMyIP();
        hostAddPlayersToMenu = StartCoroutine(HostAddPlayersToMenu());
        ReceiveDataThreadHost = new Thread(ReceiveDataHost);
        ReceiveDataThreadHost.Start();
    }
    private Coroutine hostAddPlayersToMenu;
    private IEnumerator HostAddPlayersToMenu()
    {
        //hostPlayersMenuCards[0].ChangeIPText(Multiplayer.GetMyIP());
        hostPlayersMenuCards[0].nicknameText.text = nickname.text;
        hostPlayersMenuCards[0].cardGameObject.SetActive(true);
        int clientCount = Multiplayer.Host.clients.Count;
        while (true)
        {
            Debug.Log(Multiplayer.Host.clients.Count +"|"+ clientCount);
            if (Multiplayer.Host.clients.Count > clientCount)
            {
                int playersLogged = 0;
                for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
                {
                    for (int y = 0; y < Multiplayer.Host.clients.Count; y++)
                    {
                        if (Multiplayer.Host.clients.Values.ElementAt(y).id.Equals(playersLogged))
                        {
                            Player player = Multiplayer.Host.clients.Values.ElementAt(y);
                            Debug.Log(player.name);
                            //hostPlayersMenuCards[playersLogged + 1].ChangeIPText(Multiplayer.Host.clients.Keys.ElementAt(y));
                            hostPlayersMenuCards[playersLogged + 1].nicknameText.text = player.name;
                            hostPlayersMenuCards[playersLogged + 1].cardGameObject.SetActive(true);
                            playersLogged++;
                        }
                    }
                }
                clientCount = Multiplayer.Host.clients.Count;
            }
            else if (Multiplayer.Host.clients.Count < clientCount)
            {
                for (int i = 1; i < hostPlayersMenuCards.Length; i++)
                {
                    hostPlayersMenuCards[i].cardGameObject.SetActive(false);
                }
                int playersLogged = 0;
                for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
                {
                    for (int y = 0; y < Multiplayer.Host.clients.Count; y++)
                    {
                        if (Multiplayer.Host.clients.Values.ElementAt(y).id.Equals(playersLogged))
                        {
                            Player player = Multiplayer.Host.clients.Values.ElementAt(y);
                            Debug.Log(player.name);
                            //hostPlayersMenuCards[playersLogged + 1].ChangeIPText(Multiplayer.Host.clients.Keys.ElementAt(y));
                            hostPlayersMenuCards[playersLogged + 1].nicknameText.text = player.name;
                            hostPlayersMenuCards[playersLogged + 1].cardGameObject.SetActive(true);
                            playersLogged++;
                        }
                    }
                }
                clientCount = Multiplayer.Host.clients.Count;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }
    void ReceiveDataHost()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            string InfoType = "";
            string bonusInfoReceived = "";
            for (int i = 0; i < 5; i++)
            {
                InfoType += returnData[i];
            }
            for (int i = 5; i < returnData.Length; i++)
            {
                bonusInfoReceived += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Enter"))
                {
                    //In this case the bonus info received will be the player nickname
                    if (Multiplayer.Host.clients.Count + 1 < Multiplayer.maxPlayerCount)
                    {
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Cnfrm" + (Multiplayer.Host.clients.Count + 1));
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin0" + nickname.text);
                        for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
                        {
                            Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                            Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin" + player.id + player.name);
                        }
                        Multiplayer.Host.clients.Add(RemoteIpEndPoint.Address.ToString(), new Player(Multiplayer.Host.clients.Count, bonusInfoReceived));
                    }
                    else
                    {
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PCMax");
                    }

                }
                else if (InfoType.Equals("Leave"))
                {
                    if (Multiplayer.Host.clients.TryGetValue(RemoteIpEndPoint.Address.ToString(), out Player playerLeft))
                    {
                        Multiplayer.Host.clients.Remove(RemoteIpEndPoint.Address.ToString());

                        for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
                        {
                            Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PLeft" + playerLeft.id);
                        }
                        for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
                        {
                            if (Multiplayer.Host.clients.Values.ElementAt(i).id > playerLeft.id)
                            {
                                Multiplayer.Host.clients.Values.ElementAt(i).id -= 1;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public void StartMatch()
    {
        Multiplayer.Host.myNickname = nickname.text;
        for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
        {
            Multiplayer.SendMessageToIP(Multiplayer.Host.clients.Keys.ElementAt(i), "Start");
        }
        SceneManager.LoadScene("GameplayMap00");
    }
    public void OnHostMenuLeave()
    {
        Multiplayer.isHost = false;
        ReceiveDataThreadHost.Abort();
        Multiplayer.Host.clients.Clear();
        StopCoroutine(hostAddPlayersToMenu);
        hostAddPlayersToMenu = null;
    }

    public void OnJoinMenuEnter()
    {

    }
    public void JoinSession()
    {
        ReceiveDataThreadJoin = new Thread(ReceiveDataJoin);
        ReceiveDataThreadJoin.Start();
        Multiplayer.SendMessageToIP(ServerToJoinIPAdress.text, "Enter" + nickname.text);
        //onJoinScreen.SetActive(false);
        //onServerScreen.SetActive(true);
        clientAddPlayersToMenu = StartCoroutine(ClientAddPlayersToMenu());
    }
    private Coroutine clientAddPlayersToMenu;
    private IEnumerator ClientAddPlayersToMenu()
    {
        int clientCount = Multiplayer.Client.players.Count(s => s != null);
        while (true)
        {
            if (Multiplayer.Client.players.Length > clientCount)
            {
                int playersLogged = 0;
                for (int i = 0; i < Multiplayer.Client.players.Length; i++)
                {
                    for (int y = 0; y < Multiplayer.Client.players.Length; y++)
                    {
                        if (Multiplayer.Client.players[y] != null && Multiplayer.Client.players[y].id.Equals(playersLogged))
                        {
                            Player player = Multiplayer.Client.players[y];
                            clientPlayersMenuCards[playersLogged].nicknameText.text = player.name;
                            clientPlayersMenuCards[playersLogged].cardGameObject.SetActive(true);
                            playersLogged++;
                        }
                    }
                }
                clientCount = Multiplayer.Client.players.Count(s => s != null);
            }
            else if (Multiplayer.Client.players.Length < clientCount)
            {
                for (int i = 0; i < clientPlayersMenuCards.Length; i++)
                {
                    clientPlayersMenuCards[i].cardGameObject.SetActive(false);
                }
                int playersLogged = 0;
                for (int i = 0; i < Multiplayer.Client.players.Length; i++)
                {
                    for (int y = 0; y < Multiplayer.Client.players.Length; y++)
                    {
                        if (Multiplayer.Client.players[y] != null && Multiplayer.Client.players[y].id.Equals(playersLogged))
                        {
                            Player player = Multiplayer.Client.players[y];
                            clientPlayersMenuCards[playersLogged].nicknameText.text = player.name;
                            clientPlayersMenuCards[playersLogged].cardGameObject.SetActive(true);
                            playersLogged++;
                        }
                    }
                }
                clientCount = Multiplayer.Client.players.Count(s => s != null);
            }
            if (matchStart) SceneManager.LoadScene("GameplayMap00");
            yield return new WaitForSeconds(0.25f);
        }
    }

    Thread ReceiveDataThreadJoin;
    void ReceiveDataJoin()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            string InfoType = "";
            for (int i = 0; i < 5; i++)
            {
                InfoType += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Cnfrm"))
                {
                    int playerID = int.Parse(returnData[5].ToString());
                    Multiplayer.Client.players[playerID] = new Player(playerID, nickname.text);
                    Multiplayer.Client.myID = playerID;
                    Multiplayer.Client.HostIP = RemoteIpEndPoint.Address.ToString();
                }
                else if (InfoType.Equals("PCMax"))
                {
                    onJoinScreen.SetActive(true);
                    onServerScreen.SetActive(false);
                    StopCoroutine(clientAddPlayersToMenu);
                    clientAddPlayersToMenu = null; ;
                    ReceiveDataThreadJoin = null;
                    Multiplayer.Client.HostIP = null;
                    Multiplayer.Client.myID = 0;
                    Multiplayer.Client.players = new Player[Multiplayer.maxPlayerCount];
                    continue;
                }
                else if (InfoType.Equals("PJoin"))
                {
                    int playerID = int.Parse(returnData[5].ToString());
                    string pName = "";
                    for (int i = 6; i < returnData.Length; i++)
                    {
                        pName += returnData[i];
                    }
                    Multiplayer.Client.players[playerID] = new Player(playerID, pName);
                }
                else if (InfoType.Equals("PLeft"))
                {
                    int playerID = int.Parse(returnData[5].ToString());
                    Multiplayer.Client.players[playerID] = null;
                    for (int i = playerID + 1; i < Multiplayer.Client.players.Length; i++)
                    {
                        Multiplayer.Client.players[i - 1] = Multiplayer.Client.players[i];
                        Multiplayer.Client.players[i - 1].id = Multiplayer.Client.players[i].id - 1;
                        Multiplayer.Client.players[i] = null;
                    }
                }
                else if (InfoType.Equals("Start"))
                {
                    matchStart = true;
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public void LeaveSession()
    {
        onJoinScreen.SetActive(true);
        onServerScreen.SetActive(false);
        ReceiveDataThreadJoin.Abort();
        ReceiveDataThreadJoin = null;
        StopCoroutine(clientAddPlayersToMenu);
        clientAddPlayersToMenu = null;
        Multiplayer.SendMessageToIP(ServerToJoinIPAdress.text, "Leave");
        Multiplayer.Client.HostIP = null;
        Multiplayer.Client.myID = 0;
        Multiplayer.Client.players = new Player[Multiplayer.maxPlayerCount];
    }
    public void OnJoinMenuLeave()
    {

    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
