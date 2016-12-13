/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.  
 * 
 * BubblePopup.cs Revision 1.2.1104.26
 * Controls bubbles over other player's heads displaying name tags and chat speech bubbles */

using UnityEngine;
using System.Collections;
using ReactionGrid.Jibe;

public class BubblePopup : MonoBehaviour
{

    public GUISkin skin;
    public float chatBubbleWidth = 240;
    private string currentUser;
    public float showTime = 4.0f;  // We display each bubble for 4 seconds (configurable).
    public float nameTagHeight = 15.0f; // change vertical displacement for nametag for each avatar	

    private string str = "";   // Striing to display
    private float bubbleTime = 0.0f;    // Time counter   

    public float fadeDistance = 30.0f; // after player moves over this distance from the camera the name tag is no longer shown (helpes reduce overhead)
    public float fullyVisibleDistance = 10.0f; // Up to this distance, nametag is fully rendered (no fading)
    private float currentAlpha = 0.0f;
    private Color guiColor;
    private Color guiTextColor;
    private float currentDistance = 0.0f;
    private bool isSpeaking = false;

    private GameObject localPlayer;
    private GameObject playerCamera;

    void Start()
    {
        localPlayer = GameObject.FindGameObjectWithTag("Player");
        playerCamera = GameObject.Find("PlayerCam");
    }
    void OnGUI()
    {
        //if there is something to say, show the message in the bubble
        if (!string.IsNullOrEmpty(str))
        {
            GUI.skin = skin;
            GUIContent content = new GUIContent(str);
            float textHeight = skin.GetStyle("Box").CalcHeight(content, chatBubbleWidth);
            Vector3 bubblePos = transform.position + new Vector3(0, nameTagHeight, 0);  //We adjust world Y position here to make chat bubble near the head of the model
            if (playerCamera == null)
            {
                playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            if (playerCamera != null && playerCamera.GetComponent<Camera>().enabled)
            {
                Vector3 screenPos = playerCamera.GetComponent<Camera>().WorldToScreenPoint(bubblePos);
                // We render our text only if it's in the screen view port	
                if (screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height && screenPos.z >= 0)
                {
                    if (localPlayer != null)
                    {
                        if (currentDistance < fadeDistance)
                        {
                            guiColor = GUI.color;
                            guiTextColor = GUI.contentColor;

                            guiColor.a = currentAlpha;
                            guiTextColor.a = currentAlpha;

                            GUI.color = guiColor;
                            GUI.contentColor = guiTextColor;

                            Vector2 pos = GUIUtility.ScreenToGUIPoint(new Vector2(screenPos.x, Screen.height - screenPos.y));
                            //							Debug.Log("Screen position is:" + screenPos);
                            //							Debug.Log("GUI position is:" + pos);
                            if (!isSpeaking)
                            {
                                GUI.Box(new Rect(pos.x, pos.y, chatBubbleWidth, textHeight + 20), content);
                            }
                            else
                            {
                                GUI.Box(new Rect(pos.x, pos.y, chatBubbleWidth, textHeight + 20), content, "SpeakingBubble");
                            }
                        }
                    }

                }
            }
        }
        else if (!string.IsNullOrEmpty(currentUser)) // just display username
        {
            GUI.skin = skin;
            GUIContent content = new GUIContent(currentUser);
            Vector2 textSize = skin.GetStyle("NameTag").CalcSize(content);
            Vector3 bubblePos = transform.position + new Vector3(0, nameTagHeight, 0);  //We ajust world Y position here to make chat bubble near the head of the model
            if (playerCamera == null)
            {
                playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            if (playerCamera != null && playerCamera.GetComponent<Camera>().enabled)
            {
                Vector3 screenPos = playerCamera.GetComponent<Camera>().WorldToScreenPoint(bubblePos);

                // We render our text only if it's in the screen view port	
                if (screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height && screenPos.z >= 0)
                {
                    if (localPlayer != null)
                    {
                        if (currentDistance < fadeDistance)
                        {
                            guiColor = GUI.color;
                            guiTextColor = GUI.contentColor;

                            guiColor.a = currentAlpha;
                            guiTextColor.a = currentAlpha;

                            GUI.color = guiColor;
                            GUI.contentColor = guiTextColor;

                            Vector2 pos = GUIUtility.ScreenToGUIPoint(new Vector2(screenPos.x, Screen.height - screenPos.y));
                            //							Debug.Log("Screen position is:" + screenPos);
                            //							Debug.Log("GUI position is:" + pos);
                            if (!isSpeaking)
                            {
                                GUI.Box(new Rect(pos.x - textSize.x / 2, pos.y, textSize.x + 5, textSize.y + 20), content);
                            }
                            else
                            {
                                GUI.Box(new Rect(pos.x - textSize.x / 2, pos.y, textSize.x + 5, textSize.y + 20), content, "SpeakingBubble");
                            }
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        // Here we count the time to display the message
        if (str != "")
        {
            bubbleTime += Time.deltaTime;
            if (bubbleTime > showTime)
            {
                bubbleTime = 0;
                str = "";
            }
        }
        // Fade out based on distance preferences
        currentDistance = Vector3.Distance(transform.position, localPlayer.transform.position);
        if (currentDistance > fullyVisibleDistance && currentDistance < (fadeDistance + fullyVisibleDistance))
        {
            currentAlpha = 1 - currentDistance / (fadeDistance + fullyVisibleDistance);
        }
        else if (currentDistance <= fullyVisibleDistance)
            currentAlpha = 1;
        else
            currentAlpha = 0;
    }

    // Function to be called if we want to show new bubble
    void ShowBubble(string bubbleMessage)
    {
        bubbleTime = 0;
        this.str = bubbleMessage;
    }
    // Set the display name
    public void SetDisplayName(IJibePlayer user)
    {
        currentUser = user.Name;
    }

    public void SetSpeaking(bool userIsSpeaking)
    {
        Debug.Log(currentUser + " speaking: " + userIsSpeaking);
        isSpeaking = userIsSpeaking;
    }
}
