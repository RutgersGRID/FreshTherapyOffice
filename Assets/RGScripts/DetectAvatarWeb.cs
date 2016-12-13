/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * DetectAvatarWeb.cs Revision 1.0.1103.01
 * Detect an avatar on collision and load up a web page based on that collision  */

using UnityEngine;
using System.Collections;

public class DetectAvatarWeb : MonoBehaviour
{
    public string dataOnTrigger = "http://reactiongrid.com";

    void OnTriggerEnter(Collider other)
    {
		Debug.Log("Load web: " + dataOnTrigger);

        if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            Application.ExternalCall("LoadExternal", dataOnTrigger);
        }
        else
        {
            Application.OpenURL(dataOnTrigger);
        }
    }
	
	void OnTriggerExit(Collider other)
    {
        Debug.Log("trigger exit");
    }
}
