/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * Sit.cs Revision 1.4.1107.20
 * Enables an avatar to sit on items designated to be sittable */

using UnityEngine;
using System.Collections;
using ReactionGrid.Jibe;

public class Sit : MonoBehaviour
{
    public string sitPose = "sit1";
    public string groundSitPose = "groundsit";
    private bool isSitting = false;
    private bool isHovering = false;
    public float clickDistanceLimit = 15;
    public float sitTargetOffsetX = 0.0f;
    public float sitTargetOffsetY = 0.0f;
    public float sitTargetOffsetZ = 0.0f;
	public Vector3 unsitOffset = Vector3.zero;
    public string sitText = "sit";
    public string groundSitText = "Sit";
    public bool enableGroundSit = true;

    public Texture2D sitIcon;
    public string standText = "stand";

    public GUISkin skin;

    public bool ShowStandButton = false;

    private Vector3 sitTarget;
    private Quaternion sitRotation;

    private GameObject currentPlayer; // contains movement controls
    public NetworkController networkController; // controls the environment
    public ChatInput chatController; // controls the chat system
    private GameObject playerCam;

    public GameObject cursor;

    private GameObject currentChair;

    private JibeActivityLog jibeLog;
    private string username;
    private string userId;

    private float playerCapsuleRadius = 0.2f;
    private float playerCapsuleHeight = 1.6f;

    private bool showTerrainSit = false;
    private Vector2 terrainSitPosition;
    private Vector3 terrainSitCoordinates;
    private float groundSitShowDuration = 2.0f;
    private float groundSitShowElapsed = 0.0f;
	//used in case another function that takes the same screenspace as the sit screenspace needs to override sitting
	private int sitNextFrame=-1;
	public GameObject standUpButton;
    void Start()
    {
        // Initialize static values
        sitTarget = transform.position;
        sitRotation = transform.rotation;
        if (cursor == null)
            cursor = GameObject.Find("Cursor");
        if (networkController == null)
        {
            networkController = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
        }
        if (chatController == null)
        {
            //chatController = GameObject.Find("ChatBox").GetComponent<ChatInput>();
        }
    }



    void Update()
    {
		if(sitNextFrame==0)
		{
			InitiateSit();
		}
        if (!isSitting)
        {
            DetectMousePosition();
        }
        if (isHovering)
        {
            cursor.SendMessage("ShowSitCursor", true, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            cursor.SendMessage("ShowSitCursor", false, SendMessageOptions.DontRequireReceiver);
        }
        if (showTerrainSit)
        {
            groundSitShowElapsed += Time.deltaTime;
            if (groundSitShowElapsed > groundSitShowDuration)
            {
                showTerrainSit = false;
                groundSitShowElapsed = 0.0f;
            }
        }
		sitNextFrame--;
    }

    private void DetectMousePosition()
    {
        // Can't do this with MouseOver / MouseExit since the camera could be disabled
        // and mouseover doesn't play nice if that has happened!
        if (playerCam == null) playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        if (playerCam != null)
        {
            if (playerCam.GetComponent<Camera>().enabled)
            {
                Ray mouseRay = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(mouseRay, out hit, clickDistanceLimit))
                {
                    //Debug.Log(hit.transform.name);


                    if (hit.transform.tag == "SitTarget")
                    {
                        // Player is hovering over a sittable object - one tagged as a "SitTarget". 
                        // NOTE A sittable object MUST contain a collider (even a simple trigger would do) for this (raycasting) to work.
                        if(!Test.isHitting)
						{
							isHovering = true;
						}
						else
						{
							isHovering=false;
						}
                        sitTarget = hit.transform.position;
                        sitRotation = hit.transform.rotation;
                        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftAlt) && !Test.isHitting)
                        {
                            currentChair = hit.transform.gameObject;                            
                            sitNextFrame=5;
							//InitiateSit();
                        }
                    }
                    else if (enableGroundSit && hit.transform.name == "Terrain")
                    {
                        terrainSitCoordinates = hit.point;
                        if (Input.GetMouseButtonDown(1))
                        {
                            terrainSitPosition = playerCam.GetComponent<Camera>().WorldToScreenPoint(terrainSitCoordinates);
                            showTerrainSit = true;
                        }
                    }
                    else
                    {
                        isHovering = false;
                    }	
                }
				else
				{
					isHovering=false;
				}
            }
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;

