/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * GetData.cs Revision 1.0.1103.01a
 * Use a web request to a specific url to retrieve some data. Often used in partnership with WebTexture for inworld slide functionality  */

using UnityEngine;
using System;
using System.Collections;

public class GetData : MonoBehaviour
{
    public string dataUrl = "http://jibemix.com/currentslide.aspx"; // url to a page that returns a string
    public string currentData = "";    
    public bool dataForSlidesByRoom = false; // used for dynamic rooms - not yet implemented
    WWW webRequest;
    public bool runOnTimer = true;
    public float timerInterval = 5;
    private float timerTicks = 0;
    public bool forceUpdateScreen = true; // useful if there is a presentation screen in the scene for the screen to be updated from here

    void Awake()
    {
        if (dataForSlidesByRoom) // only used in dynamic room scenarios
        {
            string roomId = PlayerPrefs.GetInt("ClassId").ToString();
            if (string.IsNullOrEmpty(roomId)) roomId = "0";
            dataUrl = dataUrl + roomId;
        }
        Debug.Log("Getting data from " + dataUrl);
        StartCoroutine(LoadData(dataUrl));
    }

    void FixedUpdate()
    {
        if (runOnTimer)
        {
            // update the timer ticks and when the interval has elapsed, poll for new data
            timerTicks += Time.deltaTime;
            if (timerTicks > timerInterval)
            {
                timerTicks = 0;
                StartCoroutine(LoadData(dataUrl));
            }
        }
    }

    private IEnumerator LoadData(string requestUrl)
    {
        // use a coroutine to get data asyncronously
        yield return (webRequest = new WWW(requestUrl));
        if (webRequest.error != null)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            string newData = webRequest.text;
            if (newData != currentData)
            {
                currentData = newData;
                if (forceUpdateScreen)
                {
                    try
                    {
                        GetComponent<WebTexture>().ForceUpdateScreen();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Failed to update screen - does the game object have a copy of WebTexture script in it? " + ex.Message);
                    }
                }
            }            
        }
    }
}