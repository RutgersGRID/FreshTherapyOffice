using UnityEngine;
using System.Collections;

public class DestroyOnGroupInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void GroupInit(string filler)
	{
		
		Debug.Log("Finished loading");
		Destroy(this.gameObject);
	}
}
