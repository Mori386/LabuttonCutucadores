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


public class MenuControl : MonoBehaviour
{
    [SerializeField] private GameObject DefaultMenu, JoinMenu, HostMenu;
    [SerializeField] private TMP_InputField nickname;
    [Header("Client"), SerializeField] private TMP_InputField ServerToJoinIPAdress;
    [Header("Host"), SerializeField] private TextMeshProUGUI playersInSession;
    [SerializeField] private TextMeshProUGUI serverIP;
    //Host Screen


    Thread ReceiveDataThreadHost;
    public void OnHostMenuEnter()
    {
        serverIP.text = Multiplayer.GetMyIP();
        addPlayersToMenu = StartCoroutine(AddPlayersToMenu());
        ReceiveDataThreadHost = new Thread(ReceiveDataHost);
        ReceiveDataThreadHost.Start();
    }
    private Coroutine addPlayersToMenu;
    private IEnumerator AddPlayersToMenu()
    {
        while (true)
        {
            string playersConnectedList = "";
            for(int i =0;i<Multiplayer.clients.Count;i++)
            {
                playersConnectedList += Multiplayer.clients.Keys.ElementAt(i) + " " + Multiplayer.clients.Values.ElementAt(i) + "\n";
            }
            playersInSession.text = playersConnectedList;
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
            for (int i =0;i<5;i++)
            {
                InfoType += returnData[i];
            }
            for(int i = 5; i< returnData.Length;i++)
            {
                nicknameReceived += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Enter"))
                {
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Cnfrm" + Multiplayer.clients.Count);
                    Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Playr0" + nickname.text);
                    for (int i = 0;i< Multiplayer.clients.Count;i++)
                    {
                        Multiplayer.SendMessageToIP(RemoteIpEndPoint.Address.ToString(), "Playr" + (i+1).ToString() + Multiplayer.clients.Values.ElementAt(i));
                    }
                    Multiplayer.clients.Add(RemoteIpEndPoint.Address.ToString(), nicknameReceived);
                }
                else if (InfoType.Equals("Leave"))
                {
                    Multiplayer.clients.Remove(RemoteIpEndPoint.Address.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public void OnHostMenuLeave()
    {
        ReceiveDataThreadHost.Abort();
        Multiplayer.clients.Clear();
        StopCoroutine(addPlayersToMenu);
        addPlayersToMenu = null;
    }
    public void OnJoinMenuEnter()
    {

    }
    public void JoinSession()
    {
        ReceiveDataThreadJoin = new Thread(ReceiveDataJoin);
        ReceiveDataThreadJoin.Start();
        Multiplayer.SendMessageToIP(ServerToJoinIPAdress.text, "Enter" + nickname.text);
        serverIP.text = Multiplayer.GetMyIP();
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
            string playerCountID = "";
            for (int i = 0; i < 5; i++)
            {
                InfoType += returnData[i];
            }
            for (int i = 5; i < returnData.Length; i++)
            {
                playerCountID += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Cnfrm"))
                {

                }
                else if (InfoType.Equals("Playr"))
                {

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
