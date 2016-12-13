/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * NetworkController.cs Revision 1.4.1106.29
 * Controls all interaction with back-end networking infrastructure from Unity code files */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ReactionGrid.Jibe;
using System;
using ReactionGrid.JibeAPI;

public class NetworkController : MonoBehaviour
{
	public List<GameObject> objectsToCallJibeInitOn = new List<GameObject>();
	private static int _nextInstanceId = 0;
    private int _instanceId;
	public Dictionary<int,string> onlyChatWithGroup = new Dictionary<int, string>(); //key is userId value is group they are chatting with

    private IJibeServer jibeServerInstance;
    private string defaultRoom;

    public IJibePlayer localPlayer;
	private IJibePlayer host; // player responsible for syncing other players
    public string loaderScene = "Loader";
    public string roomToLoad;
    public bool useDefaultRoom;
	public bool lonely = true; //am i the first one in the world
    public int maxRoomSize;
	private int timeSinceSpawn = 0;
    private string _nextLevel = "";

    public float networkWakeup = 60.0f;
    private float _networkIdle = 0.0f;

    private string _version = "";
    public string RemotePlayerGameObjectPrefix = "remote_";

    // used for loading up a new jibe instance with an application.loadUrl as called from a web player
    private string navigateUrl;
    public ChatInput chatController;
	private Dictionary<int, List<string>> groups = new Dictionary<int, List<string>>(); //RSO keeps track of what groups each player is in - key is playerid values are the list of the groups
    private List<string> spawnedPlayers = new List<string>();
    private Dictionary<int, string> pendingLeavingPlayers = new Dictionary<int, string>(); // collection of players recently left, may respawn or may have actually left - pause a few seconds before notifying player leaving.    
    public float playerLeavingDelay = 3.0f;
    private float playerLeavingInterval = 0.0f;
	private List<Dictionary<string,string>> dataToSync = new List<Dictionary<string, string>>(); //RSO all of the persistent data - currently has to broadcast this as a message and users selectively ignore it.  Eventually may either change the the privatecustomdata that i made in jibesfs2xserver or open up a direct p2p connection and send it around the server
	private bool synced =false;
	private Dictionary<string, List<string>> destroyTable = new Dictionary<string, List<string>>(); //RSO this keeps track of everything that is to be removed from persistent custom data when people leave
	//private UnityP2PNetworkController P2PController;
	public UIGrid onlineUserGrid;
	private List<Dictionary<string,string>> localDataToSync = new List<Dictionary<string, string>>(); //data to sync that only the localplayer should ever send.
	public Transform managementOnlineUsers;
	public Transform unionOnlineUsers;
	public Transform onlineUserPrefab;
	public bool isAdmin;
	private bool repositionNextUpdate;
    public NetworkController()
    {
        // Tracking instance IDs is for advanced debugging
        _instanceId = _nextInstanceId++;
    }

    #region Public properties and methods to retrieve player information
    /// <summary>
    /// Get the current local player's name (or gracefully return an empty string if the user has disconnected)
    /// </summary>
    /// <returns>The local player's name</returns>
    public string GetUserName()
    {
        return IsConnected ? localPlayer.Name : "";
    }
    /// <summary>
    /// Get the current local player's ID (or gracefully return -1 if the user has disconnected)
    /// </summary>
    /// <returns>The local player's ID</returns>
    public int GetUserId()
    {
        return IsConnected ? localPlayer.PlayerID : -1;
    }
    /// <summary>
    /// Get the version number of the world so it can be displayed on screen and used in UAT scenarios
    /// </summary>
    /// <returns>The local player's name</returns>
    public string GetVersion()
    {
        return _version;
    }

