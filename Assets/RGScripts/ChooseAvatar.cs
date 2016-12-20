/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.  
 * 
 * ChooseAvatar.cs Revision 1.4.1106.11
 * Initial Loader scene main script - initializes Jibe instances and configures avatar selection  */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ReactionGrid.JibeAPI;
using ReactionGrid.Jibe;
using System.Text.RegularExpressions;
public class ChooseAvatar : MonoBehaviour
{
    private JibeActivityLog jibeLog;
    private IJibeServer jibeServerInstance;
    private IJibePlayer localPlayer;

    public bool debug = true;

    private string userDataConnString;

    public GUISkin gSkin;
    // Choose which level is to be shown next
    public string levelAfterLogin = "JibeBasic";
    private string dressingRoomName = "DressingRoom";
    private string levelToLoad;
    public bool alwaysShowAvatarChoices = false;

    // external data
    private string username = "";
    private string dynamicRoomId = "";  // integer to add to the name of the rooms to which the users are connecting - used for dynamic room support (coming in next release)

    private string password = "";
    private string infoMessage = "Ready for login";
    private string headerMessage = "Connecting to server...";
    private bool isGuest = false;
    private string loadProgress = "0";

    public string maskCharacter = "*";

    public bool opensimAuth = false; // database auth directly from here requires standalone client, will not work in web

    // Headshots
    public Texture2D[] avatarHeadPics;
    // Full pics of avatars
    public Texture2D[] avatarFullPics;
    /// depending on which style the user chooses, the avatarPics array will use either head or full shots
    private Texture2D[] avatarPics;

    private string headShotStyle = "PictureButtonsSmall";
    private string fullPicStyle = "PictureButtons";
    private string selectedStyle;

    public enum LoginScreenImageStyle { HeadOnly, FullPreview };
    // User chooses which style for avatars on login screen
    public LoginScreenImageStyle loginScreenImageStyle = LoginScreenImageStyle.HeadOnly;

    // Some options for changing background image without needing to edit code. However some will need to make more fine grain edits here
    // see comments in the OnGUI method for refining login screen background image position and size
    public Texture loginScreenBackgroundImage;
    public bool useLoginScreenBackgroundImage = true;
    public bool backgroundImageFullWidth = false;
    public bool backgroundImageFullHeight = false;
    public float backgroundImageFixedWidth = 720.0f;
    public float backgroundImageFixedHeight = 480.0f;

    private Color guiColor;
    // When an avatar thumbnail is selected a preview is shown of the full model on the right
    public float avatarPreviewTransparency = 0.7f;
    // The image to show for the login / start button (optional)
    public Texture2D loginButtonImage;

    private LoginResultEventHandler loginResultHandler;
    private int selected = -1;

    public int avatarPreviewsPerRow = 7;
    bool avatarPrefsAlreadyOnFile = false;

    public void Start()
    {
        username = PlayerPrefs.GetString("username", "Guest" + UnityEngine.Random.Range(0, 999));
        // The following code remains in here with comments in case Guest logins are required
        //if (username.StartsWith("Guest"))
        //isGuest = true;
        if (loginScreenImageStyle == LoginScreenImageStyle.FullPreview)
        {
            avatarPics = avatarFullPics;
            selectedStyle = fullPicStyle;
        }
        else if (loginScreenImageStyle == LoginScreenImageStyle.HeadOnly)
        {
            avatarPics = avatarHeadPics;
            selectedStyle = headShotStyle;
        }
    }

    #region Properties set externally - PlayerPrefs store information as small cookies
    public void SetUserName(string newname)
    {
        if (newname != "")
        {
            username = newname;
            PlayerPrefs.SetString("username", username);
        }
    }
    public void SetUserID(string useruuid)
    {
        if (useruuid != "")
        {
            PlayerPrefs.SetString("useruuid", useruuid);
        }
    }
    public void SetDynamicRoomId(string id)
    {
        int dynamicRoomId = 0;
        int.TryParse(id, out dynamicRoomId);
        PlayerPrefs.SetInt("DynamicRoomId", dynamicRoomId);
    }
    #endregion

    void OnApplicationQuit()
    {
        jibeServerInstance.Disconnect();
    }

