/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * SitOffset.cs Revision 1.4.1107.20
 * Provides offset parameters for an avatar while seated */

using UnityEngine;
using System.Collections;

public class SitOffset : MonoBehaviour {

    public float SitOffsetX = 0.0f;
    public float SitOffsetY = 0.0f;
    public float SitOffsetZ = 0.0f;
    public string SitPose = "sit1";
	public Quaternion offsetRotation;
	void JibeInit()
	{
		string avatarName = GameObject.Find("localPlayer").transform.GetChild(0).name;
		/*if(avatarName.Equals("M1CharacterMixamo") || avatarName.Equals("M2CharacterMixamo") || avatarName.Equals("M3CharacterMixamo"))
		{
			Debug.Log("Extra offset for mixamo males");
			SitOffsetX+=-.1f;//.1
			SitOffsetY+=.8f;//.55
			SitOffsetZ+=.5f;//.4
		}*/
		Vector3 localOffset = new Vector3(SitOffsetX,SitOffsetY,SitOffsetZ);
		localOffset+=transform.position;
		localOffset=transform.InverseTransformPoint(localOffset); //switch to local coordinates
		SitOffsetX=localOffset.x;
		SitOffsetY=localOffset.y;
		SitOffsetZ=localOffset.z;
	}
}
