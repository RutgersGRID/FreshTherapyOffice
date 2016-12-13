/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * VivoxHud2.cs Revision 1.4.1107.19
 * VivoxHud for use with Jibe 1.x projects - not compatible with previous editions  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReactionGrid.Jibe;

public class VivoxHud2 : MonoBehaviour
{
    public GUISkin skin;
    private bool hasVersionBeenChecked = false;
    public NetworkController networkController;

    private string myPlayerName = "";
    public bool isReady = false;
    public bool debugMode = false;

    public string channelName = "sip:confctl-158@regp.vivox.com"; // Set to the Vivox channel for the current scene using the inspector

    private List<VivoxPlayerNode> playerList = new List<VivoxPlayerNode>();

    // Control network messages about voice
    private float networkSpeechSyncBlock = 2.0f;
    private float lastNetworkSend = 0.0f;
    private JibePlayerVoice voiceStatus = JibePlayerVoice.None;
    private JibePlayerVoice myCurrentVoiceState = JibePlayerVoice.None;
    private bool micOpen = false;
    public Texture2D toggleMicOnIcon;
    public Texture2D toggleMicOffIcon;
    private Rect micIconPosition;
    private GUIStyle settingsIconStyle;
	private ChatInput chatController;
	public Mic micButton;//used to output debugs to the chatlog
	private bool globallyMuted;
	private bool locallyMuted;
    // Called via broadcast from NetworkController - anything with this method signature within the Jibe or JibeGUI game object will be run as soon as a local player exists and the scene is ready    
    public void JibeInit()
    {
        if (!string.IsNullOrEmpty(channelName))
        {
            Debug.Log("Initializing Vivox");
            ConnectVivox();

//            micIconPosition = GameObject.Find("UIBase").GetComponent<UIBase>().GetMicIconPosition();
//            Debug.Log("Mic Icon Position: " + micIconPosition);
//            settingsIconStyle = GameObject.Find("UIBase").GetComponent<UIBase>().GetSettingsButtonStyle();
        }
        else
        {
            Debug.Log("Not configured for Vivox voice");
        }
    }
    // Called via broadcast from NetworkController - anything with this method signature within the Jibe or JibeGUI game object will be run when a player requests to leave the current scene
    public void JibeExit()
    {
        if (!string.IsNullOrEmpty(channelName))
        {
            Debug.Log("Disconnecting Vivox");
            Logout();
        }
        else
        {
            Debug.Log("Not configured for Vivox voice");
        }
    }

    public void ConnectVivox()
    {
        isReady = true;
        myPlayerName = networkController.GetMyName();
        Application.ExternalCall("VivoxUnityInit");
    }
    void Start()
    {
        if (networkController == null)
            networkController = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
		if(chatController ==null)
		{
//			chatController= GameObject.Find("ChatBox").GetComponent<ChatInput>();
		}
		if(PlayerPrefs.HasKey("Group"))
		{

		}
    }
    public void Logout()
    {
        Application.ExternalCall("VivoxLogout", channelName);
    }
   /* void OnGUI()
    {
        if (isReady)
        {
            GUI.skin = skin;
            if (debugMode)
            {
                // Debug messages showing who is connected to the voice channel - this is only for last resort debugging
                // since this code is still relatively beta            
                GUILayout.BeginArea(new Rect(50, 50, 400, 400));
                GUILayout.BeginVertical();
                foreach (IJibePlayer player in networkController.GetAllUsers())
                {
                    GUILayout.Label(player.Name + " " + player.Voice);
                }
                foreach (VivoxPlayerNode entry in playerList)
                {
                    GUILayout.Label(entry.playerName + " " + entry.hasVoice + " " + entry.isSpeaking);
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            /*if (settingsIconStyle != null)
            {
                if (GUI.Button(micIconPosition, !micOpen ? toggleMicOffIcon : toggleMicOnIcon, settingsIconStyle))
                {
                    Debug.Log("Toggling microphone: " + micOpen);
                    Application.ExternalCall("VivoxMicMute", micOpen);
                    micOpen = !micOpen;
                }
            }*/
/*        }
    }*/

    void FixedUpdate()
    {		
        if (isReady)
        {
            lastNetworkSend += Time.deltaTime;
            if (voiceStatus != myCurrentVoiceState && lastNetworkSend > networkSpeechSyncBlock)
            {
                voiceStatus = myCurrentVoiceState;
                lastNetworkSend = 0.0f;
                addMessage("Sending voice status: " + voiceStatus);
                networkController.SetVoice(voiceStatus);
            }
        }
    }

    //called from vioxunity.js response when user logs in
    void onVivoxLogin(string message)
    {
        addMessage(message);
        addMessage("Creating Vivox voice channel (" + channelName + ").");
        Application.ExternalCall("VivoxCreateChannel", channelName);
        Application.ExternalCall("VivoxMicMute", micOpen);
    }

    //void onVivoxLogout()
    //{
    //    netController.VivoxLogoutComplete();
    //}

    //called from vioxunity.js response when vivox voice object created
    void onVersionCheck(string message)
    {
        string[] my_array = message.Split(":"[0]);
        addMessage(my_array[1]);
        if (my_array[0] == "0")
        {
            if (!hasVersionBeenChecked)
            {
                addMessage("Installing Vivox Voice plugin.");
                Application.ExternalCall("VivoxInstall");
                hasVersionBeenChecked = true;
            }
        }
    }

