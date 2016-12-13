using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
public class Countdown : MonoBehaviour {
	public DateTime endTime;
	public UILabel label;
	public bool done=false;
	NetworkController netController;
	// Use this for initialization
	void Start () {
		netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
		endTime = System.DateTime.Now;
	}
	
	// Update is called once per frame
	void Update () {
		if(!done)
		{
			DateTime today = System.DateTime.Now;
			TimeSpan timeLeft = endTime-today;
			if(timeLeft<=System.TimeSpan.Zero)
			{
				Debug.Log("Done");
				done=true;
			}
			label.text = timeLeft.ToString().Substring(0,timeLeft.ToString().IndexOf('.'));
		}
		else
		{
			label.text = System.TimeSpan.Zero.ToString();
		}
	}
	public void SetEndTime(DateTime newEndTime) {
		endTime = newEndTime;
		done=false;
		Dictionary<string,string> dataToSend = new Dictionary<string,string>();
		dataToSend["MethodToCall"] = "ReceiveEndTime";
		dataToSend["SendingObjectName"] = this.transform.name;
		dataToSend["EndTime"] = newEndTime.ToString();
		string[] usersToDestroyOn = new string[0]; //we want this data to persist as long as anyone is there
		netController.SendCustomData(dataToSend, true, usersToDestroyOn);
	}
	public void ReceiveEndTime(Dictionary<string,string> data)
	{
		done=false;
		endTime = DateTime.Parse(data["EndTime"]);
	}
	public void ToggleVisibility(bool isVisible) //the function the admin calls to change the visibility of the non admins
	{
		Dictionary<string,string> dataToSend = new Dictionary<string, string>();
		dataToSend["SendingObjectName"] = this.name;
		dataToSend["MethodToCall"] = "ChangeVisibility";
		dataToSend["Visible"] = isVisible.ToString();
		string[] usersToDestroyOn = new string[0];
		netController.SendCustomData(dataToSend, true, usersToDestroyOn);
		ChangeVisibility(dataToSend);
	}
	public void ChangeVisibility(Dictionary<string,string> data) //the function that actually changes the visibilty for the non admins
	{
		if(data["Visible"].Equals("True"))
		{
			this.SendMessage("Toggle", true);
		}
		else
		{
			this.SendMessage("Toggle", false);
		}
	}
}
