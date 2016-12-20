/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * JibeConfig.cs Revision 1.4.2.1108.29
 * Configuration for connection to backend server goes here! */

using UnityEngine;
using System.Collections;
using ReactionGrid.Jibe;
using ReactionGrid.JibeAPI;

public class JibeConfig : MonoBehaviour
{
    public string Room = "ENTER_ROOM_NAME";
    public string Zone = "ENTER_ZONE_NAME";
    public string ServerIP = "ENTER_SERVER_IP";
    public int ServerPort = 0;
	public int WebSocketPort = 8888;
    public string RoomPassword = "ENTER_ROOM_PASSWORD";
    public string[] RoomList = new string[0];
    public string UserDataConnString = "";
    public int DataSendRate = 200;
    public string Version = "1.4.2";
    private string _jibeVersion = "1.4.2";
	public int HttpPort = 0;
	public int PolicyServerPort = 843;

    public JibeDebugLevel debugLevel = JibeDebugLevel.INFO;
    public SupportedServers ServerPlatform = SupportedServers.JibeSmartFox;

    void Awake()
    {
		Application.ExternalCall ("GetRoomName");
        if (RoomList.Length == 0)
        {
            RoomList = new string[1];
            RoomList[0] = Room;
        }
    }

    public string GetVersion()
    {
        return _jibeVersion;
    }
}