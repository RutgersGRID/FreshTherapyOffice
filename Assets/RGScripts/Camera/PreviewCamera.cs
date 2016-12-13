/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * PreviewCamera.cs Revision 1.0.1104.05
 * Ensures that a camera used while in design mode is removed during runtime */

using UnityEngine;
using System.Collections;

/// <summary>
/// Turn off the preview camera during runtime
/// </summary>
public class PreviewCamera : MonoBehaviour {

    public bool turnOffByDefault = true;
	void Start () 
    {
        SetActive(!turnOffByDefault);
	}
	
	public void SetActive(bool active)
	{
		this.GetComponent<Camera>().enabled = active;
	}
}
