using UnityEngine;
using System.Collections;

public class Mic : MonoBehaviour {
	public UIAtlas theAtlas;
	private static bool micOpen = true;
	public UISlicedSprite micIconSprite;
	public bool canUnMute=true;
	public GameObject muteNotification;
	// Use this for initialization
	void Start () {
		if(micIconSprite.spriteName=="unMuted")
		{
			Debug.Log("Starting off unmuted");
			micOpen=true;
		}
		else
		{
			Debug.Log("Starting off muted");
			micOpen=false;
		}
	}
	
	public static void UpdateMicStatus()
	{
		Application.ExternalCall("VivoxMicMute", !micOpen);
	}
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isDown) {
		if(isDown==true && canUnMute)
		{
            Application.ExternalCall("VivoxMicMute", micOpen);
            micOpen = !micOpen;
			if(micOpen)
			{
				micIconSprite.sprite=theAtlas.GetSprite("unMuted");
				micIconSprite.spriteName="unMuted";
			}
			else
			{
				micIconSprite.sprite=theAtlas.GetSprite("muted");
				micIconSprite.spriteName="muted";
			}
		}
	}
	
	public void Mute(bool isMuted)
	{
        Application.ExternalCall("VivoxMicMute", isMuted);
		if(isMuted)
		{
			muteNotification.SendMessage("Toggle", false);
			micIconSprite.sprite=theAtlas.GetSprite("Forcemuted");
			micIconSprite.spriteName="Forcemuted";
		}
	}
	public void ResetSprite()
	{
		muteNotification.SendMessage("Toggle", true);
		Application.ExternalCall("VivoxMicMute", !micOpen);
		if(micOpen)
		{
			micIconSprite.sprite=theAtlas.GetSprite("unMuted");
			micIconSprite.spriteName="unMuted";
		}
		else
		{
			micIconSprite.sprite=theAtlas.GetSprite("muted");
			micIconSprite.spriteName="muted";
		}
	}
}
