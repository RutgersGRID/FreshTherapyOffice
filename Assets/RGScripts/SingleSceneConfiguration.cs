/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.  
 * 
 * SingleSceneConfiguration.cs Revision 1.4.1106.11
 * Used to provide single scene configuration Jibe instances  */

using UnityEngine;
using System;
using System.Collections;
using ReactionGrid.JibeAPI;
using ReactionGrid.Jibe;

public class SingleSceneConfiguration : MonoBehaviour
{
    private bool runSingleSceneConfig = false;
    public GUISkin guiSkin;
    // external data
    private string username = "";
    private string dynamicRoomId = "";  // integer to add to the name of the rooms to which the users are connecting - used for dynamic room support (coming in next release)
    private int selected = -1;
    private IJibeServer jibeServerInstance;
    private IJibePlayer localPlayer;
    private string infoMessage = "Ready for login";
    private string headerMessage = "Connecting to server...";
    private bool isGuest = false;

    // Headshots
    public Texture2D[] avatarHeadPics;
    // Full pics of avatars
    public Texture2D[] avatarFullPics;
    public float fullPicWidth = 128;
    public float fullPicHeight = 128;

    private Color guiColor;
    // When an avatar thumbnail is selected a preview is shown of the full model on the right
    public float avatarPreviewTransparency = 0.7f;
    // The image to show for the login / start button (optional)
    public Texture2D loginButtonImage;

    public Texture2D backgroundImage;

    GameObject jibeGUI;
    GameObject previewCamera;

    public float fadeSpeed = 0.3f;
    private int drawDepth = -1000;
    private float alpha = 1.0f;
    private float fadeDir = -1;
    
    void Update()
    {
        if (runSingleSceneConfig)
        {
            if (jibeServerInstance == null)
            {
                return;
            }
            jibeServerInstance.Update();
        }
    }

    public void RunConfiguration()
    {
        selected = PlayerPrefs.GetInt("avatar", -1);
        if (selected >= avatarHeadPics.Length)
        {
            selected = -1;
        }
        runSingleSceneConfig = true;
        ShowPreviewCamera(true);
        previewCamera = GameObject.Find("PreviewCameraForDesignModeOnly");
        jibeGUI = GameObject.Find("JibeGUI");
        ToggleGUIElements(false);
        alpha = 1;
        fadeIn();
        Debug.Log("Configuring Jibe");
		Cursor.visible = true;
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(username) || username == "Unknown user")
        {
            username = "Guest" + UnityEngine.Random.Range(0, 999);
            isGuest = true;
            PlayerPrefs.SetString("username", username);
        }      
        
        Application.runInBackground = true;

        // Gather configuration
        JibeConfig config = GetComponent<JibeConfig>();

        // add the id to each room name for the class for use in dynamic rooms
        if (string.IsNullOrEmpty(dynamicRoomId)) dynamicRoomId = "1";
        PlayerPrefs.SetInt("DynamicRoomId", int.Parse(dynamicRoomId));

        

        Debug.Log(config.Room + " " + config.Zone + " " + config.ServerIP + " " + config.ServerPort.ToString() + " " + config.RoomPassword + " " + config.ServerPlatform.ToString());

