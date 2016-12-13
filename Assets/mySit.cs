using UnityEngine;
using System.Collections;

public class mySit : MonoBehaviour {
	public string sitPose = "sit1";
	public string idle = "idle";
	public float clickDistanceLimit = 15;
	
	public float sitOffsetX = -1f;
	public float sitOffsetY = .5f;
	public float sitOffsetZ = -1f;
	
	private GameObject currentPlayer;
	// Use this for initialization
	private bool isSatOn = false;
	
	
	
	
	public NetworkController networkController; // controls the environment
	
	void Start () {
		if (networkController == null)
		{
			networkController = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
		}
	}
	
	// Update is called once per frame
	void Update () {
        if (isSatOn && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
			currentPlayer.GetComponent<MouseLook>().playerControlOn=true;
			currentPlayer.GetComponent<FPSInputController>().enabled=true;
			currentPlayer.GetComponent<CharacterMotor>().enabled=true;
			isSatOn = false;
			
			
			// bump the player up so they're above the floorish
			
			Vector3 sitPosition = gameObject.transform.position;
			sitPosition.y = 3;
			currentPlayer.transform.position = sitPosition;
			
			// set the animation back to idle
			
			AnimateCharacter tpa = currentPlayer.transform.GetChild(0).GetComponent<AnimateCharacter>();
			tpa.SitAnimation(idle);
			currentPlayer.transform.GetChild(0).GetComponent<PlayerMovement>().SetSitting(true, idle);
			
			
			networkController.SendTransform(currentPlayer.transform);
			networkController.SendAnimation(idle);
			
		}
		
	}
	
	void OnMouseDown () {
		//currentPlayer = GameObject.FindGameObjectWithTag("Player");
        currentPlayer = GameObject.Find("localPlayer");
        if (currentPlayer == null) { Debug.LogError("Unable to find sitee"); return; }

		isSatOn = true;
		
		currentPlayer.GetComponent<FPSInputController>().enabled=false;
		currentPlayer.GetComponent<CharacterMotor>().enabled=false;
		
		
		
		Debug.Log ("sitting");
		
		
		//set sit rotation
		Vector3 sitRotation = gameObject.transform.eulerAngles;
		sitRotation.x = currentPlayer.transform.eulerAngles.x;
		sitRotation.z = currentPlayer.transform.eulerAngles.z;
		currentPlayer.transform.eulerAngles = sitRotation;
		currentPlayer.transform.RotateAround (Vector3.zero, Vector3.up, 90);
		
		
		//set sit position
		Vector3 sitPosition = gameObject.transform.position;
		sitPosition.y += sitOffsetY;
		sitPosition.x += sitOffsetX;
		sitPosition.z += sitOffsetZ;
		currentPlayer.transform.position = sitPosition;
		
		
		//set sit pose
		AnimateCharacter tpa = currentPlayer.transform.GetChild(0).GetComponent<AnimateCharacter>();
		tpa.SitAnimation(sitPose);
		currentPlayer.transform.GetChild(0).GetComponent<PlayerMovement>().SetSitting(true, sitPose);
		
		
		networkController.SendTransform(currentPlayer.transform);
		networkController.SendAnimation(sitPose);
		
		
		currentPlayer.GetComponent<MouseLook>().playerControlOn=false;
		
		
		
	}
}