    //called from vioxunity.js response when participant joins channel
    void VivoxParticipantAdded(string participant)
    {
        VivoxPlayerNode newEntry = new VivoxPlayerNode();
        newEntry.hasVoice = true;
        newEntry.isSpeaking = false;
        newEntry.playerName = participant;
        playerList.Add(newEntry);
    }

    //called from vioxunity.js response when participant leaves channel
    void VivoxParticipantRemoved(string participant)
    {
        foreach (VivoxPlayerNode entry in playerList)
        {
            if (entry.playerName == participant)
            {
                addMessage(entry.playerName + " removed from Vivox Voice.");
                playerList.Remove(entry);
                break;
            }
        }
    }

    //called from vioxunity.js response when a participant is speaking
    void VivoxParticipantIsSpeaking(string combo)
    {
        string[] my_array = combo.Split(":"[0]);
        foreach (VivoxPlayerNode entry in playerList)
        {
            if (entry.playerName == my_array[0])
            {
                Debug.Log("Player: " + entry.playerName + " is speaking: " + my_array[1]);

                if (entry.playerName == myPlayerName)
                {
                    entry.isSpeaking = bool.Parse(my_array[1]);
                    if (entry.isSpeaking)
                    {
                        addMessage(entry.playerName + " is talking");
                        myCurrentVoiceState = JibePlayerVoice.IsSpeaking;
                    }
                    else
                    {
                        addMessage(entry.playerName + " is not talking");
                        myCurrentVoiceState = JibePlayerVoice.HasVoice;
                    }
                }
            }
        }
    }

    //called from vioxunity.js response when channel is created
    void onVivoxChannelCreate(string combo)
    {
        string[] my_array = combo.Split(":"[0]);
        if (my_array[0] != "Error")
        {
            addMessage("Vivox channel created (" + combo + "). Joining channel.");
            Application.ExternalCall("VivoxJoinChannel", combo, "0");
        }
        else
        {
            addMessage(combo);
        }
    }

    //called from vioxunity.js response when vivox voice object is connected
    void onVivoxConnected(string message)
    {
        Application.ExternalCall("VivoxLogin", myPlayerName);
        lastNetworkSend = 0.0f;
        networkController.SetVoice(JibePlayerVoice.HasVoice);
        myCurrentVoiceState = JibePlayerVoice.HasVoice;
//        micIconPosition = GameObject.Find("UIBase").GetComponent<UIBase>().GetMicIconPosition();
        Debug.Log("Mic Icon Position: " + micIconPosition);
    }

    //adds message to the vivox messages hud
    void addMessage(string message)
    {
        if (debugMode)
        {
            if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
            {
                // for web player option, send debug message to hosting web page
                Application.ExternalCall("DebugHistory", message);
            }
        }
    }
	public void SwitchToChannel(string newChannel)
	{
		if(!newChannel.Equals(channelName))
		{
			isReady=false;
			Debug.Log("Switching Vivox Channel");
			Application.ExternalCall("SwitchToChannel", newChannel);
		}
	}
	public void UpdateCurrentChannel(string newChannel) //called from webpage after the channel has been switched
	{
		channelName=newChannel;
	}
	public void HandleMuting(bool isMuted)
	{
		Application.ExternalCall("HandleMuting", isMuted);
	}
	public void VivoxJoinedRoom(string doesntdoanything)
	{
		EnterNewZone.isReady=true;
		Mic.UpdateMicStatus();
	}
	public void RemoteUserMute(Dictionary<string,string> data)
	{
		if(!networkController.isAdmin)
		{
			Debug.Log("My name: "+ networkController.localPlayer.Name);
			Debug.Log("Their name: "+ networkController.localPlayer.Name);
			if(!data.ContainsKey("UserName"))
			{
				Debug.Log("Toggling mute");
				if(data["Mute"]=="True")
				{
					globallyMuted=true;
				}
				else
				{
					globallyMuted=false;
				}
				if(data["Mute"]=="True")
				{
					micButton.canUnMute=false;
					micButton.Mute(true);
				}
				else if(!locallyMuted)
				{
					micButton.ResetSprite();
					micButton.canUnMute=true;
				}
			}
			else if(data["UserName"].Equals(networkController.localPlayer.Name))
			{
				if(data["Mute"]=="True")
				{
					locallyMuted=true;
				}
				else
				{
					locallyMuted=false;
				}
				if(data["Mute"]=="True")
				{
					micButton.canUnMute=false;
					micButton.Mute(true);
				}
				else if(!globallyMuted)
				{
					micButton.ResetSprite();
					micButton.canUnMute=true;
				}
			}
		}
		else
		{
			//admins cannot be muted, but their mute icons should be updated.
			if(data.ContainsKey("ID"))
			{
				GameObject onlineUsers = GameObject.Find("OnlineUserParent");
				for(int i=0; i<onlineUsers.transform.GetChildCount(); i++)
				{
					Transform targetOnlineUser = onlineUsers.transform.GetChild(i).FindChild(data["ID"]);
					if(targetOnlineUser!=null)
					{
						targetOnlineUser.gameObject.BroadcastMessage("FakePress");
					}
				}
			}
			else
			{
				GameObject.Find("Global Mute").SendMessage("FakePress");
			}
		}
	}
}

class VivoxPlayerNode
{
    public string playerName;
    public bool hasVoice = false;
    public bool isSpeaking = false;
}