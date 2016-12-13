using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CloseButton : MonoBehaviour {
	public List<GameObject> objectsToClose;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnPress (bool isPressed)
	{
		foreach(GameObject currentObject in objectsToClose)
		{
			currentObject.transform.position=new Vector3(-1000f,-1000f,-1000f);
		}
	}
	void OnMouseDown()
	{
		foreach(GameObject currentObject in objectsToClose)
		{
			currentObject.transform.position=new Vector3(-1000f,-1000f,-1000f);
		}
	}
}
