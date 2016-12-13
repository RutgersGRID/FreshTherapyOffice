/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * SampleCustomData.cs Revision 1.4.1.1108.22
 * Sample code to show how to send custom data across the network for sync events  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SampleCustomData : MonoBehaviour
{

    public GUISkin skin;
    private string mostRecentlyReceivedMessage = "";

    void OnGUI()
    {
        GUI.skin = skin;
        GUIStyle buttonStyle = new GUIStyle("Button");
        buttonStyle.fixedWidth = 200;
        // A simple demo, show a button on screen, then when a message is received the message is shown on screen. 
        if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2, 150, 25), "Click to send 'Hello world!'", buttonStyle))
        {
            // get a reference to the Network Controller to send the message
            NetworkController netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
            // Construct a custom chunk of data to send over the network
            Dictionary<string, string> dataToSend = new Dictionary<string, string>();
            dataToSend["item1"] = "Hello";
            dataToSend["item2"] = "World";
            dataToSend["item3"] = "!";
            dataToSend["Sender"] = netController.GetMyName();
            dataToSend["SendingObjectName"] = gameObject.name;
            dataToSend["MethodToCall"] = "ShowReceivedData";
            Debug.Log("Sending data");
            netController.SendCustomData(dataToSend);
        };
        if (!string.IsNullOrEmpty(mostRecentlyReceivedMessage))
        {
            // If a new message has been received, show it on screen
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2 + 100, 200, 100), mostRecentlyReceivedMessage);
        }
    }

    public void ShowReceivedData(Dictionary<string, string> dataReceived, string sendingUserName)
    {
        // Called from NetworkController when a custom message is received.
        mostRecentlyReceivedMessage = dataReceived["Sender"] + " sends: \n";

        foreach (KeyValuePair<string, string> dataItem in dataReceived)
        {
            if (dataItem.Key != "Sender" && dataItem.Key != "SendingObjectName" && dataItem.Key != "MethodToCall")
            {
                mostRecentlyReceivedMessage += dataItem.Value + "\n";
            }
        }
    }
}
