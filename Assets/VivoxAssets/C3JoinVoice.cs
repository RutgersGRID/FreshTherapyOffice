using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using System.IO; 


// ----------------- CHANGES FOR C3 LITE INTEGRATION --------------------

/// <summary>
/// This file can be included in your project to enable simple integration with C3
/// </summary>
public class C3JoinVoice : MonoBehaviour
{
	public GUISkin skin;
    public static C3JoinVoice SP;

	private Texture voiceIcon;
	private int iconSize = (int)32;
	public Vector2 tooltip_pos = new Vector2(30,0);
	
	

	// this string identifies your application - it should be provided to you by Vivox
	// Each game or publisher should request a C3 App ID from Vivox by emailing c3@vivox.com
	private string yourC3AppId = "ChangeThis";

	
    void Awake()
    {
        SP = this;
		voiceIcon = (Texture)Resources.Load("VivoxAssets/Unity_C3_32x32");
    }

    void OnGUI ()
	{        
		
		GUI.skin = skin;
		GUI.SetNextControlName ("");

		GUILayout.BeginArea (new Rect (Screen.width - iconSize - 150, Screen.height - iconSize - 20, iconSize+150, iconSize + 10));
        		
		// Create the button with a tooltip
		GUIContent C3Button = new GUIContent ();
		C3Button.text = "";
		C3Button.image = voiceIcon;
		C3Button.tooltip = "Click Here to Join Voice Chat!";
		// display button for joining the voice channel in C3
        if (GUILayout.Button(C3Button, "", GUILayout.Height(40))) {
			JoinVoice("UnityDemo", "UnityDemo");
		}
		
        GUI.Label(new Rect (tooltip_pos.x, tooltip_pos.y, 142, 40), GUI.tooltip);

        GUILayout.EndArea();
	}

	void OnLeftRoom()
    {
        this.enabled = false;
    }

    void OnJoinedRoom()
    {
        this.enabled = true;
    }
    void OnCreatedRoom()
    {
        this.enabled = true;
    }
	
	
	public void JoinVoice(string channelId, string userId)
	{
		
		// construct the URL for the web page that allows download of C3 and also redirects to a URL protocal request that causes C3 to join the specified channel
		string sUrl = "http://social.c3p.vivox.com/lite_protocol.php?app=" + yourC3AppId + "&channel=" + channelId + "&user=" + userId;
		
#if UNITY_WEBPLAYER
		// start browser and show web page
		Application.ExternalEval("window.open('" + sUrl + "','C3');");
		
#elif UNITY_STANDALONE_WIN
		// check if C3 is installed
		string sC3Path = System.Environment.GetEnvironmentVariable("C3PATH");
		
		if (sC3Path == "")
			sC3Path = "C:\\Program Files (x86)\\Vivox\\C3";
		
		if (System.IO.File.Exists(sC3Path + "\\C3.exe"))
		{
			// C3 is installed
			// send URL protocol request to C3 without starting the browser to show the web page
			string sProtocol = "c3:TP-" + yourC3AppId + "-" + channelId + "-" + userId;
			Application.OpenURL(sProtocol);
		}
		else
		{
			// start browser and show web page
			Application.OpenURL(sUrl);
		}
#elif UNITY_STANDALONE_OSX
		// check if C3 is installed (on OSX we only check for the existance of the environment variable)
		string sC3Path = System.Environment.GetEnvironmentVariable("C3PATH");
		// 05/2012,  the C3PATH does not exist even after C3 is installed on a Mac

		if (!String.IsNullOrEmpty(sC3Path))
		{
			// C3 is installed
			// send URL protocol request to C3 without starting the browser to show the web page
			string sProtocol = "c3:TP-" + yourC3AppId + "-" + channelId + "-" + userId;
			Application.OpenURL(sProtocol);
		}
		else
		{
			// start browser and show web page
			Application.OpenURL(sUrl);
		}
		
#else
		// start browser and show web page
		Application.OpenURL(sUrl);
#endif
	}
}

		// -------------- END OF CHANGES FOR C3 LITE INTEGRATION ----------------

	
