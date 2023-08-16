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


public class MenuControl : MonoBehaviour
{
    [SerializeField] private GameObject DefaultMenu, JoinMenu, HostMenu;
    //Host Screen


    Thread ReceiveDataThread;
    public void OnHostMenuEnter()
    {
        ReceiveDataThread = new Thread(ReceiveData);
        ReceiveDataThread.Start();
    }
    private Coroutine addPlayersToMenu;
    private IEnumerator AddPlayersToMenu()
    {
        while (true)
        {

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
            try
            {
                if (returnData.Equals("Enter"))
                {
                    Multiplayer.clients.Add(RemoteIpEndPoint.Address.ToString(), returnData);
                }
                else if (returnData.Equals("Leave"))
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
    }
    public void OnJoinMenuEnter()
    {

    }
    public void OnJoinMenuLeave()
    {

    }
}
