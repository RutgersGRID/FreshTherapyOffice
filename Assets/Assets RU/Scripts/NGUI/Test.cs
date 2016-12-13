using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	GameObject camera;
	public static bool isHitting;
	// Use this for initialization
	void Start () {
		camera = GameObject.FindGameObjectWithTag("NGUICamera");
	}
	
	// Update is called once per frame
	void Update () {
		MouseLook.canZoom=true;
		RaycastHit hit;
		if(Physics.Raycast(camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
		{
			if(hit.transform.GetComponent<UITextList>()!=null)
			{
				MouseLook.canZoom=false;
			}
			isHitting=true;
		}
		else
		{
			isHitting=false;
		}
	}

}