	void Awake()
	{
		if (!Application.isWebPlayer)
		{
			Init (GameObject.Find ("Jibe").GetComponent<JibeConfig> ().Room);
		}
		else
		{
			Application.ExternalCall("GetUnityRoom");
		}
	}

    void Init(string roomName)
    {
		Debug.Log("Awake");
		Cursor.visible = true;
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(username) || username == "Unknown user")
        {
            username = "Guest" + UnityEngine.Random.Range(0, 999);
            isGuest = true;
            PlayerPrefs.SetString("username", username);
        }

        password = "";
        // The following calls are requests to web page javascript methods - if these are missing from the hosting 
        // page then javascript errors will occur which may impact functionality later in the application. Always
        // include stubs for these methods in the containing page if you can even if you do not need this functionality
        Application.ExternalCall("GetUserName", gameObject.name);
        Application.ExternalCall("GetUserUUID", gameObject.name);
        Application.ExternalCall("GetDynamicRoomId", gameObject.name);

        Application.runInBackground = true;

        // Gather configuration
        GameObject jibeConfiguration = GameObject.Find("Jibe");
        JibeConfig config = jibeConfiguration.GetComponent<JibeConfig>();
		config.Room = roomName;
        userDataConnString = config.UserDataConnString;

        // add the id to each room name for the class for use in dynamic rooms
        if (string.IsNullOrEmpty(dynamicRoomId)) dynamicRoomId = "1";
        PlayerPrefs.SetInt("DynamicRoomId", int.Parse(dynamicRoomId));

        selected = PlayerPrefs.GetInt("avatar", -1);
		if(selected==-1)
		{
			selected=5;
		}
        avatarPrefsAlreadyOnFile = selected > -1;
        if (avatarPrefsAlreadyOnFile)
        {
            levelToLoad = levelAfterLogin;
        }
        else
        {
            levelToLoad = dressingRoomName;
        }

        Debug.Log(config.Room + " " + config.Zone + " " + config.ServerIP + " " + config.ServerPort.ToString() + " " + config.RoomPassword + " " + config.ServerPlatform.ToString());

