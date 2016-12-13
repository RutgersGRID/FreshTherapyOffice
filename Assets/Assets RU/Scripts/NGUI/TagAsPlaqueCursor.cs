using UnityEngine;
using System.Collections;

public class TagAsPlaqueCursor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void GroupInit(string myGroup)
	{
		foreach(string currentGroup in GetComponent<StoreGroups>().myGroups)
		{
			if(myGroup==currentGroup)
			{
				this.transform.tag="plaque";
			}
		}
	}
}
