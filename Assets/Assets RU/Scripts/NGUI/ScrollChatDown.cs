using UnityEngine;
using System.Collections;

public class ScrollChatDown : MonoBehaviour {
	public ChatInput chatController;
	// Use this for initialization
	void Start () {
		chatController = GameObject.Find("ChatBox").GetComponent<ChatInput>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isPressed) {
		if(isPressed==true)
		{
			StartCoroutine("DoScrolling");
		}
		else
		{
			StopCoroutine("DoScrolling");
		}
	}
	
	IEnumerator DoScrolling() {
		while(true)
		{
			chatController.textList[chatController.currentGroup].SendMessage("OnScroll", -.1f);
			yield return new WaitForSeconds(.1f);
		}			
	}
}
