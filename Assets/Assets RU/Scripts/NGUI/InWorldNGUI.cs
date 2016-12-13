using UnityEngine;
using System.Collections;

public class InWorldNGUI : MonoBehaviour {
	Camera camera;
	// Use this for initialization
	void JibeInit () {
	camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();		
	}
	
	// Update is called once per frame
	void Update () {
		if(camera==null)
		{
			if(GameObject.FindGameObjectWithTag("MainCamera")!=null)
			{
				camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			}
		}
		if(Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if(hit.transform.GetComponent<UIButton>()!=null)
				{
					hit.transform.gameObject.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}
