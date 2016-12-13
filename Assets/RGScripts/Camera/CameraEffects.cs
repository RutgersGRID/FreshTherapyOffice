/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * CameraEffects.cs Revision 1.0.1103.01
 * Some simple overrides for player camera clip plane and fog settings. Includes simple way to apply underwater fog based on water height */

using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    public float playerCameraNearClipPlane = 0.35f;
    public float playerCameraFarClipPlane = 400.0f;
    public float playerCameraFieldOfView = 60.0f;

    public bool fogEnabled = false;
    public Color fogColor = new Color(235.0f, 224.0f, 190.0f, 255.0f);
    public float fogDensity = 0.007f;

    public bool useUnderwaterFog = false;
    public float waterHeight = 20.0f;
    public Color underwaterFogColor = new Color(20.0f, 20.0f, 20.0f, 20.0f);
    public float underwaterFogDensity = 0.1f;

	void Start () 
    {
        Camera.main.nearClipPlane = playerCameraNearClipPlane;
        Camera.main.farClipPlane = playerCameraFarClipPlane;
        Camera.main.fieldOfView = playerCameraFieldOfView;

        RenderSettings.fog = fogEnabled;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
	}
	
	void Update () 
    {
        // if Underwater Fog is enabled and the user goes under the water level, use the underwater fog effect
        if (useUnderwaterFog && transform.position.y < waterHeight)
        {
            RenderSettings.fogColor = underwaterFogColor;
            RenderSettings.fogDensity = underwaterFogDensity;
        }
        else
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
	}
}
