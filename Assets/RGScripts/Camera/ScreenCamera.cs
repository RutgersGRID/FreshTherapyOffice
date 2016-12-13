/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * ScreenCamera.cs Revision 1.0.1103.01
 * Used to control a camera's position relative to a screen for a slideshow tool */

using UnityEngine;
using System.Collections;

public class ScreenCamera : MonoBehaviour {

    public Transform target;
    public float distance = 20.0f;

    public float maxDistance = 30.0f;
    public float minDistance = 10.0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (distance > minDistance)
            {
                distance--;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (distance < maxDistance)
            {
                distance++;
            }
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            Vector3 localPos = transform.localPosition;
            localPos.z = distance;
            transform.localPosition = localPos;
        }
    }
}
