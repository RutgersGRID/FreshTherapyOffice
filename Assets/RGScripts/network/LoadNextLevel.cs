/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * LoadNextLevel.cs Revision 1.0.1103.01
 * Portal teleport to another level - use this for a proximity detection teleporter */

using UnityEngine;
using System;
using System.Collections;

public class LoadNextLevel : MonoBehaviour
{
    public string nextLevel = "Loader";
    private bool showNextLevelButton;
    public GUISkin skin;
    public Texture levelImage;
    public string nextLevelPrompt = "Go to ";
    private string nextLevelPromptDisplay;
    private string loadProgress = "0";
    public NetworkController networkController;
	public bool instantTeleport = false;

    void FixedUpdate()
    {
        // Handy way to test whether the next level is ready (if you are using a streamed web player deployment)
        int progress = (int)Math.Round(100 * Application.GetStreamProgressForLevel(nextLevel));
        loadProgress = "Loading " + progress.ToString() + "%";
    }

    void OnTriggerEnter(Collider other)
    {
        // Proximity trigger
		if (instantTeleport)
		{
			networkController.ChangeLevel(nextLevel);
		}
		else
		{
			showNextLevelButton = true;
		}
    }
    void OnTriggerExit(Collider other)
    {
        showNextLevelButton = false;
    }

    public void ChangeDestination(string newDestination)
    {
        // Use this method from another script if you want to dynamically change the destination - this has been done in the past
        // to control a group of students in a classroom. The teacher could select a new destination from their hud and the student's
        // instances of jibe would be updated to choose a new destination through the portal. This sort of update needs to be handled
        // via network messages and a custom flag to detect if a user is a teacher.
        nextLevel = newDestination;
    }

    void OnGUI()
    {
        GUI.skin = skin;
        int buttonWidth = 235;
        int buttonHeight = 76;

        if (showNextLevelButton)
        {
            if (Application.CanStreamedLevelBeLoaded(nextLevel))
            {
                nextLevelPromptDisplay = nextLevelPrompt + nextLevel;

                GUIContent content;
                if (levelImage != null)
                {
                    content = new GUIContent(nextLevelPromptDisplay, levelImage, nextLevelPrompt);
                }
                else
                {
                    content = new GUIContent(nextLevelPromptDisplay, nextLevelPrompt);
                }

                if (GUI.Button(new Rect((Screen.width / 2) - (buttonWidth / 2), (Screen.height / 2) - (buttonHeight / 2), buttonWidth, buttonHeight), content, "PortalLinkButton") || EnterPressed())
                {
                    if (networkController != null)
                        networkController.ChangeLevel(nextLevel);
                }

            }
            else
            {
                GUIContent content = new GUIContent(loadProgress);
                GUI.Label(new Rect((Screen.width / 2) - (buttonWidth / 2), (Screen.height / 2) - (buttonHeight / 2), buttonWidth, buttonHeight), content, "PortalLinkButton");
            }
        }        
    }
    private bool EnterPressed()
    {
        return (Event.current.type == EventType.keyDown && Event.current.character == '\n');
    }
}
