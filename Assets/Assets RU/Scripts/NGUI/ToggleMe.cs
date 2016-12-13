using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ToggleMe : MonoBehaviour {
	public Vector3 startPosition;
	// Use this for initialization
	void Awake () {
		startPosition = this.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Toggle(bool toggle) {
		if(!toggle)
		{
			this.transform.localPosition = startPosition;
		}
		else if(this.transform.localPosition!=new Vector3(-1000f,-1000f,-1000f))
		{
			startPosition = this.transform.localPosition;
			this.transform.localPosition = new Vector3(-1000f,-1000f,-1000f);
		}
	}
	
	public void GroupInit(string groupName)
	{
		List<string> myGroups = GetComponent<StoreGroups>().myGroups;
		foreach(string currentGroup in myGroups)
		{
			if(currentGroup.Equals(groupName))
			{
				Toggle(true);
			}
		}
	}	
}
