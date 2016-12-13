using UnityEngine;
using System.Collections;

public class DetectFullScreen : MonoBehaviour {
	bool fullscreen=false;
	// Use this for initialization
	void Start () {
		Application.ExternalCall("FullScreen");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnGUI() {
		if(fullscreen)
		{
			if(GUI.Button(new Rect(0,0,100,25), "Exit"))
			{
				Application.ExternalCall("LeaveFullScreen");
			}
		}
	}
	void FullScreen (string isFullscreen) {
		fullscreen=true;
	}
}