        if (!JibeComms.IsInitialized())
        {
            try
            {
                Debug.Log("Generate new Jibe instance");
                // Initialize backend server
                switch (config.ServerPlatform)
                {
                    case SupportedServers.JibePhoton:
                        jibeServerInstance = new JibePhotonServer(config.ServerIP, config.ServerPort, config.Zone, config.Room, config.DataSendRate, config.DataSendRate, config.debugLevel, Debug.Log, Debug.LogWarning, Debug.LogError);
                        break;
                    case SupportedServers.JibeSFS2X:
						int port = config.ServerPort;
						#if UNITY_WEBGL
							port = config.WebSocketPort;
						#endif
                        jibeServerInstance = new JibeSFS2XServer(config.ServerIP, port, config.Zone, config.Room, config.RoomPassword, false, config.DataSendRate, config.DataSendRate, config.debugLevel, Debug.Log, Debug.LogWarning, Debug.LogError, config.HttpPort);
                        break;
                }

                JibeComms.Initialize(config.Room, config.Zone, config.ServerIP, config.ServerPort, config.RoomPassword, config.RoomList, config.Version, jibeServerInstance);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);

            }
        }

        loginResultHandler = new LoginResultEventHandler(LoginResult);
        jibeServerInstance.LoginResult += loginResultHandler;
        jibeLog = GameObject.Find("Jibe").GetComponent<JibeActivityLog>();

        headerMessage = "Connecting to server, please wait...";
        try
        {
            // Connect to Jibe and wire up a couple of event handlers for the local player
            localPlayer = jibeServerInstance.Connect();
            localPlayer.NameUpdated += localPlayer_NameUpdated;
            localPlayer.AppearanceUpdated += localPlayer_AppearanceUpdated;
        }
        catch (Exception ex)
        {
            infoMessage = ex.Message;
        }
    }

    void localPlayer_AppearanceUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendAppearance();
    }
    void localPlayer_NameUpdated(object sender, EventArgs e)
    {
        jibeServerInstance.SendName();
    }

    void FixedUpdate()
    {
        if (!Application.CanStreamedLevelBeLoaded(levelToLoad))
        {
            int progress = (int)Math.Round(100 * Application.GetStreamProgressForLevel(levelToLoad));
            loadProgress = "Loading " + progress.ToString() + "%";
            if (progress > 97)
            {
                loadProgress = "Ready for login";
            }
        }
        else
        {
            loadProgress = "Ready for login - click Start!";
        }
    }

    void Update()
    {
        if (jibeServerInstance == null)
        {
            return;
        }
        jibeServerInstance.Update();
    }


    void OnGUI()
    {
        // GUI is built here - if you need to tweak button position and further tweak background image, take care while in here
        GUI.skin = gSkin;

        // Background Image
        float bgWidth = backgroundImageFixedWidth;
        float bgHeight = backgroundImageFixedHeight;
        if (backgroundImageFullWidth)
            bgWidth = Screen.width;
        if (backgroundImageFullHeight)
            bgHeight = Screen.height;
        if (useLoginScreenBackgroundImage)
        {
            GUILayout.BeginArea(new Rect(0, 0, bgWidth, bgHeight), loginScreenBackgroundImage);
        }
        else
        {
            GUILayout.BeginArea(new Rect(0, 0, bgWidth, bgHeight));
        }

        GUILayout.BeginVertical();
        GUILayout.Space(5);

        // Welcome prompt / header message - will show whatever text is set in headerMessage
        GUILayout.Label(headerMessage, "WelcomePrompt");
        try
        {
            if (jibeServerInstance!= null && jibeServerInstance.IsConnected)
            {
                if (!avatarPrefsAlreadyOnFile || alwaysShowAvatarChoices)
                {
                    headerMessage = "Choose an avatar";
                    // Choose avatar - arrange GUI elements in an area (sized here) using layout tools      
                    GUILayout.BeginArea(new Rect(5, 30, 600, 180));
                    GUILayout.BeginHorizontal();

                    // Show the choice of avatars - default is pics laid out in rows up to 8 icons per row
                    selected = GUILayout.SelectionGrid(selected, avatarPics, avatarPreviewsPerRow, selectedStyle);

                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else
                {
                    headerMessage = "Welcome to Rutgers Virtual Worlds!";
                }

                // we're back to vertical layout - the space here controls vertical displacement for next GUI elements (offset from top)
                GUILayout.Space(290);

                GUILayout.BeginHorizontal();
                // horizontal offset from left
                GUILayout.Space(120);

                // Give the player the option to change their name
                GUILayoutOption[] nameoptions = { GUILayout.Width(120), GUILayout.Height(22) };
                username = GUILayout.TextField(username, "UserNameField", nameoptions);

                if (isGuest || Application.platform == RuntimePlatform.WindowsEditor)
                    GUILayout.Label("Edit your name here!", "InstructionLabel");

                if (opensimAuth)
                {
                    password = MaskedPasswordBox(password, nameoptions);
                }
                GUILayout.EndHorizontal();

                if (!Application.CanStreamedLevelBeLoaded(levelToLoad))
                {
                    infoMessage = loadProgress;
                }
                else
                {
                    GUILayout.Space(8);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(120);
                    GUIContent content = new GUIContent("Start", "Start!");
                    GUILayoutOption[] buttonSize = {GUILayout.Width(120), GUILayout.Height(26)};
                    
                    // check the player has selected an avatar, then show button
                    if (selected >= 0 && GUILayout.Button(content, "LoginButton", buttonSize))
                    {
                        // OpenSim auth (or any other auth that goes straight to a database) only works in standalone)
                        if (opensimAuth && (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer))
                        {
                            OSDCJibe osdcJibe = new OSDCJibe(userDataConnString);
                            try
                            {
                                string userid = osdcJibe.AuthenticateUser(username, password);
                                if (!string.IsNullOrEmpty(userid))
                                {
                                    Debug.Log("Authenticated " + userid);

                                    DoLogin(selected);
                                }
                                else
                                {
                                    Debug.Log("Nope.");
                                    infoMessage = "Unable to authenticate avatar";
                                }

                            }
                            catch
                            {
                                Debug.Log("Unable to authenticate.");
                                infoMessage = "Unable to authenticate avatar";
                            }
                        }
                        else
                        {
                            DoLogin(selected);
                        }
                    }
                    /*if (avatarPrefsAlreadyOnFile && levelAfterLogin != dressingRoomName)
                    {
                        if (GUILayout.Button("Dressing Room", "LoginButton", buttonSize))
                        {
                            levelToLoad = dressingRoomName;
                            DoLogin(selected);
                        }
                    }*/
                    GUILayout.EndHorizontal();
                }
                guiColor = GUI.color;
                guiColor.a = avatarPreviewTransparency;
                GUI.color = guiColor;
                if (selected > -1)
                {
                    GUI.DrawTexture(new Rect(420, 10, 150, 300), avatarFullPics[selected]);
                }
                guiColor.a = 1.0f;
                GUI.color = guiColor;
            }
        }
        catch (Exception ex)
        {
            infoMessage = ex.Message + ": " + ex.StackTrace;
            Debug.Log(infoMessage);
        }
        GUILayout.Space(50);
        GUILayout.BeginHorizontal();
        GUILayout.Space(120);
        GUILayout.Label(infoMessage, "InstructionLabel");
        // Must always end all GUILayout elements - missing closing tags do not make unity happy
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private string GetSkinName(string normalTextureName)
    {
        // We rely on naming conventions for getting the name of a skin - all assets in the resources folder must be named according to convention
        // and all headshots and full previews too.
        string skinName = normalTextureName;
        if (loginScreenImageStyle == LoginScreenImageStyle.HeadOnly)
        {
            // remove "head" from name
            skinName = skinName.Substring(0, skinName.IndexOf("Head"));
        }
        return skinName + "_skin";
    }

    private void DoLogin(int selected)
    {   
        LogLoginEvent();
        Debug.Log("DoLogin");
		username = Regex.Replace(username, @"<[^>]*>", String.Empty); //remove all possible html
		username = Regex.Replace(username, @"&[^>]*;", String.Empty);
		if(username.Length>20)
		{
			username=username.Substring(0,19);
		}
        PlayerPrefs.SetString("username", username);
        jibeServerInstance.RequestLogin(username);
    }

    private void LogLoginEvent()
    {
        if (jibeLog != null && jibeLog.logEnabled)
        {
            if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
            {
                Debug.Log(jibeLog.TrackEvent(JibeEventType.Login, Application.absoluteURL, 0.0f, 0.0f, 0.0f, username, username, "Web Player Login"));
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                Debug.Log(jibeLog.TrackEvent(JibeEventType.Login, Application.dataPath, 0.0f, 0.0f, 0.0f, username, username, "Editor login"));
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log(jibeLog.TrackEvent(JibeEventType.Login, Application.dataPath, 0.0f, 0.0f, 0.0f, username, username, "Standalone Client login"));
            }
            else
            {
                Debug.Log(jibeLog.TrackEvent(JibeEventType.Login, Application.dataPath, 0.0f, 0.0f, 0.0f, username, username, "Other Client login"));
            }
        }
    }


    private void LoginResult(object sender, LoginResultEventArgs e)
    {
        if (e.Success)
        {
            // Player has logged in successfully! Store some prefs
            PlayerPrefs.SetInt("avatar", selected);

            // Update localPlayer
            localPlayer.AvatarModel = selected;
            localPlayer.Name = PlayerPrefs.GetString("username");
            localPlayer.Skin = GetSkinName(avatarPics[selected].name);

            // Unwire event handlers
            localPlayer.NameUpdated -= localPlayer_NameUpdated;
            localPlayer.AppearanceUpdated -= localPlayer_AppearanceUpdated;           
            jibeServerInstance.LoginResult -= loginResultHandler;

            // Change scenes
            Debug.Log("Go to " + levelToLoad);
            Debug.Log(localPlayer.ToString()); 
            Application.LoadLevel(levelToLoad);
        }
        else
        {
            Debug.Log("Login FAIL " + e.Message);
            infoMessage = e.Message;
        }
    }

    private string MaskedPasswordBox(string currentPass, GUILayoutOption[] nameoptions)
    {
        // Utility to mask characters typed into a text field
        string maskedPassword = "";
        if (Event.current.type == EventType.repaint || Event.current.type == EventType.mouseDown)
        {
            maskedPassword = "";
            for (int i = 0; i < currentPass.Length; i++)
            {
                maskedPassword += maskCharacter;
            }
        }
        else
        {
            maskedPassword = currentPass;
        }
        GUI.changed = false;
        maskedPassword = GUILayout.TextField(maskedPassword, "PasswordField", nameoptions);
        if (GUI.changed)
        {
            currentPass = maskedPassword;
        }
        return currentPass;
    }
}