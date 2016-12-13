using UnityEngine;
using System.Collections;

public class GoToDressingRoom : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			Application.ExternalEval("window.location.reload()");
			//GameObject.Find("NetworkController").GetComponent<NetworkController>().JoinLoader();
		}
	}
}
