/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * DressingRoom.cs Revision 1.3.1105.25
 * Dressing room functionality allowing for avatar selection and outfit changing  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ReactionGrid.Jibe;
using System;

using ReactionGrid.JibeAPI;

public class DressingRoom : MonoBehaviour
{
    // This script is like a mini network controller for this limited scene. 
    // No multiplayer interaction occurs here, but we need to maintain a connection to Jibe server and interact with it
    private IJibeServer jibeServerInstance;

    // Avatar prefabs, headshot previews and available hair textures
    public Transform[] playerPrefabs;
    public Texture2D[] avatarHeadPics;
    public Texture2D[] hairTextures;

    private string headShotStyle = "PictureButtonsSmall";

    private List<Texture2D> availableSkinPreviews;
    private List<Texture2D> availableSkins;
    private List<Texture2D> availableHair;

    private IJibePlayer localPlayer;

    // The main loader scene
    public string loaderScene = "Loader";
	private int indexToLoad = 0;
    // The next level is the first regular Jibe scene
    public string[] nextLevel;
    public string[] nextLevelPassword;
	public string[] nextLevelPrompt;
	private int authenticationIndex = -1;
	private string currentPasswordAttempt = "";

    public GUISkin jibeSkin;

    // Handle selection changes for avatar, skin and hair using selection grids
    private int _selected = -1;
    private int _currentSelection = -1;

    private int _selectedSkin = 0;
    private int _currentSelectedSkin = 0;

    private int _selectedHair = 0;
    private int _currentSelectedHair = 0;

    private bool hasHair = false;
    private bool isEmitting = false;
    private float emitTimer = 0.0f;
    // How long to show particles when a new player model is chosen
    public float emissionTime = 1.0f;
    public string loadProgress;
	private bool spawnPlayerNextUpdate = false;
    // Assign a reference to the pose stand (which contains particle emitters)
    public GameObject poseStand;


    public string GetUserName()
    {
        return IsConnected ? localPlayer.Name : "";
    }
    public int GetUserId()
    {
        return IsConnected ? localPlayer.PlayerID : -1;
    }

    public bool IsConnected
    {
        get
        {
            return localPlayer != null;
        }
    }

