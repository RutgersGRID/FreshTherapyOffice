using UnityEngine;
using System.Collections;

public class ScrollUp : MonoBehaviour {
	public GameObject objectToScroll;
	public float distanceToScroll=1f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnMouseDown()
	{
		objectToScroll.transform.position = new Vector3(objectToScroll.transform.position.x,objectToScroll.transform.position.y+distanceToScroll, objectToScroll.transform.position.z);
	}
	
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			OnMouseDown();
		}
	}
}