    /// <summary>
    /// Public property to determine connection state, using the concept that if a user is connected there will be an instance of a localPlayer object
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return localPlayer != null;
        }
    }

    public string GetMyName()
    {
        return localPlayer.Name;
    }
    public IJibePlayer GetLocalPlayer()
    {
        return localPlayer;
    }
    public string GetRemoteName(int playerId)
    {
        IJibePlayer remotePlayer = jibeServerInstance.GetPlayer(playerId);
        if (remotePlayer != null)
        {
            return remotePlayer.Name;
        }
        else
        {
            return "Unknown";
        }
    }
    public IEnumerable<IJibePlayer> GetAllUsers()
    {
        if (jibeServerInstance != null && jibeServerInstance.Players != null)
            return jibeServerInstance.Players;
        else
            return null;
    }
    #endregion   

    #region LocalPlayer Events - handle all update information sent by the local player and pass to the Jibe Server for transfer over the network
    /// <summary>
    /// Update the localPlayer object with a new player object. If there is already a localPlayer object then
    /// the localPLayer events need to be unwired completely and rewired to the new player object
    /// </summary>
    /// <param name="player">An IJibePlayer object representing the local player</param>
    private void SetLocalPlayer(IJibePlayer player)
    {
        if (localPlayer != null)
        {
            UnwireLocalPlayerEvents();
        }

        localPlayer = player;

        if (localPlayer != null)
        {
            Debug.Log("wiring up local player event handlers");

            WireLocalPlayerEvents();
        }
    }

    /// <summary>
    /// Wire up local player event handling - each of these methods will be invoked when the corresponding event is raised
    /// </summary>
    private void WireLocalPlayerEvents()
    {
        localPlayer.NameUpdated += localPlayer_NameUpdated;
        localPlayer.AnimationUpdated += localPlayer_AnimationUpdated;
        localPlayer.AppearanceUpdated += localPlayer_AppearanceUpdated;
        localPlayer.TransformUpdated += localPlayer_TransformUpdated;
        localPlayer.VoiceUpdated += localPlayer_VoiceUpdated;
    }

    /// <summary>
    /// Unwire event handlers for the local player-  this needs to be done whenever a scene is left and whenever a player disconnects.
    /// Duplicate event handlers will confuse Jibe quite a lot, and will manifest as duplicate chat messages, ghostly dopplegangers, etc.
    /// </summary>
    private void UnwireLocalPlayerEvents()
    {
        localPlayer.NameUpdated -= localPlayer_NameUpdated;
        localPlayer.AnimationUpdated -= localPlayer_AnimationUpdated;
        localPlayer.AppearanceUpdated -= localPlayer_AppearanceUpdated;
        localPlayer.TransformUpdated -= localPlayer_TransformUpdated;
        localPlayer.VoiceUpdated -= localPlayer_VoiceUpdated;
    }
    void localPlayer_TransformUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendTransform(localPlayer.PosX, localPlayer.PosY, localPlayer.PosZ, localPlayer.RotX, localPlayer.RotY, localPlayer.RotZ, localPlayer.RotW);
    }

    void localPlayer_AnimationUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendAnimation(localPlayer.Animation.ToString());
    }
    void localPlayer_AppearanceUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendAppearance();
    }
    void localPlayer_NameUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendName();
    }
    void localPlayer_VoiceUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendSpeech();
    }
    #endregion

    #region Monobehaviour Events - Events that are part of Unity pipeline

    // We start working from here
    void Start()
    {
        Application.runInBackground = true; // Let the application be running while the window is not active.
        Debug.Log("NetController instance " + _instanceId);
        Debug.Log("About to start processing events in level " + Application.loadedLevelName);

        if (JibeComms.IsInitialized())
        {
            DoInitialization();
        }
        else
        {
            JibeConfig config = GetComponent<JibeConfig>();
            SingleSceneConfiguration ssc = GetComponent<SingleSceneConfiguration>();
            if (config != null && ssc != null)
            {
                // Network Controller has configuration information attached! Use it and try single-scene Jibe configuration
                ssc.RunConfiguration();
            }
            else
            {
                Debug.Log("Jibe is null - back to loader");
                Application.LoadLevel("Loader");
                return;
            }
        }
		//P2PController = GameObject.Find("NetworkController").AddComponent<UnityP2PNetworkController>(); //add it during runtime because the p2p controller is a custom script made by me, not in vanilla jibe and this makes upgrading much easier as we don't have to change the prefabs manually
    }

    public void DoInitialization()
    {
        Debug.Log("Jibe initialised - getting config");
        JibeComms jibe = JibeComms.Jibe;
        defaultRoom = jibe.DefaultRoom;
        if (!useDefaultRoom)
        {
            foreach (string room in jibe.RoomList)
            {
                if (roomToLoad == room)
                {
                    defaultRoom = roomToLoad;
                    break;
                }
            }
        }
        _version = jibe.Version;
        Debug.Log("Setting world version to " + _version);

        if (chatController == null)
    //        chatController = GameObject.Find("ChatBox").GetComponent<ChatInput>();

        // Set up connections to the Jibe Server
        jibeServerInstance = jibe.Server;
        // Jibe server publishes many events - we need to subscribe to these events and handle them in here.
        jibeServerInstance.NewRemotePlayer += new RemotePlayerEventHandler(jibeServerInstance_NewRemotePlayer);
        jibeServerInstance.LostRemotePlayer += new RemotePlayerEventHandler(jibeServerInstance_LostRemotePlayer);
        jibeServerInstance.NewChatMessage += new ChatEventHandler(jibeServerInstance_NewChatMessage);
        jibeServerInstance.NewBroadcastChatMessage += new BroadcastChatEventHandler(jibeServerInstance_NewBroadcastChatMessage);
        jibeServerInstance.NewPrivateChatMessage += new PrivateChatEventHandler(jibeServerInstance_NewPrivateChatMessage);
        jibeServerInstance.RoomJoinResult += new RoomJoinResultEventHandler(RoomJoinResult);
        jibeServerInstance.RoomLeaveResult += new RoomLeaveResultEventHandler(RoomLeaveResult);
        jibeServerInstance.CustomDataEvent += new CustomDataEventHandler(jibeServerInstance_CustomDataEvent);

        // Create the local player and connect to the server
        SetLocalPlayer(jibeServerInstance.Connect());
        Debug.Log(localPlayer.ToString());

        if (!IsConnected)
        {
            // If there is no connection to the server, go back to the Loader scene
            Application.LoadLevel("Loader");
            //return;
        }
        else
        {
            // now we're connected, join a room
            jibeServerInstance.JoinRoom(defaultRoom, jibe.RoomPassword);
        }
    }
	public void GroupInitialization()
	{
		Debug.Log("Group initialization");
		if(!PlayerPrefs.HasKey("Group")) //this is currently not in use, but is here so that if we ever want to disable all group features, we can just remove the assignments to the playerprefs in the dressingroom script and everything will act the same as before groups were added
		{
			isAdmin=true;
			onlineUserPrefab = GetComponent<PlayerSpawnController>().adminOnlineUserPrefab; //make sure that when we spawn online users in different groups they still have the mute button next to them.
			SetCurrentGroup("GlobalChat", localPlayer.PlayerID);
		}
		else
		{
			string myGroup = PlayerPrefs.GetString("Group");
			AddMemberToGroup(myGroup, localPlayer.PlayerID, localPlayer.Name);
			SetCurrentGroup("GlobalChat", localPlayer.PlayerID);
			Dictionary<string,string> dataToSend = new Dictionary<string, string>();
			dataToSend["MethodToCall"] = "AddMemberToGroup";
			dataToSend["SendingObjectName"] = "NetworkController";
			dataToSend["Group"] = myGroup;
			dataToSend["PlayerID"] = localPlayer.PlayerID.ToString();
			dataToSend["PlayerName"] = localPlayer.Name;
			SendCustomData(dataToSend, false, new string[0], true);
			Debug.Log("Sent message containing group info");
			foreach(GameObject currentObject in GameObject.FindGameObjectsWithTag("GroupSpecific"))
			{
				currentObject.SendMessage("GroupInit", myGroup, SendMessageOptions.DontRequireReceiver);
			}
			Debug.Log("Finished calling GroupInit");
			if(myGroup=="GlobalChat")
			{
				isAdmin=true;
				onlineUserPrefab = GetComponent<PlayerSpawnController>().adminOnlineUserPrefab; //make sure that when we spawn online users in different groups they still have the mute button next to them.
			}
		}

	}
    void Update()
    {
		if(repositionNextUpdate==true)
		{
			onlineUserGrid.repositionNow=true;
		}
        if (jibeServerInstance == null)
        {
            return;
        }
        jibeServerInstance.Update();
        localPlayer.Update();
    }

    void FixedUpdate()
    {
		if(timeSinceSpawn==90)
		{
		}
		if(timeSinceSpawn<100) //RSO begin - gives time to check if first player in world
		{
			timeSinceSpawn++;
		}
		else if(lonely && timeSinceSpawn>=100 && host==null)
		{
			host=localPlayer;
			//P2PController.setMeAsHost();
			Debug.Log("I was first to join the room, I am now hosting!");
		} //RSO end
		else if(timeSinceSpawn>=100 && host==null)
		{
			Debug.Log("Host left - now setting a new host");
			int lowest = 9999999;
			if(localPlayer==null)
			{
				Debug.Log("WTF local player is null");	
			}
			else
			{				
				lowest = localPlayer.PlayerID;
				host=localPlayer;
			}
        	foreach (IJibePlayer activeplayer in jibeServerInstance.Players)
			{
				if(activeplayer.PlayerID<lowest)
				{
						host=activeplayer;
						lowest = activeplayer.PlayerID;
				}
			}
		}
		else if(host==null)
		{
			Debug.Log("host is null");
		}
		else if(host.Equals(""))
		{
			Debug.Log("host is empty string");
		}
/*		else
		{
			Debug.Log("Host is neither of these");
		}		*/
        _networkIdle += Time.deltaTime;
        if (_networkIdle > networkWakeup)
        {
            Debug.Log("Idle wakeup");
            _networkIdle = 0;
            jibeServerInstance.SendAppearance();
        }

        playerLeavingInterval += Time.deltaTime;
        if (playerLeavingInterval > playerLeavingDelay)
        {
            playerLeavingInterval = 0.0f;
            if (pendingLeavingPlayers.Count > 0)
            {
                int found = 0;
                foreach(int playerId in pendingLeavingPlayers.Keys)
                {
                    foreach (IJibePlayer activeplayer in jibeServerInstance.Players)
                    {
                        if (activeplayer.Name == pendingLeavingPlayers[playerId])
                        {
                            found++;
                            // player has reconnected
                            break;
                        }
                    }
                    if (found == 0)
                    {
                        // assume player has left / disconnected for good
						//processLogoutEvents(playerId); moved to jibeserverinstance on remoteplayer leaves
						//the online users is on top so even though the text in the chat has the same name it will select the onlineuser
					}
                }
            }
            // clear out collection
            pendingLeavingPlayers = new Dictionary<int, string>();
        }
    }
    void OnApplicationQuit()
    {
		if(isAdmin)
		{
			Dictionary<string,string> dataToSend = new Dictionary<string, string>();
			dataToSend["SendingObjectName"] = "VivoxHud";
			dataToSend["MethodToCall"] = "RemoteUserMute";
			dataToSend["Mute"] = "False";
			SendCustomData(dataToSend, true, new string[0], false);
		}
        if (jibeServerInstance != null)
        {
            jibeServerInstance.Disconnect();
        }
    }
	
    #endregion Events   

    #region Jibe Server Event Handlers
    /// <summary>
    /// Event raised when the local player has successfully joined a network room - at this point we have network presence
    /// and it's time to spawn an avatar
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RoomJoinResult(object sender, RoomJoinResultEventArgs e)
    {
        if (e.Success)
        {
            Debug.Log("Room join result in NetController " + _instanceId);
            GameObject spawnedLocalPlayer = GetComponent<PlayerSpawnController>().SpawnLocalPlayer(localPlayer);
            SendTransform(spawnedLocalPlayer.transform);
			SendAppearance(localPlayer.Skin, localPlayer.Hair, localPlayer.AvatarModel);
            Debug.Log("Calling JibeInit on all Jibe objects");
            // Call init methods on all scripts within Jibe and JibeGUI prefabs
            SendMessageToAllJibeObjects("JibeInit");
			GroupInitialization();
        }
        else
        {
            Debug.Log("Room Join FAIL " + e.Message);
        }
    }

    /// <summary>
    /// Sends method calls to all game objects that are children of the Jibe or JibeGUI Prefabs
    /// </summary>
    /// <param name="message">The message (name of method) to call</param>
    private void SendMessageToAllJibeObjects(string message)
    {
        Transform jibeObject = this.transform.parent;
        foreach (Transform t in jibeObject.transform)
        {
            t.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			foreach(Transform tt in t.transform)
			{
				tt.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
        }
        GameObject jibeGUI = GameObject.Find("JibeGUI");
        if (jibeGUI != null)
        {
            foreach (Transform t in jibeGUI.transform)
            {
                t.SendMessage(message, SendMessageOptions.DontRequireReceiver);
            }
        }
		//RSO
		foreach(GameObject currentObject in GameObject.FindGameObjectsWithTag("Door"))
		{
			currentObject.SendMessage("JibeInit");
		}
		foreach(GameObject currentObject in GameObject.FindGameObjectsWithTag("SitTarget"))
		{
			currentObject.SendMessage("JibeInit");
		}
		GameObject.Find("RaiseHand").BroadcastMessage("JibeInit");
		GameObject.Find("ChatBox").SendMessage("JibeInit");
		foreach(GameObject currentObject in objectsToCallJibeInitOn)
		{
			currentObject.SendMessage("JibeInit");
		}
    }


    /// <summary>
    /// Called in response to leaving a network room - once the room has been left on the server, we can clean up the current scene and prepare
    /// to load the next scene.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RoomLeaveResult(object sender, RoomLeaveResultEventArgs e)
    {
        if (e.Success)
        {
            // now we can change levels
            Debug.Log("Player has now left level");
			
			//TODO zpwh
			//send message to GUI_Button to let it know it's okay to reset position
			/*GameObject gui = GameObject.FindWithTag("GUI");
			gui.GetComponent("GUI_Button").NewLevel();*/
			/*GUI_Button someScript;
			someScript = GetComponent<GUI_Button>();
			someScript.NewLevel();*/

            UnwireLocalPlayerEvents();

            jibeServerInstance.NewRemotePlayer -= new RemotePlayerEventHandler(jibeServerInstance_NewRemotePlayer);
            jibeServerInstance.LostRemotePlayer -= new RemotePlayerEventHandler(jibeServerInstance_LostRemotePlayer);
            jibeServerInstance.NewChatMessage -= new ChatEventHandler(jibeServerInstance_NewChatMessage);
            jibeServerInstance.NewBroadcastChatMessage -= new BroadcastChatEventHandler(jibeServerInstance_NewBroadcastChatMessage);
            jibeServerInstance.NewPrivateChatMessage -= new PrivateChatEventHandler(jibeServerInstance_NewPrivateChatMessage);
            jibeServerInstance.RoomJoinResult -= new RoomJoinResultEventHandler(RoomJoinResult);
            jibeServerInstance.RoomLeaveResult -= new RoomLeaveResultEventHandler(RoomLeaveResult);
            jibeServerInstance.CustomDataEvent -= new CustomDataEventHandler(jibeServerInstance_CustomDataEvent);
            if (!string.IsNullOrEmpty(_nextLevel))
            {
                Debug.Log("Changing to new scene " + _nextLevel);
                Application.LoadLevel(_nextLevel);
            }
            else
            {
                // If it is null then user is logging out instead of switching levels
                localPlayer.RaiseDisconnect();
                if (!string.IsNullOrEmpty(navigateUrl))
                {
                    try
                    {
                        Application.OpenURL(navigateUrl);
                    }
                    catch
                    {
                        Debug.Log("Failed to open URL!");
                    }
                }
                else
                {
                    // User is quitting - this gives us the option of a "quit" box in the UI
                    Application.Quit();
                }
            }
        }
        else
        {
            Debug.Log("Room leave FAIL " + e.Message);
        }
    }
    void jibeServerInstance_NewChatMessage(object sender, ChatEventArgs e)
    {
        ChatMessageReceived(e.Message, e.SendingPlayer);
    }
    void jibeServerInstance_NewBroadcastChatMessage(object sender, BroadcastChatEventArgs e)
    {
        BroadcastChatMessageReceived(e.Message);
    }
    void jibeServerInstance_NewPrivateChatMessage(object sender, PrivateChatEventArgs e)
    {
        PrivateChatMessageReceived(e.Message, e.SendingPlayer);
    }
    void jibeServerInstance_LostRemotePlayer(object sender, RemotePlayerEventArgs e)
    {
        playerLeavingInterval = 0.0f;
        pendingLeavingPlayers.Add(e.RemotePlayer.PlayerID, e.RemotePlayer.Name);
        GameObject remotePlayerObject = GameObject.Find(RemotePlayerGameObjectPrefix + e.RemotePlayer.PlayerID);
        if (remotePlayerObject != null)
        {
			processLogoutEvents(e.RemotePlayer.PlayerID);
			Destroy(remotePlayerObject);
        }
    }
    /// <summary>
    /// Handle a new Remote Player joining the scene
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void jibeServerInstance_NewRemotePlayer(object sender, RemotePlayerEventArgs e)
    {
		lonely=false;
        IJibePlayer remotePlayer = e.RemotePlayer;
        bool spawned = false;
        NetworkReceiver ntr = null;
        AnimationSynchronizer anisync = null;

        Debug.Log("Spawn new remote player " + remotePlayer.Name + ", ID: " + remotePlayer.PlayerID);
        GameObject remotePlayerObject = null;

        remotePlayer.AppearanceUpdated += delegate
        {
            if (!spawned)
            {
                remotePlayerObject = GetComponent<PlayerSpawnController>().SpawnRemotePlayer(remotePlayer);
                ntr = remotePlayerObject.GetComponent<NetworkReceiver>();
                anisync = remotePlayerObject.GetComponent<AnimationSynchronizer>();
                jibeServerInstance.RequestFullUpdate(remotePlayer);         
                spawned = true;
            }

            Texture2D newSkin = (Texture2D)Resources.Load(remotePlayer.Skin);
            remotePlayerObject.GetComponent<NetworkReceiver>().SetSkinRemote(newSkin);
        };
        remotePlayer.TransformUpdated += delegate
        {
            if (spawned)
            {                
                Vector3 newPosition = new Vector3(remotePlayer.PosX, remotePlayer.PosY, remotePlayer.PosZ);
                Quaternion newRotation = new Quaternion(remotePlayer.RotX, remotePlayer.RotY, remotePlayer.RotZ, remotePlayer.RotW);
                ntr.ReceiveTransform(newPosition, newRotation);
                if (!spawnedPlayers.Contains(remotePlayer.Name))
                {
                    GetComponent<PlayerSpawnController>().ShowSpawnParticles(newPosition, newRotation);
                    GetComponent<PlayerSpawnController>().PlaySpawnSound();
                    spawnedPlayers.Add(remotePlayer.Name);
                }                
            }
        };

        remotePlayer.AnimationUpdated += delegate
        {
            if (spawned)
            {
                anisync.PlayAnimation(remotePlayer.Animation.ToString());
            }
        };
        // Jibe doesn't currently do much on a name change event,
        // but it is anticipated that in future, this will be used.
        remotePlayer.NameUpdated += delegate
        {
            if (spawned)
            {
                Debug.Log("Name Updated for " + remotePlayer.Name);
            }
        };
        remotePlayer.VoiceUpdated += delegate
        {
            if (spawned)
            {
                //remotePlayerObject.GetComponent<BubblePopup>().SetSpeaking(remotePlayer.Voice == JibePlayerVoice.IsSpeaking);
            }
        };
        remotePlayer.Disconnected += delegate
        {
            Debug.Log("Remote player leaving");
            playerLeavingInterval = 0.0f;
            pendingLeavingPlayers.Add(remotePlayer.PlayerID, remotePlayer.Name);
            if (remotePlayerObject != null)
            {
                Destroy(remotePlayerObject);
            }
        };
    }
    void jibeServerInstance_CustomDataEvent(object sender, CustomDataEventArgs e)
    {
//        Debug.Log("Custom data received from " + e.SendingPlayer.Name + "!");
        // Handle incoming custom data here! 
        /*
         * Now you have your dictionary of data:
         *      Dictionary<string, string> dataReceived = e.CustomData;
         * And you have the sending player:
         *      IJibePlayer player = e.SendingPlayer;
        */

        // Now you can pass this to the appropriate game object and component.
        //GameObject customDataObject = GameObject.Find("SampleCustomData");
        //if (customDataObject != null)
       // {
       //     customDataObject.GetComponent<SampleCustomData>().ShowReceivedData(e.CustomData, e.SendingPlayer.Name);
       // }   
        Dictionary<string, string> dataReceived = e.CustomData;	
//		Debug.Log("Custom data was:  "  + dataReceived["MethodToCall"]);
        if (dataReceived["SendingObjectName"] != null)
        {
            GameObject customDataObject = GameObject.Find(dataReceived["SendingObjectName"]);
//			Debug.Log("SendingObjectName was not null");
            // Generic receiver - this should mean you never have to add code directly to Network Controller again!
            if (dataReceived["MethodToCall"] != null)
            {
//				Debug.Log("Calling custom method");
                customDataObject.SendMessage(dataReceived["MethodToCall"], e.CustomData);
            }

            // Specifically for iTween, and also useful sample code!
            if (dataReceived["iTweenEventToFire"] != null)
            {
                customDataObject.GetComponent<JibeiTweenClick>().ProcessEvent(e.CustomData, e.SendingPlayer.Name);
            }
            else
            {
                customDataObject.GetComponent<SampleCustomData>().ShowReceivedData(e.CustomData, e.SendingPlayer.Name);
            }
        }
    }
    #endregion

    #region Middle-man methods - handle calls from other code to do things that require interaction with the server!
    /// <summary>
    /// Called by scripts to change the current level (a term representing the combination of a 3D scene and a network room)
    /// </summary>
    /// <param name="levelName">Name of the level to join (the Scene name)</param>
    public void ChangeLevel(string levelName)
    {
        if (levelName != defaultRoom)
        {
            _nextLevel = levelName;
            Debug.Log("-- Leaving level --");
            SendMessageToAllJibeObjects("JibeExit");
            jibeServerInstance.LeaveRoom();
            // now wait for RoomLeaveResult to process before moving to next level
        }
        else
        {
            Debug.LogWarning("Can't change to new level - room name is the same as current room!");
        }
    }
    /// <summary>
    /// Called by scripts to send a message to disconnect the current local player and leave the room before loading a new URL - 
    /// clean up dead remote players for other people
    /// </summary>
    /// <param name="urlOfDestination">Optional url of new jibe space to load, if you just want to leave, pass in null</param>
    public void DisconnectLocalPlayer(string urlOfDestination)
    {
        _nextLevel = null; // force this to null so that the RoomLeaveResult callback doesn't switch levels, and instead it disconnects the player
        navigateUrl = urlOfDestination;
        SendMessageToAllJibeObjects("JibeExit");
        Debug.Log("-- Leaving level --");        
        jibeServerInstance.LeaveRoom();        
        // now wait for RoomLeaveResult to process before moving to next level
    }
    public void SendCustomData(Dictionary<string, string> dataToSend)
	{
		string[] users = new string[0];
		SendCustomData(dataToSend, false, users);
	}
	public void SendCustomData(Dictionary<string, string> dataToSend, bool clobber)
	{
		string[] users = new string[0];
		SendCustomData(dataToSend, clobber, users);
	}
	public void SendCustomData(Dictionary<string, string> dataToSend, bool clobber, string[] destroyOnUsersExit, bool isLocallySynced)
	{
		if(!isLocallySynced)
		{
			SendCustomData(dataToSend, clobber, destroyOnUsersExit);
		}
		else
		{
			if(clobber)
			{
				bool found=false;
				for(int i = 0; i<localDataToSync.Count; i++)
				{
					if(localDataToSync[i]["MethodToCall"].Equals(dataToSend["MethodToCall"]))
					{
						if(localDataToSync[i]["SendingObjectName"].Equals(dataToSend["SendingObjectName"]))
						{
							localDataToSync[i]=dataToSend;
							found=true;
							break;
						}
					}
				}
				if(!found)
				{
					localDataToSync.Add(dataToSend);
				}
			}
			else
			{
				localDataToSync.Add(dataToSend);
			}
			jibeServerInstance.SendCustomData(dataToSend);
		}
	}
    public void SendCustomData(Dictionary<string, string> dataToSend, bool clobber, string[] destroyOnUsersExit) //removed all of jibe code and replaced with my own clobber is used if we want this particular function to only have the latest version stored, e.g. in the tag game where we only need to know the current person who is it.   destroyonusersexit  is for destroying data when user leaves, i.e. when a user is holding an object
    {
		if(jibeServerInstance!=null)
		{
	        // Send Custom data here!
	        /* Sample Code (also see SampleCustomData.cs):
	         * in your class that calls this method, construct a Dictionary object as follows
	         *      Dictionary<string, string> dataToSend = new Dictionary<string, string>();
	         * You may need to add "using System.Collections.Generic" to the top of your code 
	         * Then add things to the dictionary:
	         *      dataToSend[key] = value;
	         * where Key is like a lookup or column value (item1, item2, etc.)
	         * and a Value is the actual value to send.
	         * Then, find the NetworkController component of the NetworkController game object, and call this method
	         * and pass in the dataToSend. Your data is then broadcast to others. 
	         * The incoming messages will arrive in the CustomDataEvent above.
	        */
			int indexAddedAt=0;
			bool send=false;
			Dictionary<string, string> destroyDict = new Dictionary<string, string>();
			destroyDict["MethodToCall"] = "addToDestroyTable";
			destroyDict["SendingObjectName"] = "NetworkController";
			destroyDict["Clobber"]="false";
			if(dataToSend.ContainsKey("MethodToCall") && !dataToSend.ContainsKey("SyncData")/* && !(dataToSend["SendingObjectName"].Equals("NetworkController") && dataToSend["MethodToCall"]!="AddMemberToGroup")*/) //checking that this is not data being sent to sync another player so that it does not sync the syncing
			{
				send=true;
				if(clobber)
				{
					bool found = false;
					for(int i=0; i<dataToSync.Count; i++)
					{
						if(dataToSync[i]["MethodToCall"].Equals(dataToSend["MethodToCall"]))
						{
							dataToSync[i]=dataToSend;
							found=true;
							indexAddedAt=i;
							break;
						}
					}
					if(!found)
					{
						Debug.Log("Added data to sync queue: " + dataToSend["MethodToCall"]);
						dataToSync.Add(dataToSend);
						indexAddedAt=dataToSync.Count-1;
					}
				}
				else
				{
					Debug.Log("Added data to sync queue: " + dataToSend["MethodToCall"]);
					dataToSync.Add(dataToSend); //RSO
					indexAddedAt=dataToSync.Count-1;
				}
				destroyDict["DestroyIndex"] = ""+indexAddedAt;
				foreach(string currentPlayerID in destroyOnUsersExit)
				{
					destroyDict[currentPlayerID] = "t";
				}
			}
			if(clobber) //this part here is so that the sender knows it is ok to clobber the code so that both users don't have to do the calculations
			{
				dataToSend["Clobber"] = "true";
			}
			else
			{
				dataToSend["Clobber"] = "false";
			}
			jibeServerInstance.SendCustomData(dataToSend);
			//P2PController.GenericDataSender(dataToSend);
			if(send)
			{
				jibeServerInstance.SendCustomData(destroyDict);
				addToDestroyTable(destroyDict);
			}
			dataToSend["SyncData"] = "true";
		}
		else
		{
			Debug.LogWarning("Attempted to send custom data before the server was initialized");
		}
    }
    #endregion

    #region Local Player Updates
    public void SendTransform(Transform trans)
    {
        localPlayer.PosX = trans.position.x;
        localPlayer.PosY = trans.position.y-2.5f;
        localPlayer.PosZ = trans.position.z;
        localPlayer.RotX = 0;
        localPlayer.RotY = trans.rotation.y;
        localPlayer.RotZ = 0;
        localPlayer.RotW = trans.rotation.w;
        _networkIdle = 0.0f;
    }
    public void UpdateSkin(string skin)
    {
        localPlayer.Skin = skin;
        _networkIdle = 0.0f;
    }
    public void UpdateHair(string hair)
    {
        localPlayer.Hair = hair;
        _networkIdle = 0.0f;
    }
    public void UpdateAvatar(int avatar)
    {
        localPlayer.AvatarModel = avatar;
        _networkIdle = 0.0f;
    }
    public void SetVoice(JibePlayerVoice voiceStatus)
    {
        localPlayer.Voice = voiceStatus;       
        _networkIdle = 0.0f;
    }
    public void SendAppearance(string skin, string hair, int avatar)
    {
        localPlayer.Skin = skin;
        localPlayer.Hair = hair;
        localPlayer.AvatarModel = avatar;
        jibeServerInstance.SendAppearance();
        _networkIdle = 0.0f;
    }

    public void SendAnimation(string animationToPlay)
    {
        localPlayer.Animation = animationToPlay;
        jibeServerInstance.SendAnimation(animationToPlay);
        _networkIdle = 0.0f;
    }
    #endregion

    #region Incoming Chat Messages
    public void ChatMessageReceived(string message, IJibePlayer fromUser)
    {
		Debug.Log("Received message from player with id: " + fromUser.PlayerID);
        int userId = fromUser.PlayerID;
		if(userId != localPlayer.PlayerID)
		{
			/*if(onlyChatWithGroup[localPlayer.PlayerID].Equals("GlobalChat")) //non group chat - changed so that the globalchat is the same as any other group, and it is only that all users are able to view its contents sop this part should be obsolete
			{
				Debug.Log("Sender was in GlobalChat");
	            string spokenMessage = fromUser.Name + ": " + message;
	
	            // Send chat message to the Chat Controller			
	            chatController.AddChatMessage(message, fromUser.Name);
	
	            //Find player object with such Id
	            GameObject user = GameObject.Find(RemotePlayerGameObjectPrefix + userId);
	            //If found - send bubble message
	            if (user != null)
	            {
	                user.SendMessage("ShowBubble", spokenMessage);
	            }
			}
			else //groupchat
			{*/
			bool inGroup=false;
			if(!onlyChatWithGroup.ContainsKey(fromUser.PlayerID))
			{
				onlyChatWithGroup[fromUser.PlayerID]="GlobalChat";
			}
			if(onlyChatWithGroup[localPlayer.PlayerID].Equals(onlyChatWithGroup[fromUser.PlayerID]))
			{
				inGroup=true;
			}
			if(!inGroup)
			{
				chatController.AddMessage(message, fromUser.Name, onlyChatWithGroup[fromUser.PlayerID]);
				Debug.Log("Received chat message from non group member:"+fromUser.Name+" - storing it in group: " + onlyChatWithGroup[fromUser.PlayerID]);
			}
	        if (inGroup)
	        {  // If it's not myself
	            string spokenMessage = fromUser.Name + ": " + message;
	
	            // Send chat message to the Chat Controller			
	            chatController.AddMessage(message, fromUser.Name, onlyChatWithGroup[fromUser.PlayerID]);
				
	            //Find player object with such Id
	            GameObject user = GameObject.Find(RemotePlayerGameObjectPrefix + userId);
	            //If found - send bubble message
	            if (user != null)
	            {
	                user.SendMessage("ShowBubble", spokenMessage);
	            }
		        //}	
			}
		}
        _networkIdle = 0.0f;
    }

    public void BroadcastChatMessageReceived(string message)
    {
        // Send chat message to the Chat Controller			
        chatController.AddMessage(message, "GlobalChat", "SYSTEM");
        _networkIdle = 0.0f;
    }

    public void PrivateChatMessageReceived(string message, IJibePlayer fromUser)
    {
        int userId = fromUser.PlayerID;
        if (fromUser.PlayerID != localPlayer.PlayerID)
        {
            string spokenMessage = fromUser.Name + ": " + message;
            // Send chat message to the Chat Controller			
            chatController.AddPrivateChatMessage(message, fromUser.Name, fromUser.PlayerID);

            //Find player object with such Id
            GameObject user = GameObject.Find(RemotePlayerGameObjectPrefix + userId);
            //If found - send bubble message
            if (user != null)
            {
                user.SendMessage("ShowBubble", spokenMessage);
            }
        }
    }
    #endregion

    #region Outgoing Chat Messages
    public void SendChatMessage(string chatMessage)
    {
        jibeServerInstance.SendChatMessage(chatMessage);
        _networkIdle = 0.0f;
    }
    public void SendPrivateChatMessage(string chatMessage, int recipient)
    {
        Debug.Log("Sending private message " + chatMessage + " to " + recipient);
        jibeServerInstance.SendPrivateChatMessage(chatMessage, recipient);
        _networkIdle = 0.0f;
    }
    #endregion
	//RSO begin
    public void Synchronize()
	{
		if(!synced && host!=localPlayer)
		{
			Debug.Log("Sending synchronization request");
			Dictionary<string,string> dataToSend= new Dictionary<string, string>();
			dataToSend["SendingObjectName"] = "NetworkController";
			dataToSend["MethodToCall"] = "RespondSynchronization";
			SendCustomData(dataToSend);
			synced=true;
		}
	}
	public void RespondSynchronization(Dictionary<string,string> data)
	{
		Debug.Log("Responding");
		if(host==localPlayer)
		{
			Debug.Log("Recieved sync request");
    	    for(int i=0;i<dataToSync.Count;i++)
			{
				SendCustomData(dataToSync[i]);
				Debug.Log("Sending sync data " + dataToSync[i]["MethodToCall"]);  
			}
			Dictionary<string, string> hostData = new Dictionary<string, string>();
			hostData["SendingObjectName"] = "NetworkController";
			hostData["MethodToCall"] = "setHost";
			hostData["host"] = "" + host.PlayerID;
			SendCustomData(hostData);
			Debug.Log("Sent sync");
		}
		else
		{
			Debug.Log("It isn't my job to sync you!");
		}
		foreach(Dictionary<string,string> currentDict in localDataToSync)
		{
			Debug.Log("syncing local data: " + currentDict["MethodToCall"]);
			SendCustomData(currentDict);
		}
	}
	public void setHost(Dictionary<string, string> hostData) //we need to assign a host to do all the calculations that should be server side, but since we can only do client side calculations we have the host pretend to be the server
	{
		Debug.Log("host is now" + hostData["host"]);
		foreach (IJibePlayer activeplayer in jibeServerInstance.Players)
		{
			if((""+activeplayer.PlayerID).Equals(hostData["host"]))
			{
				host=activeplayer;
				break;
			}
		}
	}
	public void processLogoutEvents(int id)
	{
		try
		{
			Debug.Log("Player left - cleaning up networked data they left behind");
			if(host==null) //assigning new host
			{
				int lowest = localPlayer.PlayerID;
				host=localPlayer;
	        	foreach (IJibePlayer activeplayer in jibeServerInstance.Players)
				{
					if(activeplayer.PlayerID<lowest)
					{
							host=activeplayer;
							lowest = activeplayer.PlayerID;
					}
				}
			}
			if(destroyTable.ContainsKey(""+id)) //checks if any items should be removed from syncing now that player left
			{
				foreach(string methodToDestroy in destroyTable[""+id])
				{
					Debug.Log("Removing a method now that player has left");
					int index; 
					bool success = Int32.TryParse(methodToDestroy, out index);
					if(success)
					{
						if(dataToSync.Count>index) //in case it was already destroyed by another player leaving
						{
							dataToSync[index]=null; //can't remove or all other indexes will be confused
						}
					}
					else
					{
						Debug.Log(methodToDestroy+ " is not a valid index");
					}
				}
				destroyTable.Remove(""+id); //clean up
			}
			if(managementOnlineUsers.FindChild(""+id)!=null)
			{
				Destroy(managementOnlineUsers.FindChild(""+id).gameObject);
			}
			if(unionOnlineUsers.FindChild(""+id)!=null)
			{
				Destroy(unionOnlineUsers.FindChild(""+id).gameObject);
			}
			groups.Remove(id);
			Destroy(onlineUserGrid.transform.FindChild(""+id).gameObject); // this should be the online user gameobject
			repositionNextUpdate=true; //we can't reposition the grid now because the gameobject will not be destroyed until the next update so we destroy it then.
	//		onlineUserGrid.repositionNow=true;
		}
		catch(Exception e)
		{
			Debug.LogError(e);
		}
	}
	public void addToDestroyTable(Dictionary<string,string>destroyDict)
	{
		string index = destroyDict["DestroyIndex"];
		Dictionary<string,string>.KeyCollection keys = destroyDict.Keys; //need to turn into a KeyCollection so that it is iterable
		foreach(string playerToAdd in keys) //removing keys for index etc. not worth time it takes to check, just add and ignore them
		{
			if(destroyTable.ContainsKey(playerToAdd)) //checking if this is first instance of player having item in removetable
			{
				destroyTable[playerToAdd].Add(index);
			}
			else
			{
				destroyTable[playerToAdd]=new List<string>();
				destroyTable[playerToAdd].Add(index);
			}	
		}
	}
		//RSO Group stuff
	public void AddMemberToGroup(Dictionary<string,string> groupInfo)
	{
		AddMemberToGroup(groupInfo["Group"],Int32.Parse(groupInfo["PlayerID"]), groupInfo["PlayerName"]);
	}
	public void AddMemberToGroup(string groupName, int playerIDToAdd, string playerNameToAdd)
	{
		bool addToOnlineUsers=false;
		Debug.Log("Attempting to add member to a group");
		if(groups.ContainsKey(playerIDToAdd))
		{
			if(!groups[playerIDToAdd].Contains(groupName))
			{
				addToOnlineUsers=true;
				groups[playerIDToAdd].Add(groupName);
				Debug.Log("Successfully added player with ID:" + playerIDToAdd + "to group:" +groupName);
			}
		}
		else
		{
			addToOnlineUsers=true;
			groups[playerIDToAdd] = new List<string>();
			groups[playerIDToAdd].Add(groupName);
			Debug.Log("Successfully added player with ID:" + playerIDToAdd + " to group:" +groupName);
		}
		if(playerNameToAdd!=localPlayer.Name && addToOnlineUsers==true)
		{
			if(groupName=="Management")
			{
				Transform newOnlineUser = GameObject.Instantiate(onlineUserPrefab) as Transform;
				newOnlineUser.parent=managementOnlineUsers;
				newOnlineUser.name=""+playerIDToAdd;
				newOnlineUser.localPosition=new Vector3(newOnlineUser.localPosition.x,newOnlineUser.localPosition.y, -17f);
				newOnlineUser.GetComponentInChildren<UILabel>().text=playerNameToAdd;
				managementOnlineUsers.GetComponent<UIGrid>().repositionNow=true;
			}
			else if(groupName=="Union")
			{
				Transform newOnlineUser = GameObject.Instantiate(onlineUserPrefab) as Transform;
				newOnlineUser.parent=unionOnlineUsers;
				newOnlineUser.name=""+playerIDToAdd;
				newOnlineUser.localPosition=new Vector3(newOnlineUser.localPosition.x,newOnlineUser.localPosition.y, -17f);
				newOnlineUser.GetComponentInChildren<UILabel>().text=playerNameToAdd;
				unionOnlineUsers.GetComponent<UIGrid>().repositionNow=true;
			}
		}
	}
	public void RemoveMemberFromGroup(string groupName, int playerIDToAdd)
	{
		if(groups[playerIDToAdd]!=null)
		{
			groups[playerIDToAdd].Remove(groupName);
		}
	}
	public void SetCurrentGroup(string groupName, int playerID)
	{
		Debug.Log("Received an update to my current group");
		onlyChatWithGroup[playerID] = groupName;
		Dictionary<string,string> syncCurrentGroup = new Dictionary<string, string>();
		syncCurrentGroup["MethodToCall"] = "SetCurrentGroup2";
		syncCurrentGroup["SendingObjectName"] = "NetworkController";
		syncCurrentGroup["Group"] = groupName;
		syncCurrentGroup["ID"] = playerID.ToString();
		SendCustomData(syncCurrentGroup, true, new string[0], true);
		if(playerID==localPlayer.PlayerID)
		{
			/*if(groupName.Equals("GlobalChat"))
			{
				Application.ExternalCall("DisplayChatBox", "UnionPlaque");
				Application.ExternalCall("DisplayChatBox", "ManagementPlaque");
			}
			else
			{
				Application.ExternalCall("DisplayChatBox", groupName+"Plaque");
			}*/
		}
	}
	public void SetCurrentGroup2(Dictionary<string,string> data)// this does the same thing as the above script, but it won't let me have helper methods for some reason when i use sendmessage
	{
		Debug.Log("Received an update to current group from another player");
		onlyChatWithGroup[Int32.Parse(data["ID"])] = data["Group"];
	}
	public bool CheckIfLocalPlayerIsInGroup(string groupName)
	{
		if(groups!=null && localPlayer!=null && groups.ContainsKey(localPlayer.PlayerID)) //so it doesn't crash if the localPlayer hasn't finished initializing yet
		{
			foreach(string currentGroup in groups[localPlayer.PlayerID])
			{
				if(currentGroup.Equals(groupName))
				{
					return true;
				}
			}
		}
		return false;
	}
	
	
	//rso end
}
