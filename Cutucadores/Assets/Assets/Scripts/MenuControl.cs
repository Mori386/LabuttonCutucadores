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

            while (playersLogged < Multiplayer.clientsIndex.Count)
            {
                for (int i = 0; i < Multiplayer.clientsIndex.Count; i++)
                {
                    if (Multiplayer.clientsIndex.Values.ElementAt(i).Equals(playersLogged))
                    {
                        playersConnectedList += Multiplayer.clientsName.Keys.ElementAt(i) + ": " + Multiplayer.clientsName.Values.ElementAt(i) + "\n";
                        playersLogged++;
                    }
                }
                yield return new WaitForFixedUpdate();
            }
            hostPlayersInSession.text = playersConnectedList;
            yield return new WaitForSeconds(0.5f);
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
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Cnfrm" + Multiplayer.clientsIndex.Count);
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin0" + nickname.text);
                    for (int i = 0; i < Multiplayer.clientsIndex.Count; i++)
                    {
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PJoin" + (i + 1).ToString() + Multiplayer.clientsName.Keys.ElementAt(i) + "|" + Multiplayer.clientsName.Values.ElementAt(i));
                    }
                    Multiplayer.clientsName.Add(RemoteIpEndPoint.Address.ToString(), nicknameReceived);
                    Multiplayer.clientsIndex.Add(RemoteIpEndPoint.Address.ToString(), Multiplayer.clientsName.Keys.ToList().IndexOf(RemoteIpEndPoint.Address.ToString()));
                }
                else if (InfoType.Equals("Leave"))
                {
                    for (int i = 0; i < Multiplayer.clientsIndex.Count; i++)
                    {
                        if (Multiplayer.clientsIndex.Keys.ElementAt(i) != RemoteIpEndPoint.Address.ToString()) Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "PLeft" + Multiplayer.clientsName.Keys.ToList().IndexOf(RemoteIpEndPoint.Address.ToString()));
                    }

                    Multiplayer.clientsName.Remove(RemoteIpEndPoint.Address.ToString());
                    for (int i = Multiplayer.clientsIndex.Keys.ToList().IndexOf(RemoteIpEndPoint.Address.ToString()); i < Multiplayer.clientsIndex.Count; i++)
                    {
                        Multiplayer.clientsIndex[Multiplayer.clientsIndex.Keys.ElementAt(i)] = Multiplayer.clientsIndex.Values.ElementAt(i) - 1;
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
        for (int i = 0; i < Multiplayer.clientsIndex.Count; i++)
        {
            Multiplayer.SendMessageToIP(Multiplayer.clientsIndex.Keys.ElementAt(i), "Start");
        }
    }
    public void OnHostMenuLeave()
    {
        ReceiveDataThreadHost.Abort();
        Multiplayer.clientsIndex.Clear();
        Multiplayer.clientsName.Clear();
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
            for (int i = 0; i < Multiplayer.clientOnlyMyIndex; i++)
            {
                playersConnectedList += Multiplayer.clientsIndex.Keys.ElementAt(i) + ": " + Multiplayer.clientsName.Values.ElementAt(i) + "\n";
            }
            playersConnectedList += Multiplayer.GetMyIP() + ": " + nickname.text + "\n";
            for (int i = Multiplayer.clientOnlyMyIndex; i < Multiplayer.clientsIndex.Count; i++)
            {
                playersConnectedList += Multiplayer.clientsIndex.Keys.ElementAt(i) + ": " + Multiplayer.clientsName.Values.ElementAt(i) + "\n";

            }
            yield return new WaitForFixedUpdate();
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
                    Multiplayer.clientOnlyMyIndex = int.Parse(returnData[5].ToString());
                    Multiplayer.HostIP = RemoteIpEndPoint.Address.ToString();
                }
                else if (InfoType.Equals("PJoin"))
                {
                    string pName = "";
                    for (int i = 6; i < returnData.Length; i++)
                    {
                        pName += returnData[i];
                    }
                    Multiplayer.clientOnlyPlayersNames.Add(int.Parse(returnData[5].ToString()), pName);
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
