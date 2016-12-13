/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * TeleportLinks.cs Revision 1.4.1106.22
 * Used for teleporting to different locations in the same scene  */

using UnityEngine;
using System.Collections;
using System;
public class TeleportLinks : MonoBehaviour 
{

    public Transform[] teleportDestinations; // Array of locations for the user to teleport to
    public GUISkin skin;
    public int spawnVariance = 0; // vary landing point a little so players are less likely to land on top of each other
    private static System.Random random = new System.Random();
    public bool showTeleportParticleEffect = true;
    public string teleportDestinationPrefix = "_Teleport"; // a simple naming convention to make it easier to find teleport destinations in the scene
    public GameObject teleportParticleGenerator;
	public int teleport = 0;
	
	public void TurnOn1()
	{
		teleport = 1;
	}
	public void TurnOn2()
	{
		teleport = 2;
	}
	public void TurnOn3()
	{
		teleport = 3;
	}
	public void TurnOn4()
	{
		teleport = 4;
	}
	public void TurnOn5()
	{
		teleport = 5;
	}
	public void TurnOn(int i)
	{
		if(i<teleportDestinations.Length)
		{
			teleport = i;
		}
	}
	public void TurnOff()
	{
		teleport = 0;
	}
/*    void OnGUI()
    {
        GUI.skin = skin;
        if(teleport != 0)
		{
            DoTeleport(teleportDestinations[teleport-1]);
        }
    }
	*/
    public void DoTeleport(Transform target)
    {
        string localPlayerName = "localPlayer";
        GameObject localPlayer = GameObject.Find(localPlayerName);
        float offsetX = random.Next(spawnVariance);
        float offsetZ = random.Next(spawnVariance);
        Vector3 newPosition = target.position;
        newPosition.x = target.position.x + offsetX;
        newPosition.z = target.position.z + offsetZ;
        localPlayer.transform.position = newPosition;
        localPlayer.transform.rotation = target.rotation;
        if (showTeleportParticleEffect)
        {
            if (teleportParticleGenerator != null)
            {
                // Generate particles at destination
                Instantiate(teleportParticleGenerator, newPosition, target.rotation);
            }
        }
		
		//TODO: add snap to back
		
		//GameObject.Find("PlayerCam").GetComponent("PlayerCamera").SendMessage("SnapToBack");
		
		//GameObject cam = GameObject.Find("PlayerCam");
		localPlayer.SendMessage("SetCurrentTransformAsDefault");
		TurnOff();
    }
}