using UnityEngine;
using System.Collections;

public class follow : MonoBehaviour {
    private GameObject camToFollow;
	// Use this for initialization
	void Start () {
        camToFollow = GameObject.Find("Player Cam");
	}
	
	// Update is called once per frame
	void Update () {
        camToFollow = GameObject.Find("PlayerCam");
        if (camToFollow != null)
        {
            transform.position = camToFollow.GetComponent<Camera>().transform.position;
            transform.rotation = camToFollow.GetComponent<Camera>().transform.rotation;

        }

	}
}
