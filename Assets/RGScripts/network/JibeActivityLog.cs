/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * JibeActivityLog.cs Revision 1.0.1103.01
 * used for logging activity to a database via a web request */

using UnityEngine;
using System.Collections;

public class JibeActivityLog : MonoBehaviour
{
    private WWW webEvent;
    public bool logEnabled = false;
    public string JibeDataUrl = "";   

    /// <summary>
    /// Called to track an event in Jibe to the JibeData activity database
    /// </summary>
    /// <param name="eventType">The type of event</param>
    /// <param name="jibeInstance">The name of the Jibe world or a similar identification string</param>
    /// <param name="locX">X coordinate where event occurred</param>
    /// <param name="locY">Y coordinate where event occurred</param>
    /// <param name="locZ">Z coordinate where event occurred</param>
    /// <param name="userId">The ID of the user who initiated the event</param>
    /// <param name="username">The name of the user who initiated the event</param>
    /// <param name="eventDetails">Details of the event - a description of what happened</param>
    /// <returns>Simple string "logged" when the event is logged</returns>
    public string TrackEvent(JibeEventType eventType, string jibeInstance, float locX, float locY, float locZ, string userId, string username, string eventDetails)
    {
        JibeEvent newEvent = new JibeEvent();
        newEvent.EventType = eventType;
        newEvent.JibeInstance = jibeInstance;
        newEvent.LocX = locX;
        newEvent.LocY = locY;
        newEvent.LocZ = locZ;
        newEvent.UserId = userId;
        newEvent.UserName = username;
        newEvent.EventDetails = eventDetails;
        if (!string.IsNullOrEmpty(JibeDataUrl))
        {
            StartCoroutine(LogEvent(newEvent));
        }
        

        return "Logged";
    }

    private IEnumerator LogEvent(JibeEvent newEvent)
    {
        WWWForm eventForm = new WWWForm();
        eventForm.AddField("EventTypeId", (int)newEvent.EventType);
        eventForm.AddField("EventType", newEvent.EventType.ToString());
        eventForm.AddField("JibeInstance", newEvent.JibeInstance);
        eventForm.AddField("LocX", newEvent.LocX.ToString());
        eventForm.AddField("LocY", newEvent.LocY.ToString());
        eventForm.AddField("LocZ", newEvent.LocZ.ToString());
        eventForm.AddField("UserId", newEvent.UserId);
        eventForm.AddField("UserName", newEvent.UserName);
        eventForm.AddField("EventDetails", newEvent.EventDetails);
        webEvent = new WWW(JibeDataUrl, eventForm);
        yield return (webEvent);
        if (!string.IsNullOrEmpty(webEvent.error))
        {
            Debug.Log(webEvent.error);
        }
    }

    private void LogEventSync(JibeEvent newEvent)
    {
        // This method runs synchronously, meaning if you call this then no further processing takes places until the call completes.
        // The default is to use the async method, but this code remains here in case a need arises for a sync log event. 
        WWWForm eventForm = new WWWForm();
        eventForm.AddField("EventTypeId", (int)newEvent.EventType);
        eventForm.AddField("EventType", newEvent.EventType.ToString());
        eventForm.AddField("JibeInstance", newEvent.JibeInstance);
        eventForm.AddField("LocX", newEvent.LocX.ToString());
        eventForm.AddField("LocY", newEvent.LocY.ToString());
        eventForm.AddField("LocZ", newEvent.LocZ.ToString());
        eventForm.AddField("UserId", newEvent.UserId);
        eventForm.AddField("UserName", newEvent.UserName);
        eventForm.AddField("EventDetails", newEvent.EventDetails);
        webEvent = new WWW(JibeDataUrl, eventForm);
    }
}

public enum JibeEventType { Misc, Login, Logout, Touch, Collide, Chat, Move, Teleport, Sit };

/// <summary>
/// A class that describes a JibeEvent that can be used for tracking to a database
/// </summary>
public class JibeEvent
{
    private JibeEventType _eventType;
    /// <summary>
    /// The type of event that has occurred
    /// </summary>
    public JibeEventType EventType
    {
        get { return _eventType; }
        set { _eventType = value; }
    }

    private string _jibeInstance;
    /// <summary>
    /// The name of the Jibe world or a similar identification string
    /// </summary>
    public string JibeInstance
    {
        get { return _jibeInstance; }
        set { _jibeInstance = value; }
    }

    private float _locX;
    /// <summary>
    /// X coordinate where event occurred
    /// </summary>
    public float LocX
    {
        get { return _locX; }
        set { _locX = value; }
    }

    private float _locY;
    /// <summary>
    /// Y coordinate where event occurred
    /// </summary>
    public float LocY
    {
        get { return _locY; }
        set { _locY = value; }
    }

    private float _locZ;
    /// <summary>
    /// Z coordinate where event occurred
    /// </summary>
    public float LocZ
    {
        get { return _locZ; }
        set { _locZ = value; }
    }

    private string _userId;
    /// <summary>
    /// The ID of the user who initiated the event
    /// </summary>
    public string UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    private string _username;
    /// <summary>
    /// The name of the user who initiated the event
    /// </summary>
    public string UserName
    {
        get { return _username; }
        set { _username = value; }
    }

    private string _eventDetails;
    /// <summary>
    /// Details of the event - a description of what happened
    /// </summary>
    public string EventDetails
    {
        get { return _eventDetails; }
        set { _eventDetails = value; }
    }
    
}