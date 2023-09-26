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
    public Thread ReceiveDataNetworkThread;
    private void Start()
    {
        playerControl = GetComponent<PlayerControl>();
        ReceiveDataNetworkThread = new Thread(ReceiveDataNetwork);
        ReceiveDataNetworkThread.Start();
    }
    private void Update()
    {
        float step = playerControl.moveSpeed * playerControl.moveSpeedMultiplier * Time.deltaTime * 1f; // calculate distance to move
        float rot = playerControl.rotationSpeed * playerControl.rotationSpeedMultiplier * Time.deltaTime * 250f; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, position, step);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rot);
        float distance = Vector3.Distance(transform.position, position);
        if (distance > 0.01f)
        {
            playerControl.animator.SetFloat("Speed", 1f);
        }
        else playerControl.animator.SetFloat("Speed", 0f);
        if(fall)
        {
            Debug.Log("Fall in hole");
            playerControl.StartCoroutine(playerControl.FallAnimation(fallHole));
            fall = false;
        }
    }
    public void Teleport(Vector3 pos,Quaternion rot)
    {
        position = pos;
        rotation = rot;

        transform.position = position;
        transform.rotation = rotation;
    }
    private bool fall;
    private Vector3 fallHole;
    public void ReceiveDataNetwork()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            string infoType = returnData.Substring(0, 5);
            Debug.Log(infoType);
            if (infoType == "PosPl")
            {
                if (returnData[5].ToString().Equals(playerControl.playerID.ToString()))
                {
                    Vector3 newPos;
                    Quaternion newRot;
                    string newXValue = "";
                    int charsRead = 6;
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
                        if (returnData[i].ToString() != "W")
                        {
                            newZValue += returnData[i];
                        }
                        else
                        {
                            charsRead = i;
                            break;
                        }
                    }

                    string newWValue = "";
                    for (int i = charsRead + 1; i < receiveBytes.Length; i++)
                    {
                        newWValue += returnData[i];
                    }
                    newRot = new Quaternion(0, 0, float.Parse(newZValue), float.Parse(newWValue));
                    position = newPos;
                    rotation = newRot;
                }
            }
            else if (infoType == "HFall")
            {
                if (returnData[5].ToString().Equals(playerControl.playerID.ToString()))
                {
                    Vector3 holePos;
                    string newXValue = "";
                    int charsRead = 6;
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
                        newYValue += returnData[i];
                    }
                    holePos = new Vector3(float.Parse(newXValue), float.Parse(newYValue), 0);
                    fallHole = holePos;
                    fall = true;
                }
            }
        }
    }
}

