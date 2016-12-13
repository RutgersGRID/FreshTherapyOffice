using UnityEngine;
using System.Collections;
using System;
public class HACKTOFIXBUG31 : MonoBehaviour {
	private DateTime starttime;
	// Use this for initialization
	void Start () {
		starttime=System.DateTime.Now;
	}
	
	// Update is called once per frame
	void Update () {
		if(System.DateTime.Now-starttime>TimeSpan.FromMinutes(5.0))
		{
			Application.ExternalEval("location.reload()");
		}
	}
}
