using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class Plaque : MonoBehaviour {
	bool selected=false;
	private string oldContent;
	string lastSyncTime;
	public string groupThatCanRead = "";
	private NetworkController netController;
	public UILabel inputLabel;
	public static bool cleartext;
	private string storedPlaqueData;
	public GameObject objectToToggle;
	void OnMouseDown()
	{
		if(!Test.isHitting)
		{
			if(netController.isAdmin || groupThatCanRead=="" || netController.CheckIfLocalPlayerIsInGroup(groupThatCanRead))
			{
				if(objectToToggle!=null)
				{
					if(objectToToggle.transform.localPosition!=new Vector3(-1000,-1000,-1000))
					{
						objectToToggle.SendMessage("Toggle", false);
					}
					else
					{
						objectToToggle.SendMessage("Toggle", true);
					}
				}
			}
		}
	}
	void Start()
	{
		netController= GameObject.Find("NetworkController").GetComponent<NetworkController>();
		/*
		// remember data stored in plaques during last runtime
		string oldData = PlayerPrefs.GetString(this.transform.name);
		storedPlaqueData=oldData;
		oldContent=oldData;
		StartCoroutine("SavePlaqueData");
		if(inputLabel!=null)
		{
			inputLabel.text=oldData;
		}
		*/
		if(objectToToggle!=null) //plaque should start off closed
		{
			objectToToggle.SendMessage("Toggle", true);
		}
	}
	IEnumerator SavePlaqueData()
	{
		while(true)
		{
			yield return new WaitForSeconds(30f);
			if(inputLabel.text!=storedPlaqueData)
			{
				PlayerPrefs.SetString(this.transform.name, inputLabel.text);
				PlayerPrefs.SetString("lastLogIn", System.DateTime.Now.ToString());
				storedPlaqueData=inputLabel.text;
			}
		}
	}
	void SendPlaqueData(string text)
	{
//		Debug.Log("Sending data for plaque with id" + this.transform.name);
		Dictionary<string,string> dataToSend = new Dictionary<string,string>();
		dataToSend["MethodToCall"] = "ReceivePlaqueData";
		dataToSend["SendingObjectName"] = this.transform.name;
		dataToSend["text"] = inputLabel.text;
		netController.SendCustomData(dataToSend, true, new string[0], false);
	}
	public void ReceivePlaqueData(Dictionary<string, string> dataRecieved)
	{
//		Debug.Log("Receiving data for plaque with id" + this.transform.name);
		inputLabel.text=dataRecieved["text"];
		oldContent=inputLabel.text; //prevent the data from bouncing back and forth infinitely
	}

	void Update()
	{
		if(cleartext)
		{
			inputLabel.text="";
		}
		if(inputLabel.text!=oldContent)
		{
			SendPlaqueData(inputLabel.text);
			oldContent=inputLabel.text;
		}
	}
	void OnApplicationQuit()
	{
		PlayerPrefs.SetString(this.transform.name, inputLabel.text);
		PlayerPrefs.SetString("lastLogIn", System.DateTime.Now.ToString()); //to be used to check if a sync is necessary on next login
	}
	public void Teleported() //we want all plaques to hide whenever we teleport to new locations, to minimize them appearing through walls
	{
		if(objectToToggle!=null)
		{
			objectToToggle.SendMessage("Toggle", true);
		}
	}
	public void LateUpdate()
	{
		cleartext=false;
	}
}
