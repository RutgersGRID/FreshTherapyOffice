using UnityEngine;
using System.Collections;

public class questionMark : MonoBehaviour {
	public bool visible=false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnClick () {
		Debug.Log("Hi");
		visible=!visible;
	}
}
