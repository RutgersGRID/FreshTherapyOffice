using UnityEngine;
using System.Collections;

public class InitiatePrivateChat : MonoBehaviour {
	public ChatInput chatController;
	// Use this for initialization
	void Start () {
//		chatController = GameObject.Find("ChatBox").GetComponent<ChatInput>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			GameObject.Find("ChatToggle").GetComponent<ToggleButton>().Toggle(false);
		//	chatController.InitiatePrivateChat(this.transform.name, GetComponentInChildren<UILabel>().text);
		}
	}
}
