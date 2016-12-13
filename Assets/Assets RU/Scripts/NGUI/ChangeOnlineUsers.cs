using UnityEngine;
using System.Collections;

public class ChangeOnlineUsers : MonoBehaviour {
	public GameObject managementOnlineUsers;
	public GameObject unionOnlineUsers;
	public GameObject globalOnlineUsers;
	public string myGroup;
	public static bool resetMyLabel=false;
	public bool ignoreNextReset=false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(resetMyLabel)
		{
			if(!ignoreNextReset)
			{
				GetComponentInChildren<UISlicedSprite>().spriteName="LabelBackground";
			}
			ignoreNextReset=false;
		}

	}
	void LateUpdate() {
		resetMyLabel=false;
	}
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			if(myGroup=="Union")
			{
				managementOnlineUsers.SendMessage("Toggle", true);
				globalOnlineUsers.SendMessage("Toggle", true);
				unionOnlineUsers.SendMessage("Toggle", false);
			}
			else if(myGroup=="Management")
			{
				managementOnlineUsers.SendMessage("Toggle", false);
				globalOnlineUsers.SendMessage("Toggle", true);
				unionOnlineUsers.SendMessage("Toggle", true);
			}
			else
			{
				managementOnlineUsers.SendMessage("Toggle", true);
				globalOnlineUsers.SendMessage("Toggle", false);
				unionOnlineUsers.SendMessage("Toggle", true);
			}
			GetComponentInChildren<UISlicedSprite>().spriteName="CurrentChat"; //change my label and unselect all the other labels in the next update
			ignoreNextReset=true;
			resetMyLabel=true;
			managementOnlineUsers.transform.parent.position=new Vector3(managementOnlineUsers.transform.parent.position.x, 3.1f, managementOnlineUsers.transform.parent.position.z);
		}
	}
}
