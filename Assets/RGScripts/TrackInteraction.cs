/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * TrackInteraction.cs Revision 1.0.1103.01
 * Tracks an event and records to log - can be set to track on click or collide  */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TrackInteraction : MonoBehaviour
{
    private JibeActivityLog jibeLog;
    public float clickDistanceLimit = 20;
    public string dataOnClick = "http://reactiongrid.com";
    public enum InteractionType { Click, Collide }
    public InteractionType interactionType = InteractionType.Click;
    private string username;
    private string userId;
    public bool useEventRepeatBlock = true; // Stop too many repeated events being sent
    private float eventRepeatBlock = 5.0f;
    private float eventInterval = 0.0f;
    private bool proceed = true;
    private NetworkController networkController;

    void Start()
    {
        networkController = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
    }
    void Update()
    {
        eventInterval += Time.deltaTime;
        if (useEventRepeatBlock)
        {
            if (eventInterval > eventRepeatBlock)
            {
                proceed = true;
            }
            else
            {
                proceed = false;
            }
        }
        // Track a click
        if (interactionType == InteractionType.Click && Input.GetMouseButtonDown(0) && proceed)
        {           
            eventInterval = 0.0f;
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
                            Debug.Log(hit.transform.name + " clicked: " + dataOnClick);
                            if (jibeLog == null)
                            {
                                jibeLog = GameObject.Find("Jibe").GetComponent<JibeActivityLog>();
                                if (jibeLog != null)
                                {
                                    Debug.Log("Found jibeLog");
                                }
                            }
                            if (jibeLog.logEnabled)
                            {
                                if (string.IsNullOrEmpty(username))
                                {
                                    if (networkController != null)
                                    {
                                        username = networkController.GetUserName();
                                        userId = networkController.GetUserId().ToString();
                                    }
                                }
                                // Do the logging - a debug message will show "Logged" if the log was updated
                                Debug.Log(jibeLog.TrackEvent(JibeEventType.Touch, Application.dataPath, this.transform.position.x, this.transform.position.y, this.transform.position.z, userId, username, dataOnClick));
                            }
                        }
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger on collision
        if (interactionType == InteractionType.Collide && proceed)
        {
            if (jibeLog == null)
            {
                jibeLog = GameObject.Find("Jibe").GetComponent<JibeActivityLog>();
                if (jibeLog != null)
                {
                    Debug.Log("Found jibeLog");
                }
            }
            if (jibeLog.logEnabled)
            {
                if (string.IsNullOrEmpty(username))
                {
                    if (networkController != null)
                    {
                        username = networkController.GetUserName();
                        userId = networkController.GetUserId().ToString();
                    }
                }
                // Do the logging - a debug message will show "Logged" if the log was updated
                Debug.Log(jibeLog.TrackEvent(JibeEventType.Collide, Application.dataPath, this.transform.position.x, this.transform.position.y, this.transform.position.z, userId, username, dataOnClick));
            }
        }
    }
}
