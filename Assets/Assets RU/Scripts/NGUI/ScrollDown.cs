using UnityEngine;
using System.Collections;

public class ScrollDown : MonoBehaviour {
	public GameObject objectToScroll;
	public float minYValue=199.4f;
	public static bool resetPosition=false;
	public static bool clearThisLateUpdate=false;
	public float distanceToScroll=1f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		clearThisLateUpdate=false;
		if(Plaque.cleartext)
		{
			clearThisLateUpdate=true;
		}
	}
	void LateUpdate() {
		if(clearThisLateUpdate)
		{
			objectToScroll.transform.position= new Vector3(objectToScroll.transform.position.x,minYValue,objectToScroll.transform.position.z);
		}
	}
	void OnMouseDown()
	{
		objectToScroll.transform.position = new Vector3(objectToScroll.transform.position.x,objectToScroll.transform.position.y-distanceToScroll, objectToScroll.transform.position.z);
		if(objectToScroll.transform.position.y<minYValue)
		{
			objectToScroll.transform.position= new Vector3(objectToScroll.transform.position.x,minYValue,objectToScroll.transform.position.z);
		}
	}

	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
			OnMouseDown();
		}
	}
}
