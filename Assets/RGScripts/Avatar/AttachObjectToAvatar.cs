/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * AttachObjectToAvatar.cs Revision 1.4.1107.14
 * Attaches objects to avatars at specified locations on their skeleton */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttachObjectToAvatar : MonoBehaviour {

    public string AttachmentPoint = "HeadEnd";

	void OnMouseDown () 
    {
        GameObject localPlayer = GameObject.Find("localPlayer");
        WearAttachment(localPlayer, AttachmentPoint);
        SendAttachmentSync();
	}

    void SendAttachmentSync()
    {
        NetworkController netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        // Sending attachment information - one is the name of this object,
        // we also send attach point, avatar ID and pass the name of the receiving method
        Dictionary<string, string> dataToSend = new Dictionary<string, string>();
        dataToSend["SendingObjectName"] = gameObject.name;
        dataToSend["AttachPoint"] = AttachmentPoint;
        dataToSend["AvatarID"] = netController.GetLocalPlayer().PlayerID.ToString();
        dataToSend["MethodToCall"] = "ReceiveAttachmentSync";
        // get a reference to the Network Controller and send the message       
        Debug.Log("Sending data");
        netController.SendCustomData(dataToSend);
    }

    public void ReceiveAttachmentSync(Dictionary<string, string> data)
    {
        NetworkController netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        string remotePlayerName = netController.RemotePlayerGameObjectPrefix + data["AvatarID"];
        string attachmentPoint = data["AttachPoint"];
        GameObject player = GameObject.Find(remotePlayerName);
        if (player != null)
        {
            WearAttachment(player, attachmentPoint);
        }
    }

    public void WearAttachment(GameObject avatar, string attachmentPoint)
    {
        var requiredAttachmentPoint = SearchForAttachmentPoint(avatar.transform).First(t => t.name == attachmentPoint);
        if (requiredAttachmentPoint != null)
        {
            this.GetComponent<Renderer>().material.color = Color.red;
            this.transform.rotation = requiredAttachmentPoint.rotation;
            this.transform.parent = requiredAttachmentPoint; // Attach hat to head bone.
            this.transform.localPosition = new Vector3(0, 0, 0); // Set local position so that hat sits on top of the head.
            this.transform.localRotation = Quaternion.identity; // Zero attachment's rotation relative to parent node.
            Destroy(GetComponent<Collider>()); // required to prevent camera from acting up
        }
        else
        {
            Debug.Log("Attachment point " + attachmentPoint + " could not be found!");
        }
    }

    IEnumerable<Transform> SearchForAttachmentPoint(Transform root)
    {
        yield return root;
        foreach (Transform t in root.transform)
        {
            foreach (Transform t2 in SearchForAttachmentPoint(t)) yield return t2;
        }
    }

}
