/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * TeleportOnClick.cs Revision 1.4.1106.22
 * Used for teleporting to specific locations in the same scene  */

using UnityEngine;
using System.Collections;

public class TeleportOnClick : MonoBehaviour {

    private TeleportLinks teleportLinkController;
    public GameObject cursor;

    void OnStart()
    {
        if (cursor == null)
        {
            cursor = GameObject.Find("Cursor");
        }
    }
    void OnMouseExit()
    {
        if (cursor != null)
        {
            cursor.SendMessage("ShowTeleportCursor", false);
        }
    }
    void OnMouseOver()
    {
        if (cursor != null)
        {
            cursor.SendMessage("ShowTeleportCursor", true);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckTeleportAbility())
            {
                teleportLinkController.DoTeleport(transform);
            }
        }
    }

    bool CheckTeleportAbility()
    {
        // set up the teleport link controller
        if (teleportLinkController == null)
        {
            GameObject uiBase = GameObject.Find("UIBase");
            if (uiBase != null && uiBase.GetComponent<TeleportLinks>() != null)
            {
                teleportLinkController = uiBase.GetComponent<TeleportLinks>();
            }
        }
        return teleportLinkController != null; // if the teleport link controller has been found, this method returns true - you can teleport!
    }
}
