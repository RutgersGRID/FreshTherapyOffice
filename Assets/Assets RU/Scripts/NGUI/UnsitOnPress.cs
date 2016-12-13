using UnityEngine;
using System.Collections;

public class UnsitOnPress : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isPressed) {
		if(isPressed==false)
		{
			GameObject.Find("PlayerCamera").SendMessage("UnSit");
		}
	}
}
