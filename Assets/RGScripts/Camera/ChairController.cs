/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * ChairController.cs Revision 1.0.1103.01
 * If placed in a collider that is tagged as a SitTarget, and if a screen camera is specified, the user can switch player cam to screen cam and back */

using UnityEngine;
using System.Collections;

public class ChairController : MonoBehaviour
{
    public GameObject screenCamera;
    public bool useScreenCamera;
    public GUISkin guiSkin;
    private bool isOccupied = false;

    void OnGUI()
    {
        if (isOccupied)
        {
            GUI.skin = guiSkin;
            if (screenCamera != null)
            {
                // Toggle between screen camera and player camera via a GUI button
                if (useScreenCamera)
                {
                    if (GUI.Button(new Rect(100, 0, 100, 24), "Player Camera"))
                    {
                        useScreenCamera = false;
                        SwitchToPlayerCamera();
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(100, 0, 100, 24), "Screen Camera"))
                    {
                        useScreenCamera = true;
                        SwitchToScreenCamera();
                    }
                }
            }
        }
    }
    public void SwitchToScreenCamera()
    {
        // Turn off the player camera
        try
        {
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            mainCam.GetComponent<Camera>().enabled = false;
            ((Behaviour)mainCam.GetComponent("PlayerCamera")).enabled = false;
            ((Behaviour)mainCam.GetComponent("AudioListener")).enabled = false;
            screenCamera.active = true;
            screenCamera.GetComponent<Camera>().enabled = true;
        }
        catch (System.Exception ex)
        {
            Debug.Log("Problems with camera: " + ex.Message + ex.StackTrace);
        }
    }

    public void SwitchToPlayerCamera()
    {
        // Re-enable regular camera
        try
        {
            screenCamera.GetComponent<Camera>().enabled = false;
            screenCamera.active = false;
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            mainCam.GetComponent<Camera>().enabled = true;
            ((Behaviour)mainCam.GetComponent("PlayerCamera")).enabled = true;
            ((Behaviour)mainCam.GetComponent("AudioListener")).enabled = true;
        }
        catch (System.Exception ex)
        {
            Debug.Log("Problems with camera: " + ex.Message + ex.StackTrace);
        }
    }

    public void UnSit()
    {
        if (useScreenCamera)
        {
            SwitchToPlayerCamera();
        }
        isOccupied = false;
    }
    public void Sit()
    {       
        isOccupied = true;
        if (useScreenCamera && screenCamera != null)
        {
            SwitchToScreenCamera();
        }
    }
}
