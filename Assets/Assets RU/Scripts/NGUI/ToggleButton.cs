using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ToggleButton : MonoBehaviour {
	public List<GameObject> objectToToggle;
	private List<Vector3> startPosition = new List<Vector3>();
	private Vector3 endPosition = new Vector3(-1000,-1000,-1000);
	// Use this for initialization
	void Awake () {
		foreach(GameObject currentObject in objectToToggle)
		{
			startPosition.Add(currentObject.transform.localPosition);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress (bool isDown) {
		if(isDown==true)
		{
			Toggle();
		}
	}
	public void Toggle(bool toggle)
	{
		if(toggle)
		{
			int i=0;
			foreach(GameObject currentObject in objectToToggle)
			{
				if(currentObject.transform.localPosition!=endPosition)
				{
					startPosition[i]=currentObject.transform.localPosition;
					currentObject.transform.localPosition=endPosition;
				}
				i++;
			}
		}
		else
		{
			int i=0;
			foreach(GameObject currentObject in objectToToggle)
			{
				currentObject.transform.localPosition=startPosition[i];
				i++;
			}
		}
	}
	public void Toggle()
	{
		int i=0;
		foreach(GameObject currentObject in objectToToggle)
		{
			if(currentObject.transform.localPosition==endPosition)
			{
				//Chat is being toggled on
				currentObject.transform.localPosition=startPosition[i];
			}
			else
			{
				startPosition[i]=currentObject.transform.localPosition;				
				//Chat is being toggled off
				currentObject.transform.localPosition=endPosition;
			}
			i++;
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
