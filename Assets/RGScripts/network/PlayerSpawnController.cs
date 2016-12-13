/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * PlayerSpawnController.cs Revision 1.4.1106.11
 * Controls the creation of avatar models and associated components */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ReactionGrid.Jibe;

public class PlayerSpawnController : MonoBehaviour
{
	public Transform cam;
    public Transform[] playerPrefabs;
	public Transform[] remotePlayerPrefabs;
    public Transform[] spawnPoints;
    public Transform movementPivot;
    public AudioClip spawnSound;
    public float spawnSoundVolume = 0.5f;
    public int spawnRadius = 10;
    public Transform playerCharacter;
    private static System.Random random = new System.Random();
    public Transform playerShadow;
    public Transform remotePlayerMapIcon;
    public Transform localPlayerMapIcon;
    public float mapIconHeight = 500.0f;
    private JibeActivityLog jibeLog;
    public NetworkController netController;
    public GameObject spawnParticles;
    public bool usePhysicalCharacter = false;
    public float remotePlayerNametagFullyVisibleDistance = 10.0f;
    public float remotePlayerNametagFadeDistance = 30.0f;
    public bool enableIdleAnimationWakeup = false;
    public bool enableFlyingAvatars = true;
    public bool showOwnName = true;

    public bool useJibeAvatarControlScripts = true;
    public string[] componentsToDisableForRemotePlayers;
	public Transform onlineUserPrefab;
	public Transform onlineUserParent;
	public Transform adminOnlineUserPrefab;
    /// <summary>
    /// Create an avatar for the local player based on the user preferences gathered from the loader scene and (if used) dressing room
    /// </summary>
    /// <param name="user">An IJibePlayer representing the user</param>
    /// <returns>A GameObject representing the player and all associated scripts, effects, and local camera</returns>
    public GameObject SpawnLocalPlayer(IJibePlayer user)
    {
        string localPlayerName = "localPlayer";

        GameObject localPlayer = GameObject.Find(localPlayerName);
//		print("before");
		Debug.Log("Spawning localPlayer: " + localPlayer);

        //Check if the user has already spawned an avatar - if they have not yet spawned an avatar, this is where the avatar is set up.
        if (localPlayer == null)
        {
			if(PlayerPrefs.HasKey("Group"))
			{
				string groupName = PlayerPrefs.GetString("Group");
				if(groupName.Equals("Therapist"))
				{
                    Debug.Log("therapist spawn is at: x=" + GameObject.Find("therapistSpawn").transform.position.x + " y=" + GameObject.Find("therapistSpawn").transform.position.y + " z=" + GameObject.Find("therapistSpawn").transform.position.z);

					spawnPoints[0].transform.position= GameObject.Find("therapistSpawn").transform.position;
				}
				else if(groupName.Equals("Patient"))
				{
                    Debug.Log("patient spawn is at: x=" + GameObject.Find("patientSpawn").transform.position.x + " y=" + GameObject.Find("patientSpawn").transform.position.y + " z=" + GameObject.Find("patientSpawn").transform.position.z);

					spawnPoints[0].transform.position= GameObject.Find("patientSpawn").transform.position;
                }
                else
                {
                    Debug.Log("Group not found:" + groupName);
                }
			}
            Debug.Log("Spawn is: x=" + spawnPoints[0].transform.position.x + " y=" + spawnPoints[0].transform.position.y + " z=" + spawnPoints[0].transform.position.z);
			Component characterObjectComponent = GameObject.Instantiate(playerPrefabs[user.AvatarModel], spawnPoints[0].transform.position, spawnPoints[0].transform.rotation) as Component;
			GameObject characterObject = characterObjectComponent.gameObject;
			characterObject.name="localPlayer";

            //new working player tags
            characterObject.AddComponent<nameTag>();
            characterObject.GetComponent<nameTag>().SetDisplayName(user);

            //old broken player tags
            /*
            characterObject.AddComponent<BubblePopup>();
			characterObject.GetComponent<BubblePopup>().skin = GameObject.Find("UIBase").GetComponent<UIBase>().skin;
			characterObject.GetComponent<BubblePopup>().SetDisplayName(user);
			characterObject.GetComponent<BubblePopup>().fullyVisibleDistance = remotePlayerNametagFullyVisibleDistance;
			characterObject.GetComponent<BubblePopup>().fadeDistance = remotePlayerNametagFadeDistance;
			characterObject.GetComponent<BubblePopup>().nameTagHeight=0;
             */

//			characterObject.transform.localScale*=.75f;
			if(GameObject.FindWithTag("Skin")!=null)
			{
				Transform skinTransform = GameObject.FindWithTag("Skin").transform;
	            RetrieveClothingPreference(user);
	/*            if (characterObject != null)
	            {
	                for (int i = 0; i < characterObject.transform.childCount; i++)
	                {
	                    if (characterObject.transform.GetChild(i).tag == "Skin")
	                    {
	                        skinTransform = characterObject.transform.GetChild(i);
	                        if (user.Skin != null)
	                        {
	                            Texture2D selectedSkin = (Texture2D)Resources.Load(user.Skin);
	                            skinTransform.renderer.material.mainTexture = selectedSkin;
	                        }
	                    }
	                }
				}*/
				Texture2D selectedSkin = (Texture2D)Resources.Load(user.Skin);
				skinTransform.GetComponent<Renderer>().material.mainTexture=selectedSkin;
				/*print("startif");
	
				//ZPWH
				Component characterObjectComponent = GameObject.Instantiate(playerPrefabs[user.AvatarModel], spawnPoints[0].transform.position, spawnPoints[0].transform.rotation) as Component;
				GameObject characterObject = characterObjectComponent.gameObject;
				print("begina");
				characterObject.name = localPlayerName;
				print("beginb");
				Debug.Log("Change name from: " + characterObject.name);
				characterObject.name = localPlayerName;
	//			Debug.Log("Change name to: " + characterObject.name);
				characterObject.transform.localScale*=.75f;
				
				foreach(Transform currentModel in playerPrefabs)
				{
					Debug.Log(currentModel.name);
					GameObject check = GameObject.Find(currentModel.name);
					if(check!=null)
					{
						Debug.Log("Found a match!");
						Debug.Log(check.transform.localPosition);
						check.transform.localPosition = new Vector3(3.249682e-17f, .03184503f, -.07741547f);
						Debug.Log(check.transform.localPosition);
					}
				}*/
	/*            int n = spawnPoints.Length;
	            // Choose from one of the available spawn points - if many 
	            // users are expected to log in, this can help distribute the 
	            // avatars as they log in so you don't end up with a pile of people
	            Transform spawnPoint = spawnPoints[random.Next(n)];
	
	            // Vary the start position a little more - avatars will 
	            // emerge within the specified spawn radius of the chosen spawn point
	            float offsetX = random.Next(spawnRadius);
	            float offsetZ = random.Next(spawnRadius);
	
	            Vector3 newPosition = spawnPoint.position;
	            newPosition.x = spawnPoint.position.x + offsetX;
	            newPosition.z = spawnPoint.position.z + offsetZ;
	
	            // set up some common variables
	            GameObject characterObject;
	            AnimationSynchronizer aniSync;
	
	            UnityEngine.Component playerCharacterTemplateComponent = Instantiate(playerPrefabs[user.AvatarModel], newPosition, spawnPoint.transform.rotation) as Component;
	            characterObject = playerCharacterTemplateComponent.gameObject;
	            //playerCharacterTemplateComponent.gameObject.AddComponent<AnimationSynchronizer>();
	            characterObject.name = localPlayerName;
	            characterObject.tag = "Player";
				characterObject.transform.localScale*=.75f;
				characterObject.AddComponent<CharacterController>();
				characterObject.AddComponent<FPSInputController>();
				characterObject.AddComponent<MouseLook>();
				GameObject charCam = GameObject.Instantiate(cam) as GameObject;
				charCam.transform.parent=GameObject.Find("localPlayer").transform;
				charCam.transform.localPosition=new Vector3(0,2.3f,-4.3f);
				charCam.transform.localRotation= new Quaternion(18,0,0,1);//RSO
	            if (characterObject.GetComponent<PlayerMovement>() == null)
	            {
	                characterObject.AddComponent<PlayerMovement>();
	                Debug.Log("PlayerMovement script added");
	            }
	
	            aniSync = characterObject.GetComponent<AnimationSynchronizer>();
	            if (aniSync == null)
	            {
	                aniSync = characterObject.AddComponent<AnimationSynchronizer>();
	                Debug.Log("AnimationSynchronizer script added");
	            }
	
	            if (characterObject.GetComponent<AnimateCharacter>() == null)
	            {
	                characterObject.GetComponent<PlayerMovement>().SendTransforms(true);
	                Debug.Log("PlayerMovement script set to send transforms");
	            }
	
	            GameObject cameraTarget = GameObject.FindGameObjectWithTag("CameraTarget");
	            if (cameraTarget == null)
	            {
	                cameraTarget = new GameObject();
	                cameraTarget.name = "CameraTarget";
	                cameraTarget.tag = "CameraTarget";
	                cameraTarget.transform.parent = characterObject.transform;
	                Vector3 pos = new Vector3(0, 1.9f, 0);
	                cameraTarget.transform.localPosition = pos;
	                Debug.Log("CameraTarget configured");
	            }
	
	            ShowPreviewCamera(false);
	            GameObject sceneCamera = GameObject.Find("PlayerCamera");
	            Vector3 newLocalPosition = sceneCamera.transform.localPosition;
	            sceneCamera.transform.parent = characterObject.transform;
	            sceneCamera.transform.localPosition = newLocalPosition;
	
	            sceneCamera.GetComponent("PlayerCamera").SendMessage("SetTarget", sceneCamera.transform.parent.transform);
	            sceneCamera.GetComponent("PlayerCamera").SendMessage("SetupCamera");
	            sceneCamera.SendMessage("SetTarget", cameraTarget.transform);
	            Debug.Log("PlayerCamera configured");
	
	            AddSpotShadow(characterObject);
	            AddMapIcon(characterObject, localPlayerMapIcon);
	
	            // The following code creates a movement pivot, and is used by the movement + camera controls
	            Vector3 pivotLocation = newPosition;
	            pivotLocation.y = pivotLocation.y + 1;
	            UnityEngine.Component localPlayerPivot = Instantiate(movementPivot, newPosition, spawnPoint.transform.rotation) as Component;
	            localPlayerPivot.name = "PlayerMovementPivot";
	            // Set the PlayerCamera and PlayerCharacter scripts to coordinate with the movement pivot of the player
	            localPlayerPivot.SendMessage("SetTarget", characterObject.transform);
	            sceneCamera.SendMessage("SetMovementPivot", localPlayerPivot.transform);
	            sceneCamera.SendMessage("SetupCamera");
	            sceneCamera.camera.enabled = true;
	            Debug.Log("MovementPivot configured");
	
	            // Call a function called ConfigurePlayerCharacter in PlayerCharacter that sets a flag that the user is now "ready"
	            // so the character can now be controlled by movement keys.
	
	            PlayerCharacter charController = characterObject.GetComponent<PlayerCharacter>();
	            if (charController == null)
	            {
	                charController = characterObject.AddComponent<PlayerCharacter>();
	            }
	
	            charController.ConfigurePlayerCharacter(usePhysicalCharacter, enableFlyingAvatars);
	            Debug.Log("PlayerCharacter setup");
	
	
	            Debug.Log("Loading skin preferences");
	            Transform skinTransform;
	
	            // Set appearance based on clothing and hair texture choices.
	            RetrieveClothingPreference(user);
	            if (characterObject != null)
	            {
	                for (int i = 0; i < characterObject.transform.childCount; i++)
	                {
	                    if (characterObject.transform.GetChild(i).tag == "Skin")
	                    {
	                        skinTransform = characterObject.transform.GetChild(i);
	                        if (user.Skin != null)
	                        {
	                            Texture2D selectedSkin = (Texture2D)Resources.Load(user.Skin);
	                            skinTransform.renderer.material.mainTexture = selectedSkin;
	                        }
	                    }
	                }
	                for (int i = 0; i < characterObject.transform.childCount; i++)
	                {
	                    if (characterObject.transform.GetChild(i).tag == "Wig")
	                    {
	                        Debug.Log("Found my hair!");
	                        if (!string.IsNullOrEmpty(user.Hair))
	                        {
	                            characterObject.transform.GetChild(i).renderer.material.mainTexture = (Texture2D)Resources.Load(user.Hair);
	                        }
	                        else
	                        {
	                            user.Hair = characterObject.transform.GetChild(i).renderer.material.mainTexture.name;
	                        }
	                    }
	                }
	
	                AnimateCharacter ac = characterObject.GetComponent<AnimateCharacter>();
	                Component tpa = characterObject.GetComponent("ThirdPersonAnimation");
	                if (tpa != null)
	                {
	                    tpa.SendMessage("DisableTPA");
	                }
	                if (ac == null)
	                {
	                    ac = characterObject.AddComponent<AnimateCharacter>();
	                }
	                ac.EnableIdleWakeup(enableIdleAnimationWakeup);
	
	                // Check for existence of the UIBase game object and send a message to a script in the camera
	                // which can optionally be used to disable the UI.
	                GameObject gui = GameObject.Find("UIBase");
	                if (gui)
	                {
	                    Camera.main.SendMessage("SetGuiParent", gui);
	                }
	                if (GameObject.Find("UIBase") != null)
	                {
	                    if (characterObject.GetComponent<NameTag>() == null)
	                    {
	                        characterObject.AddComponent<NameTag>();
	
	                    }
	                    if (characterObject.GetComponent<NameTag>() != null)
	                    {
	
	                        characterObject.GetComponent<NameTag>().skin = GameObject.Find("UIBase").GetComponent<UIBase>().skin;
	                        characterObject.GetComponent<NameTag>().localPlayer = user;
	                        characterObject.GetComponent<NameTag>().showNameTag = showOwnName;
	                    }
	                    Debug.Log("NameTag added and setup");
	                }
	
	                Debug.Log("About to start sending animation messages");
	                aniSync.StartSending();
	                // If there's a minimap camera in the scene, now's the time to tell it to follow the local player transform
	                // so the map scrolls smoothly to show an overhead view of your immediate surroundings
	                GameObject miniMapCam = GameObject.Find("MiniMapCamera");
	                if (miniMapCam != null)
	                    miniMapCam.GetComponent<MiniMapFollowCamera>().SetTarget(characterObject.transform);
	                ShowSpawnParticles(newPosition, spawnPoint.transform.rotation);
	
	
	                // Player has now joined the scene - send a message to Network Controller that will appear in chat
	                string message = user.Name + " has joined";
	                netController.SendChatMessage(message);
	            }
	
	            // If the application is set up to use database logging, track the event of the user entering the room
	            if (jibeLog == null)
	            {
	                jibeLog = GameObject.Find("Jibe").GetComponent<JibeActivityLog>();
	                if (jibeLog != null)
	                {
	                    Debug.Log("Found jibeLog");
	                }
	            }
	            if (jibeLog.logEnabled)
	            {
	                Debug.Log("Logging user enter room");
	                Debug.Log(jibeLog.TrackEvent(JibeEventType.Login, Application.absoluteURL, 0.0f, 0.0f, 0.0f, user.PlayerID.ToString(), user.Name, "User entered room "));
	            }
	        */
			//print("after");
			}
		}
        else // There is already an existing local player object
        {
			print("start else");

            // USER HAS RECONNECTED
            GameObject characterObject = GameObject.FindGameObjectWithTag("Character");
            Debug.Log("PLAYER " + user.Name + " RECONNECTED!");
            if (characterObject != null) // sanity check in case this is null
            {
                if (!characterObject.GetComponent<PlayerMovement>().IsSitting())
                {
                    characterObject.SendMessage("WakeUp");
                }
            }
            else
            {
                Debug.Log("Nasty null - where's the character gone?");
            }
            if (localPlayer != null)
            {
                // Re-send our location to help sync up remote viewers
                netController.SendTransform(localPlayer.transform);
            }
            else
            {
                Debug.Log("Nasty null - where's the localPlayer gone?");
            }
        }
		GameObject plaqueParent = GameObject.Find("Plaque");
		if(plaqueParent!=null)
		{
			plaqueParent.BroadcastMessage("OnPlayerJoin");
		}
        // Return a reference to the newly-created Local Player avatar game object to Network Controller
		//Debug.Log("Done");
//		addToOnlineUsers(user.Name, user.PlayerID);
        return GameObject.FindGameObjectWithTag("Player");
    }