        if (isSitting)
        {
            // To stand up, the player moves forward or back while not chatting
            if (Input.GetButton("Vertical") || (Input.GetMouseButton(0) && Input.GetMouseButton(1)) || Input.GetButton("Jump"))
            {
                if (!chatController.IsChatting())
                {
                    UnSit();
                }
            }
            if (ShowStandButton)
            {
                // An alternative - show a button so the player can stand up when they are finished
                if (GUI.Button(new Rect(Screen.width/2-50,Screen.height-50,100,25), standText))
                {
                    UnSit();
                }
            }
        }
        if (showTerrainSit)
        {
            Vector2 pos = GUIUtility.ScreenToGUIPoint(new Vector2(terrainSitPosition.x, Screen.height - terrainSitPosition.y));
            GUIStyle buttonStyle = new GUIStyle("Button");
            buttonStyle.fixedWidth = 50;
            if (GUI.Button(new Rect(pos.x, pos.y, 50, 20), groundSitText, buttonStyle))
            {
                InitiateGroundSit();
            }
        }
    }

    private void InitiateGroundSit()
    {
        // update localPlayer
        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        // Sit on the chair/object
        isSitting = true;
        isHovering = false;
        showTerrainSit = false;
        groundSitShowElapsed = 0.0f;
        SitAtPosition(terrainSitCoordinates, currentPlayer.transform.rotation, groundSitPose);
    }

    private void InitiateSit()
    {
		if(!Test.isHitting)
		{
	        // update localPlayer
	        currentPlayer = GameObject.FindGameObjectWithTag("Player");       
	        // Sit on the chair/object
	        isSitting = true;
	        isHovering = false;
	        cursor.SendMessage("ShowSitCursor", false);
	        standUpButton.SendMessage("Toggle", false);	
	        // Move the player to the correct position and rotation
	        float extraOffsetX = 0.0f;
	        float extraOffsetY = 0.0f;
	        float extraOffsetZ = 0.0f;
	        if (currentChair.GetComponent<SitOffset>() != null)
	        {
	            extraOffsetX = currentChair.GetComponent<SitOffset>().SitOffsetX;
	            extraOffsetY = currentChair.GetComponent<SitOffset>().SitOffsetY;
	            extraOffsetZ = currentChair.GetComponent<SitOffset>().SitOffsetZ;
	            sitPose = currentChair.GetComponent<SitOffset>().SitPose;
	        }
	        Vector3 sitPosition = sitTarget;
	        sitPosition.x = sitTarget.x + sitTargetOffsetX + extraOffsetX;
	        sitPosition.y = sitTarget.y + sitTargetOffsetY + extraOffsetY;
	        sitPosition.z = sitTarget.z + sitTargetOffsetZ + extraOffsetZ;
			sitRotation = Quaternion.Euler(sitRotation.eulerAngles + currentChair.GetComponent<SitOffset>().offsetRotation.eulerAngles);
	        SitAtPosition(sitPosition, sitRotation, sitPose);
	
	        try
	        {
	            if (currentChair != null)
	            {
	                if (currentChair.GetComponent<ChairController>() != null)
	                {
	                    currentChair.GetComponent<ChairController>().Sit();
	                }
	            }
	        }
	        catch
	        {
	            Debug.Log("No sit camera script on this chair");
	        }
			currentPlayer.SendMessage("SnapCameraToDefault");
			currentPlayer.SendMessage("GetSitRotation", sitRotation);
		}
    }

    private void SitAtPosition(Vector3 targetPosition, Quaternion targetRotation, string animationToPlay)
    {		
        // Disable movement controls
//        ((Behaviour)currentPlayer.GetComponent<PlayerCharacter>()).enabled = false;
		currentPlayer.GetComponent<FPSInputController>().enabled=false;
		currentPlayer.GetComponent<CharacterMotor>().enabled=false;
		currentPlayer.GetComponent<MouseLook>().playerControlOn=false;
        // If we're using physical characters, turn off the RB component
/*        PlayerSpawnController psc = networkController.GetComponent<PlayerSpawnController>();
        if (psc != null)
        {
            if (psc.usePhysicalCharacter)
            {
                currentPlayer.GetComponent<Rigidbody>().useGravity = false;
                playerCapsuleRadius = currentPlayer.GetComponent<CapsuleCollider>().radius;
                playerCapsuleHeight = currentPlayer.GetComponent<CapsuleCollider>().height;
                currentPlayer.GetComponent<CapsuleCollider>().radius = 0.01f;
                currentPlayer.GetComponent<CapsuleCollider>().height = 0.01f;
            }
        }*/
//		currentPlayer.GetComponent<CharacterController>().radius=.1f;
//		currentPlayer.GetComponent<CharacterController>().height=.1f;
        currentPlayer.transform.position = targetPosition;
        // Note there is no simple way to control the rotation via user settings in inspector. Best approach, adjust the collider on the 
        // sittable object to the correct orientation. This could mean adding an empty cube collider to a chair and rotating to the correct
        // angle for the avatar to face the correct way.
        currentPlayer.transform.rotation = targetRotation;

        // Play the selected sit pose
        AnimateCharacter tpa = currentPlayer.transform.GetChild(0).GetComponent<AnimateCharacter>();
        tpa.SitAnimation(animationToPlay);
        tpa.EnableIdleWakeup(false); // never want an idle wake up while sitting
        currentPlayer.transform.GetChild(0).GetComponent<PlayerMovement>().SetSitting(true, animationToPlay);

        Debug.Log("Sitting: " + isSitting);

        networkController.SendTransform(currentPlayer.transform);
        networkController.SendAnimation(animationToPlay);
        TrackSit();
    }

    private void TrackSit()
    {
        // Log the sit interaction to the database, if the activity log is enabled.
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
            if (string.IsNullOrEmpty(username))
            {
                username = networkController.GetMyName();
            }
            IJibePlayer localPlayer = networkController.GetLocalPlayer();
            string playerId = localPlayer.PlayerID.ToString();
            if (PlayerPrefs.GetString("useruuid") != null)
                playerId = PlayerPrefs.GetString("useruuid");
            Debug.Log(jibeLog.TrackEvent(JibeEventType.Sit, Application.srcValue, this.transform.position.x, this.transform.position.y, this.transform.position.z, playerId, username, "User Sat on Chair"));
        }
    }

    public void UnSit()
    {
        standUpButton.SendMessage("Toggle", true);
        // Cancel sit
        try
        {
            if (currentChair != null)
            {
                currentChair.GetComponent<ChairController>().UnSit();
            }
        }
        catch
        {
            Debug.Log("No sit camera script on this chair");
        }
        AnimateCharacter tpa = currentPlayer.transform.GetChild(0).GetComponent<AnimateCharacter>();
        tpa.CancelSitAnimation();
        bool idleAnimWakeupEnabledByDefault = networkController.gameObject.GetComponent<PlayerSpawnController>().enableIdleAnimationWakeup;
        tpa.SendMessage("EnableIdleWakeup", idleAnimWakeupEnabledByDefault); // only re-enable if enabled in PSC

        // If we're using physical characters, turn off the RB component
/*        PlayerSpawnController psc = networkController.GetComponent<PlayerSpawnController>();
        if (psc != null)
        {
            if (psc.usePhysicalCharacter)
            {
                currentPlayer.GetComponent<Rigidbody>().useGravity = true;
                currentPlayer.GetComponent<CapsuleCollider>().radius = playerCapsuleRadius;
                currentPlayer.GetComponent<CapsuleCollider>().height = playerCapsuleHeight;
            }
        }*/
		currentPlayer.GetComponent<CharacterController>().radius=.5f;
		currentPlayer.GetComponent<CharacterController>().height=2f;
        currentPlayer.transform.GetChild(0).GetComponent<PlayerMovement>().SetSitting(false, sitPose);
        // Re-enable movement
//        ((Behaviour)currentPlayer.transform.GetChild(0).GetComponent<PlayerCharacter>()).enabled = true;
		currentPlayer.GetComponent<FPSInputController>().enabled=true;
		currentPlayer.GetComponent<CharacterMotor>().enabled=true;
		currentPlayer.GetComponent<MouseLook>().playerControlOn=true;
		//currentPlayer.GetComponent<MouseLook>().SetCurrentTransformAsDefault();
		currentPlayer.transform.Translate(unsitOffset);
		currentPlayer.GetComponent<MouseLook>().SetCurrentTransformAsDefault();
        isSitting = false;
        Debug.Log("Sitting: " + isSitting);
    }
	public void DisableSitThisFrame()
	{
		sitNextFrame=-1;
	}
}
