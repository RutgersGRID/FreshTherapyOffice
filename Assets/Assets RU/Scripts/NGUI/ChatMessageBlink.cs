using UnityEngine;
using System.Collections;

public class ChatMessageBlink : MonoBehaviour {
	public UILabel labelToBlink;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void StartBlinking() {
		StartCoroutine("Blink");
	}
	IEnumerator Blink() {
		while(true)
		{
			labelToBlink.color = Color.red;
			yield return new WaitForSeconds(.5f);
			labelToBlink.color = Color.gray;
			yield return new WaitForSeconds(.5f);
		}
	}
	public void StopBlinking() { 
		StopCoroutine("Blink");
	}
}
