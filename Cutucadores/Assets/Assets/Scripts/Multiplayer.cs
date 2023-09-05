using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
public class Player
{
    public Player(int ID,string NAME)
    {
        id = ID;
        name = NAME;
    }
    public int id;
    public string name;
    public Animator animator;
    public Vector3 position;
    public Vector3 rotation;
}
public class Multiplayer
{
    public readonly static int maxPlayerCount = 2;
    public static bool isHost;
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
    public class Host
    {
        public static Dictionary<string, Player> clients = new Dictionary<string, Player>(); // Ip to PlayerData
    }
    public class Client
    {
        public static string HostIP;
        public static Player[] players = new Player[maxPlayerCount];
        public static int myID;
    }
    public static void  SendMessageToIP(string ip, string message)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 11000);
        Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
        Multiplayer.udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
    }
}
