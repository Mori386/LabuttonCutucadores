using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Multiplayer
{
    public static Dictionary<string, string> clients = new Dictionary<string, string>(); // Ip to nickname
    public static UdpClient udpClient = new UdpClient(11000);
    public static string myIp;

    public static string GetMyIP()
    {
        if (myIp == null)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                myIp = endPoint.Address.ToString();
                return myIp;
            }
        }
        else return myIp;
    }
}
