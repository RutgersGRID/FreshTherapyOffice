/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * NetworkReceiver.cs Revision 1.0.1103.01
 * Handles position updates for remote players, also applies remote hair and skin textures when they are spawned */

using UnityEngine;
using System.Collections;
using System;

public class NetworkReceiver : MonoBehaviour
{

    public float yAdjust = 0.0f; // Ajust y position when synchronizing the local and remote models.
    public float interpolationPeriod = 0.1f;  // This value should be equal to the sendingPeriod value of the Sender script

    private bool receiveMode = false;
    private NetworkTransform interpolateTo = null;  // Last state we interpolate to in receiving mode.
    private NetworkTransform interpolateFrom;  // Point from which to start interpolation

    private int interpolationStartTime;
    private int interpolationEndTime;

    // We call it on remote player to start receiving his transform
    public void StartReceiving()
    {
        Debug.Log("Starting to receive transforms from the network");
        receiveMode = true;
    }

    void Update()
    {
        if (receiveMode)
        {
            InterpolateTransform();
        }
    }

    //This method is called when receiving remote transform
    // We update lastState here to know last received transform state
    public void ReceiveTransform(Vector3 pos, Quaternion rot)
    {
        if (receiveMode)
        {
            pos.y = pos.y + yAdjust;
            if (interpolateTo == null)
            {
                // for newly spawned players, create a target at their current location before first transform is received.
                interpolateTo = new NetworkTransform(this.gameObject);
            }

            interpolateFrom = new NetworkTransform(this.gameObject);
            // calculate interpolation values

            interpolationPeriod = 0.1f;
            interpolationStartTime = Environment.TickCount;

            int maxInterpolationTime = (int)Math.Floor(1000 * interpolationPeriod);

            int interpolationTime = maxInterpolationTime;

            interpolationEndTime = interpolationStartTime + interpolationTime;
            interpolateTo.InitFromValues(pos, rot);
        }
    }

    void InterpolateTransform()
    {
		if(interpolateTo!=null)
		{
	        int timeNow = Environment.TickCount;
	        // If interpolationg
	        if (timeNow < interpolationEndTime)
	        {
	            float t = (timeNow - interpolationStartTime) / (1000 * interpolationPeriod);
	            if (t > 1) t = 1;
	            transform.position = Vector3.Lerp(interpolateFrom.position, interpolateTo.position, t);
	            transform.rotation = Quaternion.Slerp(interpolateFrom.rotation, interpolateTo.rotation, t);
	        }
	        else if (interpolateTo != null)
	        {
	            // you have reached your destination
	            // Finished interpolating to the next point
	
	            // Fixing interpolation result to set transform right to the next point
	            transform.position = interpolateTo.position;
	            transform.rotation = interpolateTo.rotation;
	        }
		}
    }

    public void SetSkinRemote(Texture2D avatarSkin)
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).tag == "Skin")
            {
                Debug.Log("Found remote player skin!");
                this.transform.GetChild(i).GetComponent<Renderer>().material.mainTexture = avatarSkin;
            }
        }
    }

    public void SetHairRemote(Texture2D avatarHair)
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).tag == "Wig")
            {
                Debug.Log("Found my hair!");
                this.transform.GetChild(i).GetComponent<Renderer>().material.mainTexture = avatarHair;
            }
        }
    }
}
