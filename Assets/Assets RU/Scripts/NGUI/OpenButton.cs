using UnityEngine;
using System.Collections;

public class OpenButton : MonoBehaviour {
	Vector3 startPosition;
	public GameObject objectToOpen;
	// Use this for initialization
	void Start () {
		startPosition = objectToOpen.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			objectToOpen.transform.localPosition=startPosition;
		}
	}
	void GroupInit(string myGroup)
	{
		bool found=false;
		foreach(string currentGroup in GetComponent<StoreGroups>().myGroups)
		{
			if(myGroup==currentGroup)
			{
				found=true;
			}
		}
		if(!found)
		{
			startPosition= new Vector3(-1000,-1000,-1000);
		}
	}
}
