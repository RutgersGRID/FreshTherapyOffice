using UnityEngine;
using System.Collections;

public class ViewCountdown : MonoBehaviour {
	public Countdown countDown;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnActivate(bool isPressed)
	{
		countDown.ToggleVisibility(!isPressed);
	}
}
