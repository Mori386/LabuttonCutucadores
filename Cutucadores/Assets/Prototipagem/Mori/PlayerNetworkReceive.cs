using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class PlayerNetworkReceive : MonoBehaviour
{
    public PlayerControl playerControl;
    public Vector3 position;
    public Quaternion rotation;
    private void Start()
    {
        playerControl = GetComponent<PlayerControl>();
        ReceiveDataNetworkThread = new Thread(ReceiveDataNetwork);
        ReceiveDataNetworkThread.Start();
    }
    private void Update()
    {
        float step = playerControl.moveSpeed* Time.deltaTime*10f; // calculate distance to move
        float rot = playerControl.rotationSpeed*100f; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, position, step);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rot);
    }
    Thread ReceiveDataNetworkThread;
    void ReceiveDataNetwork()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            Vector3 newPos;
            Quaternion newRot;
            if (returnData[0].ToString().Equals(playerControl.playerID.ToString()))
            {
                string newXValue = "";
                int charsRead = 1;
                for (int i = charsRead; i < receiveBytes.Length; i++)
                {
                    if (returnData[i].ToString() != "Y")
                    {
                        newXValue += returnData[i];
                    }
                    else
                    {
                        charsRead = i;
                        break;
                    }
                }
                string newYValue = "";
                for (int i = charsRead + 1; i < receiveBytes.Length; i++)
                {
                    if (returnData[i].ToString() != "Z")
                    {
                        newYValue += returnData[i];
                    }
                    else
                    {
                        charsRead = i;
                        break;
                    }
                }
                newPos = new Vector3(float.Parse(newXValue), float.Parse(newYValue), 0);

                string newZValue = "";
                for (int i = charsRead + 1; i < receiveBytes.Length; i++)
                {
                    newZValue += returnData[i];
                }
                newRot = new Quaternion(0, 0, float.Parse(newZValue), 0);

                position = newPos;
                rotation = newRot;
            }
        }
    }
}