    // We start working from here
    void Start()
    {
		EnterNewZone.isReady=false;
		PlayerPrefs.DeleteKey("Group");
        Application.runInBackground = true; // Let the application be running while the window is not active.
		Cursor.visible = true;
        if (JibeComms.IsInitialized())
        {
            Debug.Log("Jibe initialised - getting config");
            JibeComms jibe = JibeComms.Jibe;

            jibeServerInstance = jibe.Server;
            jibeServerInstance.RoomJoinResult += new RoomJoinResultEventHandler(RoomJoinResult);
            jibeServerInstance.RoomLeaveResult += new RoomLeaveResultEventHandler(RoomLeaveResult);

            localPlayer = jibeServerInstance.Connect();
            _selected = localPlayer.AvatarModel;
            _currentSelection = _selected;

            availableSkinPreviews = new List<Texture2D>();
            availableSkins = new List<Texture2D>();
            availableHair = new List<Texture2D>();

            PopulateSkinsAndHair();

            if (!IsConnected)
            {
                Application.LoadLevel("Loader");
                return;
            }
            else
            {
                // Unlike on a regular level, we're not joining a regular room, just the dressing room
                jibeServerInstance.JoinRoom("DressingRoom", jibe.RoomPassword);          
            }
        }
        else
        {
            Debug.Log("Jibe is null - back to loader");
            Application.LoadLevel("Loader");
            return;
        }

        Debug.Log("About to start processing events in level " + Application.loadedLevelName);

    }
    void FixedUpdate()
    {
        // Progress bar for next level
		foreach(string currentLevel in nextLevel)
		{
	        if (!Application.CanStreamedLevelBeLoaded(currentLevel))
	        {
	            int progress = (int)Math.Round(100 * Application.GetStreamProgressForLevel(currentLevel));
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
    }
    void OnGUI()
    {
        GUI.skin = jibeSkin;

        // GUI for dressing room here
        GUI.Label(new Rect(5, 0, 300, 30), "Avatar Models", "WelcomePrompt");

        // GUI area size here
        GUILayout.BeginArea(new Rect(5, 30, 600, 180));

        // switch to horizontal for selection grid layouts
        GUILayout.BeginHorizontal();
        // show heads in rows of up to 7
        _selected = GUILayout.SelectionGrid(_selected, avatarHeadPics, 7, headShotStyle);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (availableSkinPreviews != null)
        {
            if (availableSkinPreviews.Count > 0)
            {
                GUI.Label(new Rect(5, Screen.height - 90, 300, 30), "Outfits", "WelcomePrompt");
                // Show outfit choices - this area controls where those are rendered on screen
                GUILayout.BeginArea(new Rect(5, Screen.height - 60, 600, 60));
                GUILayout.BeginHorizontal();

                _selectedSkin = GUILayout.SelectionGrid(_selectedSkin, availableSkinPreviews.ToArray(), 8, headShotStyle);

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
        if (hasHair)
        {
            GUI.Label(new Rect(5, Screen.height - 260, 300, 30), "Hair", "WelcomePrompt");
            // Show hair choices - this area controls where those are rendered on screen
            GUILayout.BeginArea(new Rect(5, Screen.height - 230, 400, 100));
            GUILayout.BeginHorizontal();

            _selectedHair = GUILayout.SelectionGrid(_selectedHair, hairTextures, 5, headShotStyle);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
		for(int i=0; i<nextLevel.Length; i++)
		{
	        if (Application.CanStreamedLevelBeLoaded(nextLevel[i]))
	        {
	            // Show next level button on screen - position for this button is controlled here
	            if (GUI.Button(new Rect((Screen.width - 100), (Screen.height -(1+i)*25), 100, 25), nextLevelPrompt[i]))
	            {
					authenticationIndex=i;
					Debug.Log("Clicked enter level - opening authenticate box");
	            }
	        }
	        else
	        {
				Debug.Log("Cannot load " + nextLevel[i]);
	            GUI.Label(new Rect((Screen.width - 200), (Screen.height - 25), 200, 25), loadProgress);
	        }
		}
		if(authenticationIndex!=-1)
		{
			currentPasswordAttempt=GUI.TextArea(new Rect((Screen.width - 200), (Screen.height -(1+authenticationIndex)*25), 100, 25), currentPasswordAttempt, 30);
			if(currentPasswordAttempt.Equals(nextLevelPassword[authenticationIndex]))
			{
				Debug.Log("Loading :" + nextLevel[authenticationIndex]);
                if (string.IsNullOrEmpty(localPlayer.Hair) && hairTextures.Length > 0)
                    localPlayer.Hair = hairTextures[0].name;
				indexToLoad=authenticationIndex;
				if(authenticationIndex==1)
				{
					Debug.Log("Joining as patient");
					PlayerPrefs.SetString("Group", "Patient");
				}
				else if(authenticationIndex==0)
				{
					Debug.Log("Joining as therapist");
					PlayerPrefs.SetString("Group", "Therapist");
				}
				else
				{
					Debug.Log("Invalid selection");
				}
                ChangeLevel(); // no network controller - let's try just loading the next level
			}
		}
    }

    void Update()
    {
		if(spawnPlayerNextUpdate)
		{
			spawnPlayerNextUpdate=false;
			SpawnPlayer();
		}
        // Has the user changed avatar selection?
        if (_currentSelection != _selected)
        {
            // Clear out current previews and reset for new model
            availableSkinPreviews.Clear();
            availableSkins.Clear();

            _selectedSkin = 0;
            _selectedHair = 0;
            _currentSelectedSkin = 0;
            _currentSelectedHair = 0;

            _currentSelection = _selected;
            localPlayer.AvatarModel = _currentSelection;
            localPlayer.Hair = string.Empty;

            PopulateSkinsAndHair();

            ChangeMesh();
        }
        // User has changed skin selection - show that change on avatar
        if (_currentSelectedSkin != _selectedSkin)
        {
            _currentSelectedSkin = _selectedSkin;
            ChangeSkin();
        }
        // User has changed hair selection - show that change on avatar
        if (_currentSelectedHair != _selectedHair)
        {
            _currentSelectedHair = _selectedHair;
            ChangeHair();
        }
        // Show particle effect when user changes
        if (poseStand != null)
        {
            if (isEmitting)
            {
                emitTimer = emitTimer + Time.deltaTime;
                if (emitTimer > emissionTime)
                {
                    poseStand.GetComponent<ParticleEmitter>().emit = false;
                    emitTimer = 0.0f;
                    isEmitting = false;
                }
            }
        }

        if (jibeServerInstance == null)
        {
            return;
        }
        jibeServerInstance.Update();
    }

    private void PopulateSkinsAndHair()
    {
        // Requires a naming convention for all available skin options, but place any valid image in the 
        // Resources folder and appropriate subfolders following the examples in there initially and they 
        // will appear and be usable in the dressing room and throughout Jibe
        foreach (Texture2D skinPreviewTex in Resources.LoadAll(GetSkinPreviewFolder(avatarHeadPics[_selected].name), typeof(Texture2D)))
        {
            availableSkinPreviews.Add(skinPreviewTex);
        }
        foreach (Texture2D skinTex in Resources.LoadAll(GetSkinFolder(avatarHeadPics[_selected].name), typeof(Texture2D)))
        {
			Debug.Log("Found skin named: " + skinTex);
            availableSkins.Add(skinTex);
        }
        if (availableHair.Count == 0)
        {
            foreach (Texture2D hairTex in hairTextures)
            {
                availableHair.Add(hairTex);
            }
        }
    }

    // Naming conventions for previews and skins must be followed
    private string GetSkinPreviewFolder(string normalTextureName)
    {
        string skinPreviewFolder = normalTextureName.Substring(0, normalTextureName.IndexOf("Head"));
        return skinPreviewFolder + "/previews";
    }
    private string GetSkinFolder(string normalTextureName)
    {
        string skinFolder = normalTextureName.Substring(0, normalTextureName.IndexOf("Head"));
        return skinFolder + "/skins";
    }

    void OnApplicationQuit()
    {
        jibeServerInstance.Disconnect();
    }

    // Update local player appearance - this will ensure all players in scene will see player as intended when entering main scenes
    public void SendAppearance(string skin, string hair, int avatar)
    {
        localPlayer.Skin = skin;
        localPlayer.Hair = hair;
        localPlayer.AvatarModel = avatar;
    }

    public void SpawnPlayer()
    {
        // Render the player with chosen mesh, skin and hair
        hasHair = false;
        GameObject poseStand = GameObject.FindGameObjectWithTag("SpawnPoint");
        Vector3 spawnPosition = poseStand.transform.position;
        spawnPosition.y = spawnPosition.y + 0.2f;
        UnityEngine.Component localPlayerComponent = Instantiate(playerPrefabs[localPlayer.AvatarModel], spawnPosition, poseStand.transform.rotation) as Component;
        localPlayerComponent.transform.parent = poseStand.transform;

        GameObject character = localPlayerComponent.gameObject;

        // Setup Hair

        for (int i = 0; i < character.transform.childCount; i++)
        {
            if (character.transform.GetChild(i).tag == "Wig")
            {
                hasHair = true;
                if (!RetrieveHairPreference())
                {
                    localPlayer.Hair = character.transform.GetChild(i).GetComponent<Renderer>().material.mainTexture.name;
                    for (int j = 0; j < hairTextures.Length; j++)
                    {
                        if (hairTextures[j].name == localPlayer.Hair)
                        {
                            _currentSelectedHair = j;
                            _selectedHair = j;
                        }
                    }
                }

            }
        }        
        RetrieveClothingPreference();
        ChangeSkin();
		Debug.Log("avatar here is:" + GameObject.FindGameObjectWithTag("Character").name);
        ChangeHair();
    }

    void ChangeMesh()
    {
        if (poseStand != null)
        {
            poseStand.GetComponent<ParticleEmitter>().emit = true;
            isEmitting = true;
        }
        GameObject localPlayerObject = GameObject.FindGameObjectWithTag("Character");
		Debug.Log("Destroying:" + localPlayerObject.name);
        GameObject.Destroy(localPlayerObject);
		Debug.Log(localPlayerObject.name);
        spawnPlayerNextUpdate=true;
    }
    void ChangeSkin()
    {
		Debug.Log("Changing skin");
        string folder = GetSkinFolder(avatarHeadPics[_selected].name);
        string skinName = availableSkins[_currentSelectedSkin].name;
        Texture2D textureToUse = availableSkins[_currentSelectedSkin];

        GameObject character = GameObject.FindGameObjectWithTag("Character");
        if (character != null) // sanity null check
        {
            for (int i = 0; i < character.transform.childCount; i++)
            {
                if (character.transform.GetChild(i).tag == "Skin")
                {
                    Debug.Log("Found my skin!");
                    character.transform.GetChild(i).GetComponent<Renderer>().material.mainTexture = textureToUse;
                    localPlayer.Skin = folder + @"/" + skinName;
                }
            }
        }
		else
		{
			Debug.Log("character is null - wtf?");
		}

    }
    void ChangeHair()
    {
        GameObject character = GameObject.FindGameObjectWithTag("Character");
        if (character != null) // sanity null check
        {
            for (int i = 0; i < character.transform.childCount; i++)
            {
                if (character.transform.GetChild(i).tag == "Wig")
                {
                    Debug.Log("Found my hair!");
                    character.transform.GetChild(i).GetComponent<Renderer>().material.mainTexture = hairTextures[_currentSelectedHair];
                    localPlayer.Hair = hairTextures[_currentSelectedHair].name;
                    Debug.Log(localPlayer.Hair);
                }
            }
        }

    }
    public void ChangeLevel()
    {
        StoreClothingPreference();
        PlayerPrefs.SetInt("avatar", _selected);
        Debug.Log("-- Leaving dressing room --");
        jibeServerInstance.LeaveRoom();
    }

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
            // Just spawn a local player
            Debug.Log(localPlayer.ToString());
            SpawnPlayer();           
        }
        else
        {
            Debug.Log("Room Join FAIL " + e.Message);
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
            Debug.Log("Player has now left level");
            jibeServerInstance.RoomJoinResult -= new RoomJoinResultEventHandler(RoomJoinResult);
            jibeServerInstance.RoomLeaveResult -= new RoomLeaveResultEventHandler(RoomLeaveResult);
            Application.LoadLevel(nextLevel[indexToLoad]);        
        }
    }
    private void StoreClothingPreference()
    {
        // store clothing options in playerprefs
		Debug.Log("Storing skin - my skin is:" + localPlayer.Skin);
		Debug.Log("Storing hair - my hair is:" + localPlayer.Hair);
        PlayerPrefs.SetString("skin", localPlayer.Skin);
        PlayerPrefs.SetString("hair", localPlayer.Hair);
        PlayerPrefs.SetInt("avatar", localPlayer.AvatarModel);
    }

    private void RetrieveClothingPreference()
    {
        // try to re-use previous clothing options
        string skin = PlayerPrefs.GetString("skin");
        if (!string.IsNullOrEmpty(skin))
        {
            Debug.Log("Skin from prefs = " + skin);
            //string folder = GetSkinFolder(avatarHeadPics[_selected].name);
            string trimmedSkinName = skin.Substring(skin.LastIndexOf('/') + 1);
            Debug.Log("Trimmed = " + trimmedSkinName);
            for (int i = 0; i < availableSkins.Count; i++)
            {
                if (availableSkins[i].name == trimmedSkinName)
                {
                    _currentSelectedSkin = _selectedSkin = i;
                    Debug.Log("Setting skin to " + skin);
                    localPlayer.Skin = skin;
                }
            }
        }
    }

    private bool RetrieveHairPreference()
    {
        string hair = PlayerPrefs.GetString("hair");
        if (!string.IsNullOrEmpty(hair))
        {
            for (int i = 0; i < availableHair.Count; i++)
            {
                if (availableHair[i].name == hair)
                {
                    _currentSelectedHair = _selectedHair = i;
                    Debug.Log("Setting hair to " + hair);
                    localPlayer.Hair = hair;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
