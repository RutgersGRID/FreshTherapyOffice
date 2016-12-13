using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour {
	GameObject camera;
	public Vector3 rotationOffset = Vector3.zero;
	// Use this for initialization
	void Start () {
		camera = GameObject.FindWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		if(camera==null)
		{
			camera = GameObject.FindWithTag("MainCamera");
		}
		if(camera!=null)
		{
			transform.LookAt(camera.transform);
			transform.Rotate(rotationOffset);
		}
	}
}
