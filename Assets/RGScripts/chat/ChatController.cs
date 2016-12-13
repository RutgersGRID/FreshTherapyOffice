/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * ChatController.cs Revision 1.3.1105.25
 * Controls all chat functionality */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ReactionGrid.Jibe;
using System.Text.RegularExpressions;
using System.IO;

public class ChatController : MonoBehaviour
{
    public GUISkin skin;

    private Vector2 scrollPosition;

    private int PaddingHoriz = 4;
    private int PaddingVert = 56;
    public int WindowWidth = 360;
    public int WindowHeight = 400;
    public bool showChatWindow = true; // can be set in inspector to show or hide chat window by default
    private bool resizewindow = false;
    public int maxMessageHistoryDisplay = 100; // more messages to display is more of a hit on performance - limit this as much as possible and use logging or export for full history

    public Texture2D[] backgroundStyles; // choose window background style
	
	private int blink = 50;
    private float sizeX = 0;
    private float sizeY = 0;

    // Keep all chat message history here, the key of the pair is the chat ID (0 = public, non-zero is avatar ID for a private chat)
    // Note that this will store the entire chat history with some good metadata so you can export it later!
    public Dictionary<int, List<ChatMessage>> messages = new Dictionary<int, List<ChatMessage>>(); 
	public Dictionary<string, Dictionary<int, List<ChatMessage>>> groupMessages = new Dictionary<string, Dictionary<int, List<ChatMessage>>>();
    // Available background styles - alternatives for private chat easy identification
    public Dictionary<int, int> messageBackgrounds = new Dictionary<int, int>();

    private List<int> unreadMessages = new List<int>();
    private int currentUnreadCount = 0;
    private int activeChat = 0;
	public bool groupChatEnabled;
    // Set of private variables to store window sizes
    private Rect chatWindow;
    private Rect messagesWindow;
    private Rect userMessageWindow;
    private Rect privateMessageWindow;
    private Rect privateMessageWindowLabel;

    private string userMessage = "";
    private int userMessageLengthLastCheck = 0;

    // User Is Typing settings
    public float typingDelay = 1.0f;
    private float typingTime = 0.0f;
    public string typingIndicator = "  ... ";

    // Message sounds
    public AudioClip messageSound;
    public float messageSoundVolume = 0.5f;

    // Combination of character and camera controls are needed when resizing the chat window. lost focus will disable chat (chatModeEnabled) etc.
    private GameObject character;
    private GameObject localPlayer;
    private GameObject playerCam;
    private bool resizing = false;
    private bool chatModeEnabled = false;

    // Show/hide functionality
    private float messageReceivedTime = 0.0f;
    public float messageShowTime = 5.0f;
    public float messageFadeTime = 5.0f;
    private bool showMessagesReceivedWhileChatMinimized = false;

    // Chat message tweaks
    private string chatButtonText = "Public Chat";
    public string hideChatPrompt = "Public Chat";
    public string showChatPrompt = "Public Chat";
    private bool showTimeStamp = true;

    private Color guiColor;
    private Color guiTextColor;
    private float transparency;

    // Welcome message handy for greeting users in the Jibe space
    public bool showWelcomeMessage = true;
    public string[] welcomeMessages;

    public bool hideInstantlyIfNoMessagesReceived = false;
    public bool continualChatHistoryExport = true;
    private Vector3 mousepos;

    private string privateMessage = "";
    private int privateMessageRecipient = -1;

    private float chatDistanceLimit = 15;
    public NetworkController networkController;
    public GameObject cursor;

    private Rect roomListWindow;
    private Vector2 roomScrollPosition;
    private bool showRoomList = false;
    public Texture2D roomListUnreadMessagesIndicator;

    public Texture2D[] chatIcons;
    public Texture2D[] voiceIcons;
    public Texture2D resizeIcon;
	public string chatWindowName = "Public Chat";
    // Web Chat Export
    public bool logChatToWeb = true;
    WWW webChatStoreRequest;
    public string chatStoreUrl = "";
	private Dictionary<string, bool> notificationShow = new Dictionary<string, bool>(); //keeps track of whether to show the new chat message for diferrent groups
    public Texture2D newMessageNotificationColor;
	public Texture2D newMessageNotificationColorScrollOver;
	private string currentGroup = "GlobalChat";
	public Texture2D currentChatTexture;
	public Texture2D currentChatTextureScrollOver;
	public bool permissionToStoreMessagesToDatabase = false; //only want one person to be logging the chat to the database, to prevent the same thing being done multiple times
	void Start()
    {
        messages[0] = new List<ChatMessage>();
        messageBackgrounds[0] = 0;
        guiColor = GUI.color;
        guiTextColor = GUI.contentColor;

        chatWindow = new Rect(Screen.width - WindowWidth - PaddingHoriz, Screen.height - WindowHeight - PaddingVert, WindowWidth, WindowHeight);
        messagesWindow = new Rect(Screen.width - WindowWidth - PaddingHoriz, Screen.height - WindowHeight - PaddingVert, WindowWidth, WindowHeight);
        userMessageWindow = new Rect(100, Screen.height - 23, Screen.width - 355, 22);
        roomListWindow = new Rect(4, Screen.height - 326, 160, 300);

        privateMessageWindow = new Rect(100, Screen.height - 23, Screen.width - 200, 22);
        privateMessageWindowLabel = new Rect(100, Screen.height - 40, 120, 22);

        roomListWindow = new Rect(4, Screen.height - 326, 160, 300);

        sizeX = chatWindow.xMin;
        sizeY = chatWindow.yMin;
        character = GameObject.FindGameObjectWithTag("Character");
        if (networkController == null)
        {
            networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        }
        if (cursor == null)
            cursor = GameObject.Find("Cursor");
        // TO REMOVE THE WELCOME MESSAGE
        // remove these lines.
        if (showWelcomeMessage)
        {
            foreach (string msg in welcomeMessages)
            {
                AddChatMessage(msg, "");
            }
        }
    }

