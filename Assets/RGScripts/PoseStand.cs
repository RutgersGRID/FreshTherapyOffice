/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * PoseStand.cs Revision 1.0.1103.01
 * As used in the dressing room scene - click on the pose stand and move mouse left-right to rotate the podium  */

using UnityEngine;
using System.Collections;

public class PoseStand : MonoBehaviour {

    private bool rotate;

	void Update () 
    {
        if (rotate)
        {
            transform.RotateAround(Vector3.up, Input.GetAxisRaw("Mouse X") / -10);
        }
	}

    void OnMouseDown()
    {
        rotate = true;
    }

    void OnMouseUp()
    {
        rotate = false;
    }
}
