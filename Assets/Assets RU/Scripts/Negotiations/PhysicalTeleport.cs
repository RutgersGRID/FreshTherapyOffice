using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PhysicalTeleport : MonoBehaviour {
	public List<GameObject> objectsToDisable = new List<GameObject>();
	public List<GameObject> objectsToEnable = new List<GameObject>();
	public string vivoxChannelToJoin;
	public Vector3 positionToGoTo;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnMouseDown()
	{
		if(!Test.isHitting)
		{
			GameObject.Find("localPlayer").transform.position=positionToGoTo;
			GameObject.Find("localPlayer").transform.Translate(new Vector3(0, 0, 0));
			GameObject.Find("localPlayer").transform.GetChild(0).GetComponent<AnimateCharacter>().CancelSitAnimation();
		}
	}
}
