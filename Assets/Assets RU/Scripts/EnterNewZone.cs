using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnterNewZone : MonoBehaviour {
	public List<GameObject> objectsToDisable = new List<GameObject>();
	public List<GameObject> objectsToEnable = new List<GameObject>();
	public string vivoxChannelToJoin;
	public Vector3 positionToGoTo;
	public bool isInTrigger;
	public static bool isReady;
	void OnTriggerEnter()
	{
		isInTrigger=true;
		StartCoroutine("SwitchToNewZone");
		Debug.Log("Entering new zone");
	}
	void OnTriggerExit()
	{
		Debug.Log("Leaving zone");
		isInTrigger=false;
	}

	//we handle leaving a room this way to help with the edge case where you are rapidly switching rooms.  
	//rapidly switching rooms could cause vivox to fail to connect to either, or get confused as to which is the proper room
	IEnumerator SwitchToNewZone()
	{
		while(true)
		{
			if(isReady)
			{
				if(isInTrigger)
				{
					GameObject.Find("VivoxHud").GetComponent<VivoxHud2>().SwitchToChannel(vivoxChannelToJoin); //toggling vivox =channels
					Debug.Log("VivoxChannelToJoin: " + vivoxChannelToJoin);
					foreach(GameObject currentObject in objectsToDisable)
					{
						currentObject.active=false;
					}
					foreach(GameObject currentObject in objectsToEnable)
					{
						currentObject.active=true;
					}
				}
				StopCoroutine("SwitchToNewZone");
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
