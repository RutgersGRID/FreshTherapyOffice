using UnityEngine;
using System.Collections;

public class ClearNotepads : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress(bool isPressed) {
		if(isPressed==true)
		{
			Plaque.cleartext=true;
		}
	}
}