        // Prefetch policy from designated socket server
        // only for web clients
        if (Application.platform == RuntimePlatform.WindowsWebPlayer ||
           Application.platform == RuntimePlatform.OSXWebPlayer ||
           Application.platform == RuntimePlatform.WindowsEditor ||
           Application.platform == RuntimePlatform.OSXEditor)
        {
            bool success = Security.PrefetchSocketPolicy(config.ServerIP, config.ServerPort);
            if (!success)
            {
                Debug.Log("Prefetch policy from network server failed, trying standard policy server port " + config.PolicyServerPort);
                Security.PrefetchSocketPolicy(config.ServerIP, config.PolicyServerPort);
            }
            else
            {
                Debug.Log("Prefetch policy succeeded from " + config.ServerPort);
            }
        }

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
                        jibeServerInstance = new JibeSFS2XServer(config.ServerIP, config.ServerPort, config.Zone, config.Room, config.RoomPassword, false, config.DataSendRate, config.DataSendRate, config.debugLevel, Debug.Log, Debug.LogWarning, Debug.LogError, config.HttpPort);
                        break;
                }

                JibeComms.Initialize(config.Room, config.Zone, config.ServerIP, config.ServerPort, config.RoomPassword, config.RoomList, config.Version, jibeServerInstance);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);

            }
        }

        jibeServerInstance.LoginResult += new LoginResultEventHandler(LoginResult);
        string message = "Connecting to server, please wait...";
        headerMessage = message;
        Debug.Log(message);
        try
        {
            // Connect to Jibe
            localPlayer = jibeServerInstance.Connect();
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to connect!" + ex.Message + ex.StackTrace);
            infoMessage = ex.Message;
        }
        
    }

    void OnGUI()
    {
        if (runSingleSceneConfig)
        {

            GUI.skin = guiSkin;
           
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            // Welcome prompt / header message - will show whatever text is set in headerMessage
            GUILayout.Label(headerMessage, "WelcomePrompt");
            try
            {
                if (jibeServerInstance.IsConnected)
                {                    
                    headerMessage = "Choose an avatar";
                    // Choose avatar - arrange GUI elements in an area (sized here) using layout tools      
                    GUILayout.BeginArea(new Rect(5, 30, 600, 180));
                    GUILayout.BeginHorizontal();

                    // Show the choice of avatars - default is pics laid out in rows up to 7 icons per row
                    selected = GUILayout.SelectionGrid(selected, avatarHeadPics, 7, "PictureButtonsSmall");

                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();

                    // we're back to vertical layout - the space here controls vertical displacement for next GUI elements (offset from top)
                    GUILayout.Space(290);

                    GUILayout.BeginHorizontal();
                    // horizontal offset from left
                    GUILayout.Space(2);

                    // Give the player the option to change their name
                    GUILayoutOption[] nameoptions = { GUILayout.Width(120), GUILayout.Height(22) };
                    username = GUILayout.TextField(username, "UserNameField", nameoptions);

                    if (isGuest || Application.platform == RuntimePlatform.WindowsEditor)
                        GUILayout.Label("Edit your name here!", "InstructionLabel");

                    GUILayout.EndHorizontal();

                    GUIContent content = new GUIContent("Start", "Start!");

                    // check the player has selected an avatar, then show button
                    if (selected >= 0 && GUI.Button(new Rect(2, 350, 80, 22), content, "LoginButton"))
                    {
                        DoLogin(selected);
                    }
                }
                guiColor = GUI.color;
                guiColor.a = avatarPreviewTransparency;
                GUI.color = guiColor;
                if (selected > -1 && selected < avatarFullPics.Length)
                {
                    GUI.DrawTexture(new Rect(420, 10, fullPicWidth, fullPicHeight), avatarFullPics[selected]);
                }

                guiColor.a = 1.0f;
                GUI.color = guiColor;

                // camera fade
                alpha += fadeDir * fadeSpeed * Time.deltaTime;
                alpha = Mathf.Clamp01(alpha);

                guiColor.a = alpha;
                GUI.color = guiColor;
                GUI.depth = drawDepth;

                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundImage);

                guiColor.a = 1.0f;
                GUI.color = guiColor;
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
    }
    private void DoLogin(int selected)
    {
        RetrieveClothingPreference();
        Debug.Log("DoLogin");
        jibeServerInstance.RequestLogin(username);
    }

    private void RetrieveClothingPreference()
    {
        // try to re-use previous clothing options
        string skin = PlayerPrefs.GetString("skin");
        if (!string.IsNullOrEmpty(skin))
            localPlayer.Skin = skin;
        string hair = PlayerPrefs.GetString("hair");
        if (!string.IsNullOrEmpty(hair))
            localPlayer.Hair = hair;
    }

    private string GetSkinName(string normalTextureName)
    {
        // We rely on naming conventions for getting the name of a skin - all assets in the resources folder must be named according to convention
        // and all headshots and full previews too.
        string skinName = normalTextureName;
        skinName = skinName.Substring(0, skinName.IndexOf("Head"));
        return skinName + "_skin";
    }

    private void LoginResult(object sender, LoginResultEventArgs e)
    {
        if (e.Success)
        {
            fadeOut();
            // Player has logged in successfully! Store some prefs
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetInt("avatar", selected);

            // Update localPlayer
            localPlayer.AvatarModel = selected;
            localPlayer.Name = username;
            if (string.IsNullOrEmpty(localPlayer.Skin))
            {
                localPlayer.Skin = GetSkinName(avatarHeadPics[selected].name);
            }

            // Unwire event handlers
            jibeServerInstance.LoginResult -= new LoginResultEventHandler(LoginResult);

            // Now should be ready to spawn avatars and join the room
            runSingleSceneConfig = false;
            ToggleGUIElements(true);
            LogLoginEvent();
            GetComponent<NetworkController>().DoInitialization();
        }
        else
        {
            Debug.Log("Login FAIL " + e.Message);
            infoMessage = e.Message;
        }
    }

    private void LogLoginEvent()
    {
        GameObject jibeObject = GameObject.Find("Jibe");
        JibeActivityLog jibeLog = jibeObject.GetComponent<JibeActivityLog>();
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

    private void ToggleGUIElements(bool enabled)
    {        
        if (jibeGUI != null)
        {
            jibeGUI.SetActiveRecursively(enabled);
        }
        GameObject miniMap = GameObject.Find("MiniMapCamera");
        if (miniMap != null)
        {
            miniMap.GetComponent<Camera>().enabled = enabled;
        }        
    }

    private void ShowPreviewCamera(bool enabled)
    {
        
        if (previewCamera != null)
        {
            Debug.Log("Preview Camera found, setting active to " + enabled);
            previewCamera.GetComponent<PreviewCamera>().SetActive(enabled);
        }
    }

    private void fadeIn()
    {
        fadeDir = -1;
    }

    private void fadeOut()
    {
        fadeDir = 1;
    }

}
