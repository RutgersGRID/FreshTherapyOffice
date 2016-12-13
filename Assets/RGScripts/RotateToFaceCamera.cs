/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * RotateToFaceCamera.cs Revision 1.4.1107.18
 * Keep an object facing the camera at all times */

using UnityEngine;
using System.Collections;

public class RotateToFaceCamera : MonoBehaviour
{
    GameObject mainCam;

    void Update()
    {
        if (mainCam == null)
        { 
            mainCam = GameObject.FindGameObjectWithTag("MainCamera"); 
        }
        transform.LookAt(mainCam.transform);
    }
}
