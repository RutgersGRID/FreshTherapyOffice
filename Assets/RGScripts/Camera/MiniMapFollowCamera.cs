/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * MiniMapFollowCamera.cs Revision 1.0.1103.01
 * Ensure the mini map camera tracks the position of the player */

using UnityEngine;
using System.Collections;

public class MiniMapFollowCamera : MonoBehaviour {

    public Transform target; // The player to follow
	
	void Update () 
    {
        if (target != null)
        {
            Vector3 newPos = transform.position;
            newPos.x = target.position.x;
            newPos.z = target.position.z;
            if (newPos.x != transform.position.x || newPos.z != transform.position.z)
            {
                transform.position = newPos;
            }
        }
	}

    public void SetTarget(Transform t)
    {
        // This method is called from within PlayerSpawnController to specify the avatar to follow.
        target = t;
    }
}
