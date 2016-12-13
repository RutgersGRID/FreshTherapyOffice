using UnityEngine;
using System.Collections;

public class ChangeGroup : MonoBehaviour {
	public string groupToJoin;
	public ChatInput chatController;
	public NetworkController networkController;
	public bool isPrivateGroup = false;
	public UILabel WindowLabel;
	public string nameToDisplay;
	// Use this for initialization
	void Start () {
		chatController = GameObject.Find("ChatBox").GetComponent<ChatInput>();
		networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
		WindowLabel = GameObject.FindWithTag("WindowLabel").GetComponent<UILabel>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isDown) {
		if(isDown==true)
		{
			chatController.isChattingPrivately=false;
			chatController.currentGroup=groupToJoin;
			networkController.SetCurrentGroup(groupToJoin, networkController.localPlayer.PlayerID);
			chatController.DisableOtherGroups();
			 if(nameToDisplay!=null)
			{
				WindowLabel.text=nameToDisplay;
			}
			else
			{
				WindowLabel.text=groupToJoin;
			}
			GetComponentInChildren<UISlicedSprite>().spriteName="CurrentChat";
			if(isPrivateGroup)
			{
				chatController.isChattingPrivately=true;
			}
			else
			{
				chatController.isChattingPrivately=false;
			}
			this.SendMessage("BlinkMe", false, SendMessageOptions.DontRequireReceiver);
		}
	}
}
