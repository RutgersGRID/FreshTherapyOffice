using UnityEngine;
using System.Collections;
using System;
public class UpdateEndTime : MonoBehaviour {
	public UILabel inputTimeLabel;
	public UILabel timeLabel;
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
			DateTime newTime;
			bool success = DateTime.TryParse(inputTimeLabel.text, out newTime);
			if(success==true)
			{
				timeLabel.GetComponent<Countdown>().SetEndTime(newTime);
			}
			else
			{
				Debug.Log(inputTimeLabel.text +" is not a valid input string, ending timer");
				inputTimeLabel.text = "Not a valid time";
			}
		}
	}
			
}
