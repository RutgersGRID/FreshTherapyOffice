/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * Version.cs Revision 1.0.1103.01
 * Show onscreen text displaying current build revision (as set in initial loader scene)  */

using UnityEngine;
using System.Collections;

public class Version : MonoBehaviour {

    public NetworkController networkController;
    private float versionTimeOut = 3.0f;
    private float count = 0.0f;
	void Start () {
        if (networkController == null)
            networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        count = 0.0f;
	}
	

	public void SetVersionText (string version) 
    {
        // Use a simple GUIText object to display the version on screen
        if (GetComponent<GUIText>() != null)
            GetComponent<GUIText>().text = version;
	}

    void Update()
    {
        // Cunning trick to get the active version number from Network controller - 
        // give it a few seconds to get set up correctly then when the value is set, disable this script completely
        // since there is no need to have an active script for a value that does not change
        while (count < versionTimeOut)
        {
            SetVersionText(networkController.GetVersion());
            count += Time.deltaTime;
        }
        if (count > versionTimeOut)
        {
            this.enabled = false;
        }
    }

}
