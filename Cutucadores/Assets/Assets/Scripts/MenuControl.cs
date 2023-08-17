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
    //Host Screen


    Thread ReceiveDataThread;
    public void OnHostMenuEnter()
    {
        addPlayersToMenu = StartCoroutine(AddPlayersToMenu());
        ReceiveDataThread = new Thread(ReceiveData);
        ReceiveDataThread.Start();
    }
    private Coroutine addPlayersToMenu;
    private IEnumerator AddPlayersToMenu()
    {
        while (true)
        {
            playersInSession.text = "";
            string playersConnectedList = "";
            for(int i =0;i<Multiplayer.clients.Count;i++)
            {
                playersConnectedList += Multiplayer.clients.Keys.ElementAt(i) + " " + Multiplayer.clients.Values.ElementAt(i) + "\n";
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    void ReceiveData()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            string InfoType = "";
            string nickname = "";
            for (int i =0;i<5;i++)
            {
                InfoType += returnData[i];
            }
            for(int i = 5; i< returnData.Length;i++)
            {
                nickname += returnData[i];
            }
            try
            {
                if (InfoType.Equals("Enter"))
                {
                    Multiplayer.clients.Add(RemoteIpEndPoint.Address.ToString(), nickname);
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
        ReceiveDataThread.Abort();
        Multiplayer.clients.Clear();
        StopCoroutine(addPlayersToMenu);
        addPlayersToMenu = null;
    }
    public void OnJoinMenuEnter()
    {

    }
    public void JoinSession(Button button)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerToJoinIPAdress.text), 11000);
        Byte[] sendBytes = Encoding.ASCII.GetBytes("Enter"+nickname.text);
        Multiplayer.udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
    }
    public void LeaveSession(Button button)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerToJoinIPAdress.text), 11000);
        Byte[] sendBytes = Encoding.ASCII.GetBytes("Leave");
        Multiplayer.udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
    }
    public void OnJoinMenuLeave()
    {

    }
}