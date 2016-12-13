using UnityEngine;
using System.Collections;

public class IgnoreCharacterCollisions : MonoBehaviour {
	Transform localPlayer;
	// Use this for initialization
	void Start () {
			
		if(GameObject.Find("localPlayer")!=null)
		{
			localPlayer = GameObject.Find("localPlayer").transform;
			//Debug.Log("Found localPlayer!");
			//Physics.IgnoreCollision(localPlayer.GetComponent<Collider>(), this.GetComponent<Collider>());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(GameObject.Find("localPlayer")!=null)
		{
			//Debug.Log("Looking for localPlayer");
			localPlayer = GameObject.Find("localPlayer").transform;	
			if(localPlayer!=null)
			{
				//Debug.Log("Found localPlayer!");
				Physics.IgnoreCollision(localPlayer.GetComponent<Collider>(), this.GetComponent<Collider>());
			}
		}
	}
}