    void OnMouseUp()
    {
        if (resizing)
        {
            if (playerCam != null && playerCam.GetComponent<Camera>().enabled)
                playerCam.SendMessage("SetCameraEnabled", true);
            resizing = false;
        }
    }

    void OnGUI()
    {
		blink--;
		if(blink==0)
		{
			blink=50;
			Texture2D filler = newMessageNotificationColor;
			newMessageNotificationColor=newMessageNotificationColorScrollOver;
			newMessageNotificationColorScrollOver=filler;
		}
		bool enterAlreadyUsed = false;
        GUI.skin = skin;

        // Choose appropriate text for the show/hide button
        if (showChatWindow && activeChat == 0 && currentGroup.Equals("GlobalChat"))
        {
            chatButtonText = hideChatPrompt;
        }
        else if (showChatWindow && activeChat != 0)
        {
            chatButtonText = "Public Chat";
        }
        else
        {
            chatButtonText = showChatPrompt;
        }

        // Draw show/hide button and use it to toggle active / inactive chat mode
        GUI.SetNextControlName("ChatButton");
		float offset=170f;
		GUIStyle globalChatStyle = new GUIStyle("Button");
		if(notificationShow.ContainsKey("GlobalChat") && notificationShow["GlobalChat"]==true) //notifcation of new chat message in globalchat
		{
			globalChatStyle.normal.background = newMessageNotificationColor;
			globalChatStyle.hover.background = newMessageNotificationColorScrollOver;
		}
		else if(currentGroup.Equals("GlobalChat") && showChatWindow)
		{
			globalChatStyle.normal.background = currentChatTexture;
			globalChatStyle.hover.background = currentChatTextureScrollOver;
		}
        if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 24, 82, 24), chatButtonText, globalChatStyle) || Input.GetKeyDown(KeyCode.Return))
        {
			chatWindowName="Public Chat";
			if(!currentGroup.Equals("GlobalChat")) //so that chat doesn't hide if you change group while chat is open
			{
				networkController.SetCurrentGroup("GlobalChat",networkController.localPlayer.PlayerID);
				SwitchGroup("GlobalChat");
				notificationShow["GlobalChat"]=false;
				if(!showChatWindow)
				{
					ChatButtonToggled();
				}
			}
			else
			{
				notificationShow["GlobalChat"]=false;
				ChatButtonToggled();
			}
        }
		if(networkController.CheckIfLocalPlayerIsInGroup("Management") || networkController.CheckIfLocalPlayerIsInGroup("GlobalChat"))
		{
			GUIStyle managementChatStyle = new GUIStyle("Button");
			if(notificationShow.ContainsKey("Management") && notificationShow["Management"]==true) //notifcation of new chat message in globalchat
			{
				managementChatStyle.normal.background = newMessageNotificationColor;
				managementChatStyle.hover.background = newMessageNotificationColorScrollOver;
			}
			else if(currentGroup.Equals("Management") && showChatWindow)
			{
				managementChatStyle.normal.background = currentChatTexture;
				managementChatStyle.hover.background = currentChatTextureScrollOver;
			}
			//placement of management chat gui box
			if(GUI.Button(new Rect(Screen.width - 165, Screen.height -24, 82, 24), "Management", managementChatStyle))
			{
				chatWindowName="Management Chat";
				if(!currentGroup.Equals("Management"))//so that chat doesn't hide if you change group while chat is open
				{
					networkController.SetCurrentGroup("Management", networkController.localPlayer.PlayerID);
					SwitchGroup("Management");
					notificationShow["Management"]=false;
					if(!showChatWindow)
					{
						ChatButtonToggled();
					}
				}
				else
				{
					notificationShow["Management"]=false;
					ChatButtonToggled();
				}
			}
			offset-=85;
		}
		if( networkController.CheckIfLocalPlayerIsInGroup("Union") || networkController.CheckIfLocalPlayerIsInGroup("GlobalChat"))
		{
			offset-=85;
			GUIStyle unionChatStyle = new GUIStyle("Button");
			if(notificationShow.ContainsKey("Union") && notificationShow["Union"]==true) //notifcation of new chat message in globalchat
			{
				unionChatStyle.normal.background = newMessageNotificationColor;
				unionChatStyle.hover.background = newMessageNotificationColorScrollOver;
			}
			else if(currentGroup.Equals("Union") && showChatWindow)
			{
				unionChatStyle.normal.background = currentChatTexture;
				unionChatStyle.hover.background = currentChatTextureScrollOver;
			}
			if(GUI.Button(new Rect(Screen.width - 250+offset, Screen.height -24, 82, 24), "Union", unionChatStyle))
			{
				chatWindowName="Union Chat";
				if(!currentGroup.Equals("Union"))//so that chat doesn't hide if you change group while chat is open
				{
					networkController.SetCurrentGroup("Union", networkController.localPlayer.PlayerID);
					SwitchGroup("Union");
					notificationShow["Union"] = false;
					if(!showChatWindow)
					{
						ChatButtonToggled();
					}
				}
				else
				{
					notificationShow["Union"] = false;
					ChatButtonToggled();
				}
			}
		}
        GUIStyle roomListButtonStyle = new GUIStyle("Button");
        if (unreadMessages.Count > 0)
        {
            roomListButtonStyle.normal.background = roomListUnreadMessagesIndicator;
            roomListButtonStyle.normal.textColor = Color.white;
        }
        if (GUI.Button(new Rect(0, Screen.height - 24, 100, 24), showRoomList ? "Hide" : "Online Users", roomListButtonStyle))
        {
            if (showRoomList)
            {
                showRoomList = false;
            }
            else
            {
                showRoomList = true;
            }
        }

        if (showRoomList)
        {
            RenderRoomList();
        }

        // If the chat system is currently active and enabled
        if (showChatWindow)
        {
            // Handle a resize request - on resize, disable the player's camera
            HandleResizing();

            // If the user is ready to send a message, they will press the Enter key
            if (EnterPressed())
            {
				enterAlreadyUsed=true;
                CheckSendMessages();
                Event.current.Use();
            }

            // Set up the window to display chat messages
            GUIStyle windowStyle = new GUIStyle("ChatMessagesWindow");
            windowStyle.normal.background = backgroundStyles[messageBackgrounds[activeChat]];
            if (activeChat == 0)
            {
                GUIContent windowTitle = new GUIContent(chatWindowName, backgroundStyles[0]);
                chatWindow = GUI.Window(1, chatWindow, ShowChatWindow, windowTitle, windowStyle);
            }
            else
            {
                GUIContent windowTitle = new GUIContent("Private Chat: " + networkController.GetRemoteName(activeChat), backgroundStyles[messageBackgrounds[activeChat]]);
                chatWindow = GUI.Window(1, chatWindow, ShowChatWindow, windowTitle, windowStyle);
                MarkMessagesAsRead();
            }

            if (chatModeEnabled && activeChat == 0)
            {
                // PUBLIC CHAT
                GUI.SetNextControlName("text");
                userMessage = GUI.TextField(new Rect(userMessageWindow.x,userMessageWindow.y,userMessageWindow.width+offset,userMessageWindow.height), userMessage, "ChatTextField");
                GUI.FocusControl("text");
            }
            else if (chatModeEnabled && activeChat != 0)
            {
                // PRIVATE CHAT
                GUI.Label(privateMessageWindowLabel, "Send to " + networkController.GetRemoteName(activeChat) + ":", "PrivateChatLabel");
                ChoosePrivateMessageBackgroundColor();
                GUI.SetNextControlName("privatetext");
                privateMessage = GUI.TextField(privateMessageWindow, privateMessage, "ChatTextField");
				//this spams whatever is in the text field while you are typing in "Private Message" form
				Debug.Log("Private message: " + privateMessage);

            }
            else
            {
                GUI.Label(new Rect(userMessageWindow.x,userMessageWindow.y,userMessageWindow.width+offset,userMessageWindow.height), userMessage, "ChatTextField");
            }

            if (Input.GetKey(KeyCode.Escape))//this allows quick unfocus, re-enabling camera and movement
            {
                GUI.UnfocusWindow();
                ChatLostFocus();
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Get mouse position
                mousepos = Input.mousePosition;
                mousepos.y = Screen.height - mousepos.y;

                // Did the user click outside of the chat window?
                if (chatWindow.Contains(mousepos) == false && userMessageWindow.Contains(mousepos) == false && mousepos.x < (Screen.width - 100))
                {
                    GUI.UnfocusWindow();
                    GUI.FocusControl("ChatButton");
                    ChatLostFocus();
                }
                if (mousepos.y > Screen.height - 40)
                {
                    // assume click on the text label, convert it back to a live text box
                    ChatMode(true);
                }
            }           
        }
        else // user has chat window hidden
        {
            if (showMessagesReceivedWhileChatMinimized)
            {
                // Show recent messages as they arrived for a specified period of time
                messageReceivedTime += Time.deltaTime;
                if (messageReceivedTime < messageShowTime)
                {
                    SetTransparency(1);
                    messagesWindow = GUI.Window(1, messagesWindow, ShowChatWindow, "", "ChatWindowNoFocus");
                }
                else if (messageReceivedTime > messageShowTime && messageReceivedTime < (messageShowTime + messageFadeTime))
                {
                    // Gradually fade out the chat window
                    float totallyTransparent = messageShowTime + messageFadeTime;
                    float fullyVisible = messageShowTime;
                    transparency = 1 - (messageReceivedTime - fullyVisible) / (totallyTransparent - fullyVisible);
                    SetTransparency(transparency);
                    messagesWindow = GUI.Window(1, messagesWindow, ShowChatWindow, "", "ChatWindowNoFocus");
                }
                else
                {
                    messageReceivedTime = 0.0f;
                    showMessagesReceivedWhileChatMinimized = false;
                }
            }
            
        }
        InitiatePrivateChat();
		if(EnterPressed() || enterAlreadyUsed && showChatWindow==false)
		{
			ChatButtonToggled();
		}
    }

    private void RenderRoomList()
    {
        GUI.Box(roomListWindow, "Online Users", "ChatMessagesWindow");
        GUILayout.BeginArea(roomListWindow);

        GUI.SetNextControlName("roomscroll");
        roomScrollPosition = GUILayout.BeginScrollView(roomScrollPosition);
        GUILayout.BeginHorizontal();
        RenderUserName(networkController.GetLocalPlayer());
        GUILayout.EndHorizontal();
        foreach (IJibePlayer user in networkController.GetAllUsers())
        {
            GUILayout.BeginHorizontal();
            RenderUserName(user);
            if (chatIcons.Length == 4)
            {
                /* chat icons are as follows:
                 * 0 = no chats available
                 * 1 = chat history available
                 * 2 = actively chatting
                 * 3 = unread messages from player
                */
                Texture2D iconToUse = chatIcons[0];
                if (messages.ContainsKey(user.PlayerID))
                    iconToUse = chatIcons[1];
                else if (activeChat == user.PlayerID)
                    iconToUse = chatIcons[2];
                if (unreadMessages.Contains(user.PlayerID))
                    iconToUse = chatIcons[3];

                if (GUILayout.Button(iconToUse, "PrivateChatButton"))
                {
                    InitiatePrivateChatById(user.PlayerID, user.Name);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void RenderUserName(IJibePlayer user)
    {
        GUIStyle voiceIndicatorStyle = new GUIStyle("PrivateChatButton");
        switch (user.Voice)
        {
            case JibePlayerVoice.IsSpeaking:
                GUILayout.Label(user.Name, "VoiceUserSpeaking");
                if (voiceIcons.Length > 0)
                {
                    GUILayout.Label(voiceIcons[0], voiceIndicatorStyle);
                    GUILayout.Space(2);
                }
                break;
            case JibePlayerVoice.HasVoice:
                GUILayout.Label(user.Name, "VoiceUser");
                if (voiceIcons.Length > 1)
                {
                    GUILayout.Label(voiceIcons[1], voiceIndicatorStyle);
                    GUILayout.Space(2);
                }
                break;
            case JibePlayerVoice.None:
                GUILayout.Label(user.Name, "WindowText");
                break;
        }
    }

    private void ChoosePrivateMessageBackgroundColor()
    {
        GUIStyle backgroundChooserStyle = new GUIStyle("Button");
        backgroundChooserStyle.fixedHeight = 20;
        backgroundChooserStyle.fixedWidth = 20;
        // Offer all available background styles for private chat backgrounds. Choice will persist on a chat-by-chat basis
        messageBackgrounds[activeChat] = GUI.SelectionGrid(new Rect(220, Screen.height - 40, 160, 20), messageBackgrounds[activeChat], backgroundStyles, 6, backgroundChooserStyle);
    }

    private void HandleResizing()
    {
        // Resizing handled by a RepeatButton - strange concept but this is a pretty good way to handle persistent mouse down drag events!
        resizewindow = GUI.RepeatButton(GetResizeButton(), resizeIcon, "WindowResize");
		Rect closeButtonRect = GetResizeButton();
		closeButtonRect.yMax-=10;
		closeButtonRect.x+=chatWindow.width-10;
		if(GUI.Button(closeButtonRect, "X"))
		{
			GameObject.Find("PlayerCamera").SendMessage("DisableSitThisFrame");
			ChatButtonToggled();
		}
        if (resizewindow)
        {
 //           if (playerCam != null && playerCam.camera.enabled)
//                playerCam.SendMessage("SetCameraEnabled", false);
            resizing = true;
            ResizeChatbox();
        }
        if (ResizeMouseDrag())
        {
            ResizeChatbox();
        }
        if (MouseDrag())
        {
            if (GUI.GetNameOfFocusedControl() == "Chat")
            {
                ResizeChatbox();
            }
        }
    }

    void Update()
    {
        if (showChatWindow)
        {
            CheckIsTyping();
        }
    }

    private void CheckIsTyping()
    {
        if (userMessage.Length > 0 && activeChat == 0 && chatModeEnabled)
        {
            // player is typing in public
            // Only show the typing notification for a certain amount of time - if the user is idle too long, they are not deemed to be chatting
            typingTime += Time.deltaTime;
            if (typingTime > typingDelay)
            {
                if (userMessageLengthLastCheck != userMessage.Length)
                {
                    userMessageLengthLastCheck = userMessage.Length;
                    AddMyChatMessage(typingIndicator);
                    typingTime = 0;
                }
            }
        }
    }
    public void ChangeCurrentChat(int newChat)
    {
        if (activeChat != newChat)
        {
            Debug.Log("Changing to chat window " + newChat);
            activeChat = newChat;
        }
        scrollPosition.y = 10000000000; // To scroll down the messages window
    }
    private void CheckSendMessages()
    {
        if (activeChat == 0)
        {
            // Send message if there is one to be sent
            if (userMessage.Length > 0)
            {
                AddMyChatMessage(userMessage);
                userMessage = "";
            }
        }
        else if (privateMessage.Length > 0)
        {
            AddMyPrivateChatMessage(privateMessage);
        }
    }
   
    private void AddToChatHistory(ChatMessage message)
    {
        string messageToSend = message.MessageTime + " " + message.Sender + ": " + message.Message;
        // Handle this differently for web player to offer history via javascript to containing page
		if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // for web player option, sync current chat with hosting web page
            Application.ExternalCall("ChatHistory", messageToSend);
        }
        else
        {
            WriteToChatLogFile(messageToSend);
        }
    }

    private static void WriteToChatLogFile(string messageToSend)
    {
        string filePath = Path.Combine(Application.dataPath, "chatlog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
        //Debug.Log(filePath);
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Append))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(messageToSend);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to log chat: " + ex.Message + ex.StackTrace);
        }
    }

    private void InitiatePrivateChat()
    {
        // Hovering over remote players offers option of initiating a chat on click
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        
        if (playerCam != null)
        {
            if (playerCam.GetComponent<Camera>().enabled)
            {
                Ray mouseRay = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(mouseRay, out hit, chatDistanceLimit))
                {
                    if (hit.transform.tag == "RemotePlayer")
                    {
                        cursor.SendMessage("ShowPrivateChatCursor", true);
                        if (Input.GetMouseButtonDown(0))
                        {
                            // mildly unpleasant way to extract a user name from a gameobject, but we control both ends of this code and it is not hardcoded beyond the convention of using a prefix
                            string messageRecipient = hit.transform.name.TrimStart(networkController.RemotePlayerGameObjectPrefix.ToCharArray());
                            if (int.TryParse(messageRecipient, out privateMessageRecipient))
                            {
                                Debug.Log("Initiate Chat with remote user");
                                cursor.SendMessage("ShowPrivateChatCursor", false);
                                Debug.Log(privateMessageRecipient.ToString());
                                showChatWindow = true;
                                EnsureChatQueue(privateMessageRecipient);
                                chatModeEnabled = true;
                                activeChat = privateMessageRecipient;
                            }
                        }
                    }
                    else
                    {
                        cursor.SendMessage("ShowPrivateChatCursor", false);
                    }
                }
            }
        }
    }

    private void InitiatePrivateChatById(int userId, string username)
    {
        privateMessageRecipient = userId;
        EnsureChatQueue(userId);
        activeChat = userId;
        showChatWindow = true;
    }

    private void ChatLostFocus()
    {
        ChatMode(false);
        typingTime = 0;
    }

    private void ChatButtonToggled()
    {
        if (activeChat == 0)
        {
            if (!hideInstantlyIfNoMessagesReceived)
            {
                messageReceivedTime = 0.0f;
                showMessagesReceivedWhileChatMinimized = true;
            }
            showChatWindow = !showChatWindow;
			if(!showChatWindow)
			{
				GUIUtility.keyboardControl = 0;
			}
            ChatMode(showChatWindow);
        }
        else if (showChatWindow)
        {
            activeChat = 0;
        }
        scrollPosition.y = 10000000000; // To scroll down the messages window
    }

    private void ChatMode(bool setEnabled)
    {
        chatModeEnabled = setEnabled;

        // if enabled, disable camera panning and movement
        if (character == null)
            character = GameObject.FindGameObjectWithTag("Player");
        if (playerCam == null)
            playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        if (GameObject.Find("localPlayer").transform.GetChild(0).GetComponent<PlayerMovement>() != null)
        {
            GameObject.Find("localPlayer").transform.GetChild(0).GetComponent<PlayerMovement>().enabled = !chatModeEnabled;
            GameObject.Find("localPlayer").transform.GetChild(0).GetComponent<PlayerMovement>().SetChatting(chatModeEnabled);
            playerCam.SendMessage("SetChatting", chatModeEnabled, SendMessageOptions.DontRequireReceiver);
           
        }
        else
        {
            // could be non-Jibe avatar - try using local player instead
            if (localPlayer == null)
                localPlayer = GameObject.FindGameObjectWithTag("Player");
            if (localPlayer.GetComponent<PlayerMovement>() != null)
            {
                localPlayer.GetComponent<PlayerMovement>().enabled = !chatModeEnabled;
                localPlayer.GetComponent<PlayerMovement>().SetChatting(chatModeEnabled);
                playerCam.SendMessage("SetChatting", chatModeEnabled, SendMessageOptions.DontRequireReceiver);
            }
        }
        if (playerCam != null && playerCam.GetComponent<Camera>().enabled)
            playerCam.SendMessage("SetCameraEnabled", !chatModeEnabled, SendMessageOptions.DontRequireReceiver);
    }

    private void SetTransparency(float currentTransparency)
    {
        guiColor.a = currentTransparency;
        guiTextColor.a = currentTransparency;
        GUI.color = guiColor;
        GUI.contentColor = guiTextColor;
    }

    private void ResizeChatbox()
    {
        sizeX = Input.mousePosition.x;
        sizeY = (Input.mousePosition.y * -1) + Screen.height;
        if (chatWindow.xMax - sizeX > 150) // allow a resize, but there's a minimum of 150px wide
        {
            chatWindow.xMin = sizeX;
            // resize the hidden view window at the same time
            messagesWindow.xMin = sizeX;
        }
        if (chatWindow.yMax - sizeY > 100) // allow a resize, but there's a minimum of 100px tall
        {
            chatWindow.yMin = sizeY;
            messagesWindow.yMin = sizeY;
        }
        scrollPosition.y = 10000000000; // To scroll down the messages window
    }

    private Rect GetResizeButton()
    {
        return new Rect(chatWindow.x - 10, chatWindow.y - 46, 20, 40);
    }

    private bool MouseDrag()
    {
        return (Event.current.type == EventType.MouseDrag);
    }

    private bool ResizeMouseDrag()
    {
        if (Event.current.type == EventType.MouseDrag)
        {
            return (Input.mousePosition.x >= chatWindow.xMin - 60
                    && Input.mousePosition.x < chatWindow.xMin + 60
                    && Input.mousePosition.y >= Screen.height - chatWindow.yMin - 60
                    && Input.mousePosition.y < Screen.height - chatWindow.yMin + 60);
        }
        else
        {
            return false;
        }
    }

    private bool EnterPressed()
    {
        return (Event.current.type == EventType.keyDown && Event.current.character == '\n');
    }

    void ShowChatWindow(int id)
    {
        // This section controls the appearance of the chat window when enabled for live chatting
        GUI.SetNextControlName("scroll");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		List<ChatMessage> activeMessages;
		if(messages.ContainsKey(activeChat))
		{		
        	activeMessages = messages[activeChat];
		}
		else
		{
			activeMessages = new List<ChatMessage>();
		}
        for (int i = Math.Max(0, activeMessages.Count - maxMessageHistoryDisplay); i < activeMessages.Count; i++)
        {
            ChatMessage message = activeMessages[i];
            GUILayout.BeginHorizontal();
            if (message.IsHyperLink)
            {
                if (GUILayout.Button(message.Message, "HyperLink"))
                {
                    // Web players - if you do an OpenURL the whole page navigates elsewhere... not ideal!
                    // Instead, do a JavaScript call to a function called LoadExternal on the containing page
                    // and do whatever is needed there to display the linked content.
                    if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
                    {
                        Application.ExternalCall("LoadExternal", message.Message);
                    }
                    else
                    {
                        Application.OpenURL(message.Message);
                    }
                }
            }
            else
            {
                if (showTimeStamp)
                {
                    GUILayout.Label(message.MessageTime + " ", "ChatMessagesPrefix");
                    GUILayout.Label(message.Sender + ": ", "ChatMessagesPrefix");
                    GUILayout.Label(message.Message, "ChatMessages");
                }
                else
                {
                    GUILayout.Label(message.Sender + ": ", "ChatMessagesPrefix");
                    GUILayout.Label(message.Message, "ChatMessages");
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        GUILayout.EndScrollView();

        GUI.DragWindow(/*new Rect(0, -50, 10000, 50)*/);
    }

    private void AddMyChatMessage(string message)
    {
        string userName = networkController.GetMyName();
        AddChatMessage(message, userName);
        if (message != typingIndicator && logChatToWeb)
        {
            StartCoroutine(AddChatMessageToWebStore(message));
	        SendChatMessage(message);
        }
    }

    private IEnumerator AddChatMessageToWebStore(string message)
    {
        if (!string.IsNullOrEmpty(chatStoreUrl))
        { 
            IJibePlayer localPlayer = networkController.GetLocalPlayer();

            string playerId = localPlayer.PlayerID.ToString();
            if (PlayerPrefs.GetString("useruuid") != null)
                playerId = PlayerPrefs.GetString("useruuid");

           
            WWWForm chatMessageForm = new WWWForm();
            chatMessageForm.AddField("UserId", playerId);
            chatMessageForm.AddField("UserName", localPlayer.Name);
            chatMessageForm.AddField("Message", message);
            chatMessageForm.AddField("RoomId", Application.loadedLevelName);
            chatMessageForm.AddField("JibeInstance", Application.dataPath);
            chatMessageForm.AddField("LocX", localPlayer.PosX.ToString());
            chatMessageForm.AddField("LocY", localPlayer.PosY.ToString());
            chatMessageForm.AddField("LocZ", localPlayer.PosZ.ToString());

            webChatStoreRequest = new WWW(chatStoreUrl, chatMessageForm);
            yield return (webChatStoreRequest);
            if (!string.IsNullOrEmpty(webChatStoreRequest.error))
            {
                Debug.Log(webChatStoreRequest.error);
            }
        }
    }

    private void AddMyPrivateChatMessage(string message)
    {
        EnsureChatQueue(privateMessageRecipient);
        string userName = networkController.GetMyName();
        AddPrivateChatMessage(message, userName, privateMessageRecipient);
        SendPrivateChatMessage(message);
    }

    private void EnsureChatQueue(int chatQueue)
    {
        // Prevent nasty errors - always make sure there's an element in the messages collection for the specified chat session
        if (!messages.ContainsKey(chatQueue))
        {
            messages[chatQueue] = new List<ChatMessage>();
            messageBackgrounds[chatQueue] = 1;
        }
    }

    private void SendPrivateChatMessage(string message)
    {
        networkController.SendPrivateChatMessage(message, privateMessageRecipient);
        privateMessage = "";
    }

    // This method to be called when remote chat message is received
    public void AddChatMessage(string message, string sender)
    {
        AddMessage(message, sender, 0, networkController.onlyChatWithGroup[networkController.localPlayer.PlayerID]);
    }
	public void StoreChatMessage(string message, string sender, string groupName) //remember what other groups are chatting about in case we switch to their chat
	{
		Debug.Log("Storing chat message");
        ChatMessage newMessage = new ChatMessage();
        newMessage.Message = message;
        newMessage.Sender = sender;
        newMessage.IsHyperLink = false;
		if(groupMessages.ContainsKey(groupName))
		{
			groupMessages[groupName][0].Add(newMessage);
		}
		else
		{
			groupMessages[groupName] = new Dictionary<int, List<ChatMessage>>();
			groupMessages[groupName][0] = new List<ChatMessage>();
			groupMessages[groupName][0].Add(newMessage);
		}
		if(!sender.Equals(networkController.localPlayer.Name))
		{
			notificationShow[groupName]=true;
		}
		if(permissionToStoreMessagesToDatabase)
		{
			Application.ExternalCall("StoreChatMessageInDatabase", message, sender, groupName);
		}
 }
	public void SwitchGroup(string groupName)
	{
		if(groupMessages.ContainsKey(groupName))
		{
//			messages=groupMessages[groupName];
			Dictionary<int, List<ChatMessage>> filler = new Dictionary<int, List<ChatMessage>>();
			foreach(int i in groupMessages[groupName].Keys)
			{
				filler[i] = new List<ChatMessage>();
				foreach(ChatMessage ii in groupMessages[groupName][i])
				{
					filler[i].Add(ii);
				}
			}
			messages = filler;
		}
		else
		{
			groupMessages[groupName] = new Dictionary<int, List<ChatMessage>>();
			groupMessages[groupName][0]=new List<ChatMessage>(); //the default chat list
			messages=new Dictionary<int, List<ChatMessage>>();
		}
		currentGroup=groupName;
	}
    // This method to be called when private chat message is received
    public void AddPrivateChatMessage(string message, string sender, int senderId)
    {
        EnsureChatQueue(senderId);

        if (!unreadMessages.Contains(senderId))
        {
            unreadMessages.Add(senderId);
            AlertUnreadMessages();
        }

        AddMessage(message, sender, senderId, networkController.onlyChatWithGroup[networkController.localPlayer.PlayerID]);
    }

    private void MarkMessagesAsRead()
    {
        if (unreadMessages.Contains(activeChat))
        {
            unreadMessages.Remove(activeChat);
            AlertUnreadMessages();
        }
    }
    public void RemoteUserLeaves(int playerID, string playerName)
    {
        if (unreadMessages.Contains(playerID))
        {
            unreadMessages.Remove(playerID);
            AlertUnreadMessages();
        }
        //AddChatMessage("leaves", playerName);
    }
    private void AlertUnreadMessages()
    {
        if (currentUnreadCount != unreadMessages.Count)
        {
            currentUnreadCount = unreadMessages.Count;
            Application.ExternalCall("ChatAlert", currentUnreadCount);
        }
    }
	public void AddDebugMessage(string message)
	{
/*        ChatMessage newMessage = new ChatMessage();
        newMessage.Message = message;
        newMessage.Sender = "Debug";
        newMessage.IsHyperLink = false;
		if(messages.ContainsKey(0))
		{
            messages[0].Add(newMessage);
		}
		else
		{
			messages[0] = new List<ChatMessage>();
			messages[0].Add(newMessage);
		}
		Debug.Log("Adding a debug message to the chat log");*/
	}
    private void AddMessage(string message, string sender, int senderId) //this is the old version from vanilla jibe - shouldn't be called anymore
    {
        if (!message.EndsWith(typingIndicator))
        {
            // Do a basic test
            if (message.Contains("http"))
            {
                // Try a more complex Regular Expression to match an internet address
                Regex reg = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
                MatchCollection matches = reg.Matches(message);
                if (matches.Count > 0)
                {
                    // found some hyperlinks
                    foreach (Match match in matches)
                    {
                        if (match != null && match.Value.Length > 0)
                        {                         
                            // Add a hyperlink chatmessage with just the matching link
                            ChatMessage hyperlinkMessage = new ChatMessage();
                            hyperlinkMessage.Message = match.Value;
                            hyperlinkMessage.Sender = sender;
                            hyperlinkMessage.IsHyperLink = true;
                            messages[senderId].Add(hyperlinkMessage);
                        }
                    }
                }
            }

            // Add the whole chat message (even if the message contains a hyperlink, it may also contain other text, so show all of it here)
            ChatMessage newMessage = new ChatMessage();
            newMessage.Message = message;
            newMessage.Sender = sender;
            newMessage.IsHyperLink = false;
            if (continualChatHistoryExport)
            {
                AddToChatHistory(newMessage);
            }
			if(messages.ContainsKey(senderId))
			{
				Debug.Log("Storing chat message");
	            messages[senderId].Add(newMessage);
			}
			else
			{
				messages[senderId] = new List<ChatMessage>();
				messages[senderId].Add(newMessage);
			}
//			groupMessages["GlobalChat"][senderId].Add(newMessage);
            scrollPosition.y = 10000000000; // To scroll down the messages window
            GameObject guiObject = GameObject.Find("UIBase");
            if (guiObject != null)
            {
                float playVolume = messageSoundVolume * guiObject.GetComponent<UIBase>().volume / guiObject.GetComponent<UIBase>().audioIcons.Length;
                AudioSource.PlayClipAtPoint(messageSound, transform.position, playVolume);
            }
            if (senderId == 0)
            {
                showMessagesReceivedWhileChatMinimized = true;
                messageReceivedTime = 0.0f;
            }
        }
    }
    public void AddMessage(string message, string sender, int senderId, string groupName)
    {
        if (!message.EndsWith(typingIndicator))
        {
            // Do a basic test
            if (message.Contains("http"))
            {
                // Try a more complex Regular Expression to match an internet address
                Regex reg = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
                MatchCollection matches = reg.Matches(message);
                if (matches.Count > 0)
                {
                    // found some hyperlinks
                    foreach (Match match in matches)
                    {
                        if (match != null && match.Value.Length > 0)
                        {                         
                            // Add a hyperlink chatmessage with just the matching link
                            ChatMessage hyperlinkMessage = new ChatMessage();
                            hyperlinkMessage.Message = match.Value;
                            hyperlinkMessage.Sender = sender;
                            hyperlinkMessage.IsHyperLink = true;
                            messages[senderId].Add(hyperlinkMessage);
                        }
                    }
                }
            }

            // Add the whole chat message (even if the message contains a hyperlink, it may also contain other text, so show all of it here)
            ChatMessage newMessage = new ChatMessage();
            newMessage.Message = message;
            newMessage.Sender = sender;
            newMessage.IsHyperLink = false;
            if (continualChatHistoryExport)
            {
                AddToChatHistory(newMessage);
            }
			if(messages.ContainsKey(senderId))
			{
	            messages[senderId].Add(newMessage);
			}
			else
			{
				messages[senderId] = new List<ChatMessage>();
				messages[senderId].Add(newMessage);
			}
			StoreChatMessage(message, sender, groupName);
            scrollPosition.y = 10000000000; // To scroll down the messages window
            /*GameObject guiObject = GameObject.Find("UIBase"); //spawn popup when new chat message is received
            if (guiObject != null)
            {
                float playVolume = messageSoundVolume * guiObject.GetComponent<UIBase>().volume / guiObject.GetComponent<UIBase>().audioIcons.Length;
                AudioSource.PlayClipAtPoint(messageSound, transform.position, playVolume);
            }
            if (senderId == 0)
            {
                showMessagesReceivedWhileChatMinimized = true;
                messageReceivedTime = 0.0f;
            }*/
        }
    }
    // Send the chat message to all other users
    private void SendChatMessage(String message)
    {
        networkController.SendChatMessage(message);
    }

    public bool IsChatting()
    {
        return chatModeEnabled;
    }
}
/// <summary>
/// Data structure for a chat message
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Default constructor - whenever a new ChatMessage object is created, initialise the timestamp to current datetime
    /// </summary>
    public ChatMessage()
    {
        _timestamp = DateTime.Now;
    }
    private string _message = "";
    /// <summary>
    /// The chat message body
    /// </summary>
    public string Message
    {
        get { return _message; }
        set { _message = value; }
    }
    private bool _isHyperLink = false;
    /// <summary>
    /// Boolean value - if the message is a hyperlink we can optionally treat it differently
    /// </summary>
    public bool IsHyperLink
    {
        get { return _isHyperLink; }
        set { _isHyperLink = value; }
    }

    private DateTime _timestamp;
    /// <summary>
    /// When the message was received (full timestamp)
    /// </summary>
    public DateTime MessageTimeStamp
    {
        get { return _timestamp; }
    }
    /// <summary>
    /// Formatted time when message was received (easier to display in chat history)
    /// </summary>
    public string MessageTime
    {
        get { return _timestamp.ToString("h:mm"); }
    }

    private string _sender;
    /// <summary>
    /// The name of the sender of the message
    /// </summary>
    public string Sender
    {
        get { return _sender; }
        set { _sender = value; }
    }

}
