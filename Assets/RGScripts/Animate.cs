/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * Animate.cs Revision 1.0.1103.01
 * Adding a NPC to a scene you can force a particular animation to play on loop by dropping in this script  */

using UnityEngine;
using System.Collections;

public class Animate : MonoBehaviour {

    public string animationToPlay = "idle";

	void Start () {
		GetComponent<Animation>().wrapMode = WrapMode.Loop;
        GetComponent<Animation>().Play(animationToPlay);
	}	
}
