using UnityEngine;
using System.Collections;

public class PlaqueCursor : MonoBehaviour {
	public GameObject cursor;
	public string myGroup;
	// Use this for initialization
	void Start () {
		myGroup = PlayerPrefs.GetString("Group");
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Camera.main!=null)
		{
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if(hit.transform.tag=="plaque") //the plaques will only be tagged if I am in the correct group already
				{
					if(!Test.isHitting)
					{
						cursor.SendMessage("ShowPlaqueCursor", true);
					}
				}
				else
				{
					cursor.SendMessage("ShowPlaqueCursor", false);
				}
			}
			else
			{
				cursor.SendMessage("ShowPlaqueCursor", false);
			}
		}
	}
}
