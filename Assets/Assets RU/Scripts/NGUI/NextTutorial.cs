using UnityEngine;
using System.Collections;

public class NextTutorial : MonoBehaviour {
	public Tutorial tutorial;
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
			tutorial.Next();
		}
	}
}
