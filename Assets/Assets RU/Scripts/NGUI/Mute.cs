using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Mute : MonoBehaviour {
	public string name;
	public int id;
	// Use this for initialization
	public bool muteNext=true;
	public NetworkController netController;
	public UISlicedSprite mySprite;
	// Use this for initialization
	void Start () {
		netController=GameObject.Find("NetworkController").GetComponent<NetworkController>();
		mySprite=GetComponentInChildren<UISlicedSprite>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress(bool isPressed) {
		if(isPressed==true)
		{
			Dictionary<string,string> dataToSend = new Dictionary<string, string>();
			dataToSend["SendingObjectName"] = "VivoxHud";
			dataToSend["MethodToCall"] = "RemoteUserMute";
			dataToSend["UserName"] = name;
			dataToSend["ID"] = ""+id;
			if(muteNext==true)
			{
				dataToSend["Mute"]="True";
				muteNext=false;
				mySprite.spriteName="UnMuteAll";
			}
			else
			{
				dataToSend["Mute"]="False";
				muteNext=true;
				mySprite.spriteName="Muteall";
			}			
			netController.SendCustomData(dataToSend, true, new string[0], false);
		}
	}
	
	public void FakePress() {
		if(muteNext==true)
		{
			muteNext=false;
			mySprite.spriteName="UnMuteAll";
		}
		else
		{
			muteNext=true;
			mySprite.spriteName="Muteall";
		}	
	}
}
