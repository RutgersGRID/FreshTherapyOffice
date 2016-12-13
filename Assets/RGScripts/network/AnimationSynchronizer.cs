/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * AnimationSynchronizer.cs Revision 1.3.1105.25
 * Controls network message transmission and reception - same code is used on both local and remote players. Local players send, remote players receive */

using UnityEngine;
using System.Collections;
using System;

public class AnimationSynchronizer : MonoBehaviour {
	
	public string lastState = "idle";
    private GameObject networkController;

    public bool receiveMode = false;

    // Called on local player to start sending animation messages
    public void StartSending()
    {
        Debug.Log("Starting an animation sender");
        receiveMode = false;
    }
	
	// Called on remote player model to start receiving animation messages
	public void StartReceiving() 
    {
        Debug.Log("Starting an animation receiver");
        receiveMode = true;
        GetComponent<Animation>().Play(lastState);
	}

	
	public void PlayAnimation(string message) 
    {
        GetComponent<Animation>().wrapMode = WrapMode.Loop;
        GetComponent<Animation>().CrossFade(message);
	}	
	
	public void SendAnimationMessage(string message) 
    {
        //if the new state differs, send animation message to other clients
        if (!receiveMode)
        {
            if (lastState != message)
            {
                lastState = message;
                if (networkController == null)
                    networkController = GameObject.Find("NetworkController");
                networkController.GetComponent<NetworkController>().SendAnimation(message);
            }
        }
	}
}
