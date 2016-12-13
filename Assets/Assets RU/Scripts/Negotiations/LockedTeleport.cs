using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class LockedTeleport : MonoBehaviour {
	public string groupWithPermission = "";
	public Transform teleportPoint;
	public Transform teleportPoint2;
	private bool showgui=false;	
	public string Vivox1="sip:confctl-31@regp.vivox.com";
	public string Vivox2="sip:confctl-158@regp.vivox.com";
	private int counter = 0;
	public List<GameObject> Clock1;
	public List<GameObject> Clock2;
	public GameObject camera; // need to store the camera because it has the sit script attached to it, which we need to send a message to to fix the sit-teleport bug
	// Use this for initialization
	void Start () {
		camera=GameObject.Find("PlayerCamera");
	}
	void OnMouseDown()
	{
		if(GameObject.Find("NetworkController").GetComponent<NetworkController>().CheckIfLocalPlayerIsInGroup(groupWithPermission) || GameObject.Find("NetworkController").GetComponent<NetworkController>().CheckIfLocalPlayerIsInGroup("GlobalChat"))
		{
			showgui=true;
		}
	}
	// Update is called once per frame
	void Update () {
		if(counter<101)//need to give the groups time to initialize - might change this out of update eventually (make network controller call this)
		{
			counter++;
		}
		
		if(counter==100)
		{
			if(GameObject.Find("NetworkController").GetComponent<NetworkController>().CheckIfLocalPlayerIsInGroup("GlobalChat"))
			{
				Transform tmp1 = teleportPoint;
				teleportPoint=teleportPoint2;
				teleportPoint2=tmp1;
			}
		}
		else if(GameObject.Find("NetworkController").GetComponent<NetworkController>().CheckIfLocalPlayerIsInGroup(groupWithPermission))
		{
			foreach(GameObject currentClock in Clock1)
			{
				currentClock.GetComponent<Renderer>().enabled=false;
			}
			foreach(GameObject currentClock in Clock2)
			{
				currentClock.GetComponent<Renderer>().enabled=true;
			}
		}
	}
	void OnGUI()
	{
		if(showgui)
		{
			if(GUI.Button(new Rect(0,0,100,100), "Teleport"))
			{
				camera.SendMessage("UnSit");
				Debug.Log("Teleporting the local player");
				GameObject.Find("localPlayer").transform.position = teleportPoint.position;
				GameObject.Find("localPlayer").transform.rotation = teleportPoint.rotation;
				GameObject.Find("localPlayer").SendMessage("SnapCameraToDefault"); //so that when we adjust the default transform of the player, we only adjust the player rotation and not the cameras
				GameObject.Find("localPlayer").SendMessage("SetCurrentTransformAsDefault"); //because we are altering the rotation of the player, we do this so that the camera angle doesn't jump back to what it was before we teleported
				FlipValues();
			}
			else if(GUI.Button(new Rect(100,0,100,100), "Don't Teleport"))
			{
				showgui=false;
			}
		}
	}
	void FlipValues()
	{
			Transform filler = teleportPoint;
			teleportPoint=teleportPoint2;
			teleportPoint2=filler;
			showgui=false;
			GameObject.Find("VivoxHud").GetComponent<VivoxHud2>().SwitchToChannel(Vivox1); //toggling vivox =channels
			string tmp = Vivox1;
			Vivox1=Vivox2;
			Vivox2=tmp;
			foreach(GameObject objectToNotify in GameObject.FindGameObjectsWithTag("plaque"))
			{
				objectToNotify.SendMessage("Teleported");
			}
			foreach(GameObject currentClock in Clock1)
			{
				currentClock.GetComponent<Renderer>().enabled=false;
			}
			List<GameObject> fillerClock = Clock1;
			Clock1=Clock2;
			Clock2=fillerClock;
			foreach(GameObject currentClock in Clock1)
			{
				currentClock.GetComponent<Renderer>().enabled=true;
			}
	}
}
