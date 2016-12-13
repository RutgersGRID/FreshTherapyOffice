/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * JibeiTweenClick.cs Revision 1.4.1.1108.22
 * Synchronise iTween events across the network  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JibeiTweenClick : MonoBehaviour
{

    // Name of iTween event to fire - this value shows up in the Inspector
    public string iTweenEventToFire;

    void OnMouseDown()
    {
        // when the user clicks, run the event and send notification over the network
        RunEvent();
        SendEventNotification();
    }

    void SendEventNotification()
    {
        // Sending two pieces of information - one is the name of this object,
        // the other is the name of the iTween event (in case we have more than one)
        Dictionary<string, string> dataToSend = new Dictionary<string, string>();
        dataToSend["SendingObjectName"] = gameObject.name;
        dataToSend["iTweenEventToFire"] = iTweenEventToFire;
        dataToSend["MethodToCall"] = "ProcessEvent";
        // get a reference to the Network Controller and send the message
        NetworkController netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        Debug.Log("Sending data");
        netController.SendCustomData(dataToSend);
    }

    void RunEvent()
    {
        // Run the actual iTween event - either in response to mouse click
        // or in response to network message
        // TODO: Uncomment the next line once you have imported the iTween or iTweenVisualEditor projects from the 
        // Unity asset store. If you uncomment before you have those projects then your project will not compile.
        // iTweenEvent.GetEvent(gameObject, iTweenEventToFire).Play();
    }

    public void ProcessEvent(Dictionary<string, string> dataReceived, string sendingUserName)
    {
        // Called from NetworkController when a custom message is received.
        foreach (KeyValuePair<string, string> dataItem in dataReceived)
        {
            if (dataItem.Key == "iTweenEventToFire")
            {
                iTweenEventToFire = dataItem.Value;
                RunEvent();
            }
        }
    }
}
