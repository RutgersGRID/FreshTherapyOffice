using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Buzzer : MonoBehaviour {
	public Transform linkedBuzzer;
	public string groupWithPermission;
	public NetworkController netController;
	public Texture2D texture1;
	public Texture2D texture2;
	// Use this for initialization
	void Start () {
		netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnMouseDown () {
		Debug.Log("Clicked!");
		if((netController.CheckIfLocalPlayerIsInGroup(groupWithPermission) || netController.CheckIfLocalPlayerIsInGroup("GlobalChat")) && this.transform.GetComponent<Renderer>().material.mainTexture==texture1)
		{
			BuzzerOn();
			Dictionary<string,string> dataToSend = new Dictionary<string, string>();
			dataToSend["SendingObjectName"] = this.transform.name;
			dataToSend["MethodToCall"] = "BuzzerOn";
			netController.SendCustomData(dataToSend);
			dataToSend["SendingObjectName"] = linkedBuzzer.name;
			netController.SendCustomData(dataToSend);
		}
		else if(netController.CheckIfLocalPlayerIsInGroup(groupWithPermission) || netController.CheckIfLocalPlayerIsInGroup("GlobalChat"))
		{
			BuzzerOff();
			Dictionary<string,string> dataToSend = new Dictionary<string, string>();
			dataToSend["SendingObjectName"] = this.transform.name;
			dataToSend["MethodToCall"] = "BuzzerOff";
			netController.SendCustomData(dataToSend);
			dataToSend["SendingObjectName"] = linkedBuzzer.name;
			netController.SendCustomData(dataToSend);
		}
	}
	void BuzzerOn()
	{
		transform.GetComponent<Renderer>().material.mainTexture=texture2;
		linkedBuzzer.GetComponent<Renderer>().material.mainTexture=texture2;
	}
	void BuzzerOff()
	{
		transform.GetComponent<Renderer>().material.mainTexture=texture1;
		linkedBuzzer.GetComponent<Renderer>().material.mainTexture=texture1;
	}
}