    /// <summary>
    /// Create a Remote Player object representing a user in the scene
    /// </summary>
    /// <param name="player">An IJibePlayer representing the remote user to be rendered in the scene</param>
    /// <returns>A refrence to the newly-created Remote Player object - this reference is very important, 
    /// since when the player leaves, the avatar will be removed.</returns>
    public GameObject SpawnRemotePlayer(IJibePlayer player)
    {
        // the convention to use for a remote player game object name internally
        string remotePlayerName = netController.RemotePlayerGameObjectPrefix + player.PlayerID;

        GameObject remotePlayer = GameObject.Find(remotePlayerName);

        if (remotePlayer == null)
        {
            int n = spawnPoints.Length;
            Transform spawnPoint = spawnPoints[random.Next(n)];
            // adjust remote spawn position to be close to (x and z) the main spawn point, but sufficiently out of scope in y that the player will just appear at the correct location without floating into position.
            Vector3 remoteSpawnPosition = new Vector3(spawnPoint.position.x, 10000, spawnPoint.position.z);
            Quaternion remoteSpawnRot = spawnPoint.rotation;
            int avatarId = player.AvatarModel;

            Debug.Log("Spawning Remote Player, remote avatar is type " + avatarId);

            // Instantiate a new Remote Player object for the remote user
            UnityEngine.Component remotePlayerComponent = Instantiate(remotePlayerPrefabs[avatarId], remoteSpawnPosition, remoteSpawnRot) as UnityEngine.Component;

            if (remotePlayerComponent == null)
            {
                throw new Exception("Something wrong with remote player spawn!");
            }
            // Add all required scripts
            remotePlayer = remotePlayerComponent.gameObject;
            remotePlayer.tag = "RemotePlayer";
//			remotePlayer.transform.localScale*=.8f; //RSO
            foreach (string componentToRemove in componentsToDisableForRemotePlayers)
            {
                if (remotePlayer.GetComponent(componentToRemove) != null)
                    (remotePlayer.GetComponent(componentToRemove) as Behaviour).enabled = false;
            }
            remotePlayer.AddComponent<AnimationSynchronizer>();
            remotePlayer.AddComponent<NetworkReceiver>();

            //new player nameplates
            remotePlayer.AddComponent<nameTag>();
            remotePlayer.GetComponent<nameTag>().SetDisplayName(player);



            //old broken player tags 

           /* remotePlayer.AddComponent<BubblePopup>();
            remotePlayer.GetComponent<BubblePopup>().skin = GameObject.Find("UIBase").GetComponent<UIBase>().skin;
            remotePlayer.GetComponent<BubblePopup>().SetDisplayName(player);
            remotePlayer.GetComponent<BubblePopup>().fullyVisibleDistance = remotePlayerNametagFullyVisibleDistance;
            remotePlayer.GetComponent<BubblePopup>().fadeDistance = remotePlayerNametagFadeDistance;
            */


            //Give remote player game object a name like "remote_<id>"
            remotePlayer.name = remotePlayerName;

            // The remote player doesn't need a Third Person Animation component, so disable this script when the player is a remote player
            remotePlayer.SendMessage("DisableTPA", SendMessageOptions.DontRequireReceiver);
            if (remotePlayer.GetComponent<AnimateCharacter>() != null)
            {
                remotePlayer.GetComponent<AnimateCharacter>().DisableAC();
            }
            //Start receiving transform synchronization messages
            remotePlayer.GetComponent<NetworkReceiver>().StartReceiving();
            remotePlayer.GetComponent<AnimationSynchronizer>().StartReceiving();

            // Adjust appearance for remote user
            if (!string.IsNullOrEmpty(player.Skin))
            {
                Debug.Log("Remote player has skin: " + player.Skin);
                Texture2D remoteTexture = (Texture2D)Resources.Load(player.Skin);

                if (remoteTexture != null)
                {
                    try
                    {
                        remotePlayer.GetComponent<NetworkReceiver>().SetSkinRemote(remoteTexture);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Something very wrong setting remote skin texture " + remoteTexture.name + " on player " + remotePlayerName + ", " + ex.Message + ex.StackTrace);
                    }
                }
            }
            if (!string.IsNullOrEmpty(player.Hair))
            {
                Debug.Log("Remote player has hair: " + player.Hair);
                Texture2D remoteTexture = (Texture2D)Resources.Load(player.Hair);

                if (remoteTexture != null)
                {
                    try
                    {
                        remotePlayer.GetComponent<NetworkReceiver>().SetHairRemote(remoteTexture);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Something very wrong setting remote hair texture " + remoteTexture.name + " on player " + remotePlayerName + ", " + ex.Message + ex.StackTrace);
                    }
                }
            }

            AddSpotShadow(remotePlayer);
            AddMapIcon(remotePlayer, remotePlayerMapIcon);
        }
        else
        {
            Debug.Log("Remote Player " + player.Name + " already exists!! Reusing.");
        }

        // Return the remote player game object
		//GameObject.Find("namePlaque30").SendMessage("StartSync");
		netController.Synchronize();
		//RSO add to online users list
		addToOnlineUsers(player.Name, player.PlayerID);
        return remotePlayer;
    }
	private void addToOnlineUsers(string name, int id)
	{
		Transform newOnlineUser;
		Debug.Log("Adding:" + name + "to online users");
		if(!netController.isAdmin)
		{
			newOnlineUser = GameObject.Instantiate(onlineUserPrefab) as Transform;
		}
		else
		{
			newOnlineUser = GameObject.Instantiate(adminOnlineUserPrefab) as Transform;
			newOnlineUser.GetComponentInChildren<Mute>().name=name;
			newOnlineUser.GetComponentInChildren<Mute>().id=id;
		}
		newOnlineUser.parent=onlineUserParent;
		newOnlineUser.name=""+id;
		newOnlineUser.localPosition=new Vector3(newOnlineUser.localPosition.x,newOnlineUser.localPosition.y, -17f);
		newOnlineUser.GetComponentInChildren<UILabel>().text=name;
		onlineUserParent.GetComponent<UIGrid>().repositionNow=true;
	}
	private void AddMapIcon(GameObject player, Transform icon)
    {
        UnityEngine.Component playerMapIconObject = Instantiate(icon, player.transform.position, icon.transform.rotation) as UnityEngine.Component;
        playerMapIconObject.transform.parent = player.transform;
        Vector3 mapIconPosition = playerMapIconObject.transform.position;
        mapIconPosition.y = mapIconPosition.y + mapIconHeight;
        playerMapIconObject.transform.position = mapIconPosition;
    }

