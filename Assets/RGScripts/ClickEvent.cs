/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * ClickEvent.cs Revision 1.0.1103.01
 * Launch a url with a click (with optional particle effect) */

using UnityEngine;
using System.Collections;

public class ClickEvent : MonoBehaviour
{
    public string dataOnClick = "http://reactiongrid.com";
    public float clickDistanceLimit = 20;
    public bool ShowParticlesOnClick = false;
    public GameObject particleEmitterObject; // optional - attach a prefab that has particle emitter and renderer components here and you can show a flurry of particles on click
    public bool useClickRepeatBlock = true;
    private float clickRepeatBlock = 5.0f; // how many seconds between clicks
    private float clickInterval = 0.0f;
    private bool proceed = true;

    void Update()
    {
        if (useClickRepeatBlock)
        {
            // Stop too many click events from being raised - a small sleep between click intervals is not a bad plan
            clickInterval += Time.deltaTime;
            if (clickInterval > clickRepeatBlock)
            {
                proceed = true;
            }
            else
            {
                proceed = false;
            }
        }
        if (clickInterval > clickRepeatBlock && Input.GetMouseButtonDown(0) && proceed)
        {
            clickInterval = 0.0f;
            GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (playerCam != null)
            {
                if (playerCam.GetComponent<Camera>().enabled)
                {
                    Ray mouseRay = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(mouseRay, out hit, clickDistanceLimit))
                    {
                        if (hit.transform == this.transform)
                        {
                            // click event detected successfully - either render a link on web page (via javascript call to function "LoadExternal" on hosting page
                            // or, for standalones, open the url window
                            Debug.Log(hit.transform.name + " clicked: " + dataOnClick);
                            if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
                            {
                                Application.ExternalCall("LoadExternal", dataOnClick);
                            }
                            else
                            {
                                Application.OpenURL(dataOnClick);
                            }

                            if (ShowParticlesOnClick)
                            {
                                if (particleEmitterObject != null)
                                {
                                    ParticleController particleEmitter = particleEmitterObject.GetComponent<ParticleController>();
                                    if (particleEmitter != null)
                                        particleEmitter.StartEmitting();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
