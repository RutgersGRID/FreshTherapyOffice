using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class RaiseHand : MonoBehaviour {
	public UILabel myLabel;
	public bool isHandRaised = false;
	public NetworkController netController;
	public string myName;
	public List<string> handRaisers; //list of people who have their hand raised
	void JibeInit()
	{
		myLabel = GetComponentInChildren<UILabel>();
		netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
		myName = netController.localPlayer.Name;
	}
	
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			isHandRaised=!isHandRaised;
			DoRaising(isHandRaised);
		}
	}
	
	void DoRaising(bool raiseHand)
	{
		if(myName=="")
		{
			myName = netController.localPlayer.Name;
		}
		if(raiseHand)
		{
			myLabel.text="Unraise Hand";
			handRaisers.Add(myName);
		}
		else
		{
			myLabel.text="Raise Hand";
			handRaisers.Remove(myName);
		}
		Application.ExternalCall("RaiseHand", myName, raiseHand);
		Dictionary<string,string> dataToSend = new Dictionary<string, string>();
		dataToSend["SendingObjectName"] = this.transform.name;
		dataToSend["MethodToCall"] = "DoRaising";
		dataToSend["Name"] = myName;
		dataToSend["RaiseHand"] = "" + raiseHand;
		netController.SendCustomData(dataToSend, true, new string[0], true);
	}
	
	public void DoRaising(Dictionary<string,string> data)
	{
		Application.ExternalCall("RaiseHand", data["Name"], data["RaiseHand"]);
		if(data["RaiseHand"]=="True")
		{
			handRaisers.Add(data["Name"]);
		}
		else
		{
			handRaisers.Remove(data["Name"]);
		}
	}
	
	public void UpdateHandRaisers()
	{
		foreach(string handRaiser in handRaisers)
		{
			Application.ExternalCall("RaiseHand", handRaiser, true);
		}
	}
}