    private void AddSpotShadow(GameObject player)
    {
        // Add a spot shadow for the player
        Vector3 shadowSpawnPoint = player.transform.position;
        shadowSpawnPoint.y = shadowSpawnPoint.y + 2;

        UnityEngine.Component remotePlayerShadowObject = Instantiate(playerShadow, shadowSpawnPoint, playerShadow.rotation) as UnityEngine.Component;
        remotePlayerShadowObject.transform.parent = player.transform;
    }

    /// <summary>
    /// Called when a new player is created
    /// </summary>
    public void PlaySpawnSound()
    {
        GameObject guiObject = GameObject.Find("UIBase");
        if (guiObject != null)
        {
            float playVolume = spawnSoundVolume * guiObject.GetComponent<UIBase>().volume / guiObject.GetComponent<UIBase>().audioIcons.Length;
            AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position, playVolume);
        }
    }
    /// <summary>
    /// Show pretty particles when the avatar is spawned, but only if a GameObject for a particle effect has been assigned in the inspector
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void ShowSpawnParticles(Vector3 pos, Quaternion rot)
    {
        if (spawnParticles != null)
        {
            Instantiate(spawnParticles, pos, rot);
        }
    }

    private void ShowPreviewCamera(bool enabled)
    {
        GameObject previewCamera = GameObject.Find("PreviewCameraForDesignModeOnly");

        if (previewCamera != null)
        {
            Debug.Log("Preview Camera found, setting active to " + enabled);
            previewCamera.GetComponent<PreviewCamera>().SetActive(enabled);
        }
    }

    private void RetrieveClothingPreference(IJibePlayer localPlayer)
    {
        // try to re-use previous clothing options
        string skin = PlayerPrefs.GetString("skin");
        if (!string.IsNullOrEmpty(skin))
        {
            if (playerPrefabs[localPlayer.AvatarModel].name.Substring(0, 2).ToLower() == skin.Substring(0, 2).ToLower())
                localPlayer.Skin = skin;
        }

        string hair = PlayerPrefs.GetString("hair");
        if (!string.IsNullOrEmpty(hair))
            localPlayer.Hair = hair;
    }
}
