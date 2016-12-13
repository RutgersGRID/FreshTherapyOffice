using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour {
	public UILabel timeLabel;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isPressed) {
		if(isPressed==true)
		{
			timeLabel.GetComponent<Countdown>().SetEndTime(System.DateTime.Now);
		}
	}
}
