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


public class MenuControl : MonoBehaviour
{
    [SerializeField] private GameObject DefaultMenu, JoinMenu, HostMenu;
    [SerializeField] private TMP_InputField nickname;
    [Header("Client"), SerializeField] private TMP_InputField ServerToJoinIPAdress;
    [SerializeField] private TextMeshProUGUI clientPlayersInSession;
    [Space, SerializeField] private GameObject onServerScreen;
    [SerializeField] private GameObject onJoinScreen;
    [Header("Host"), SerializeField] private TextMeshProUGUI hostPlayersInSession;
    [SerializeField] private TextMeshProUGUI serverIP;
    //Host Screen

    private bool matchStart;

    Thread ReceiveDataThreadHost;
    public void OnHostMenuEnter()
    {
        Multiplayer.isHost = true;
        serverIP.text = Multiplayer.GetMyIP();
        hostAddPlayersToMenu = StartCoroutine(HostAddPlayersToMenu());
        ReceiveDataThreadHost = new Thread(ReceiveDataHost);
        ReceiveDataThreadHost.Start();
    }
    private Coroutine hostAddPlayersToMenu;
    private IEnumerator HostAddPlayersToMenu()
    {
        while (true)
        {
            string playersConnectedList = Multiplayer.GetMyIP() + ": " + nickname.text + "\n";
            int playersLogged = 0;

            for (int i = 0; i < Multiplayer.clients.Count; i++)
            {
                for (int y = 0; y < Multiplayer.clients.Count; y++)
                {
                    if (Multiplayer.clients.Values.ElementAt(y).Equals(playersLogged))
                    {
                        playersConnectedList += Multiplayer.clients.Keys.ElementAt(y) + ": " + Multiplayer.clients.Values.ElementAt(y).name + "\n";
                        playersLogged++;
                    }
                }
            }
            hostPlayersInSession.text = playersConnectedList;
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
            string nicknameReceived = "";
            for (int i = 0; i < 5; i++)
            {
                InfoType += returnData[i];
            }
            for (int i = 5; i < returnData.Length; i++)
            {
                nicknameReceived += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Enter"))
                {
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Cnfrm" + Multiplayer.clients.Count);
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin0" + nickname.text);
                    for (int i = 0; i < Multiplayer.clients.Count; i++)
                    {
                        Player player = Multiplayer.clients.Values.ElementAt(i);
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin" + player.id + player.name);
                    }
                    Multiplayer.clients.Add(RemoteIpEndPoint.Address.ToString(), new Player(Multiplayer.clients.Count+ 1, nicknameReceived));

                }
                else if (InfoType.Equals("Leave"))
                {
                    Player playerLeft = Multiplayer.clients[RemoteIpEndPoint.Address.ToString()];
                    for (int i = 0; i < Multiplayer.clients.Count; i++)
                    {
                         Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PLeft" + playerLeft.id);
                    }
                    for(int i =0;i< Multiplayer.clients.Count;i++)
                    {
                        if(Multiplayer.clients.Values.ElementAt(i).id> playerLeft.id)
                        {
                            Multiplayer.clients.Values.ElementAt(i).id -= 1;
                        }
                    }
                    Multiplayer.clients.Remove(RemoteIpEndPoint.Address.ToString());
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
        for (int i = 0; i < Multiplayer.clients.Count; i++)
        {
            Multiplayer.SendMessageToIP(Multiplayer.clients.Keys.ElementAt(i), "Start");
        }
        SceneManager.LoadScene("MoriGameplayTest");
    }
    public void OnHostMenuLeave()
    {
        Multiplayer.isHost = false;
        ReceiveDataThreadHost.Abort();
        Multiplayer.clients.Clear();
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
        onJoinScreen.SetActive(false);
        onServerScreen.SetActive(true);
        clientAddPlayersToMenu = StartCoroutine(ClientAddPlayersToMenu());
        checkForGameStart = StartCoroutine(CheckForGameStart());
    }
    private Coroutine clientAddPlayersToMenu;
    private IEnumerator ClientAddPlayersToMenu()
    {
        while (true)
        {
            string playersConnectedList = "";
            int playersLogged = 0;
            for (int i = 0; i < Multiplayer.clients.Count; i++)
            {
                for (int y = 0; y < Multiplayer.clients.Count; y++)
                {
                    if (Multiplayer.clients.Values.ElementAt(y).Equals(playersLogged))
                    {
                        playersConnectedList += Multiplayer.clients.Keys.ElementAt(y) + ": " + Multiplayer.clients.Values.ElementAt(y).name + "\n";
                        playersLogged++;
                    }
                    else if(Multiplayer.selfPlayer.id.Equals(playersLogged))
                    {
                        playersConnectedList += Multiplayer.GetMyIP() + ": " + Multiplayer.selfPlayer.name + "\n";
                        playersLogged++;
                    }
                }
            }
            clientPlayersInSession.text = playersConnectedList;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Coroutine checkForGameStart;
    private IEnumerator CheckForGameStart()
    {
        while (true)
        {
            if(matchStart) SceneManager.LoadScene("MoriGameplayTest");
            yield return new WaitForFixedUpdate();
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
                    Multiplayer.selfPlayer = new Player(int.Parse(returnData[5].ToString()),nickname.text);
                }
                else if (InfoType.Equals("PJoin"))
                {
                    string pName = "";
                    for (int i = 6; i < returnData.Length; i++)
                    {
                        pName += returnData[i];
                    }
                    Multiplayer.clients.Add(,new Player(int.Parse(returnData[5].ToString()), pName));
                    //Check se esta com o maximo de players
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
        Multiplayer.SendMessageToIP(ServerToJoinIPAdress.text, "Leave");
    }
    public void OnJoinMenuLeave()
    {

    }
}
