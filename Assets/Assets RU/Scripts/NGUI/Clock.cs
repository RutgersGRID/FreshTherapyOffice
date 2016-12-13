using UnityEngine;
using System.Collections;
using System;
using System.Threading;
public class Clock : MonoBehaviour {
public UILabel label;
private UIGrid grid;
	void Start() 
	{
		grid = GameObject.Find("TabGrid").GetComponent<UIGrid>();
	}
	// Use this for initialization
	void Update()
	{
		grid.repositionNow=true;
	    DateTime today = System.DateTime.Now;
	    label.text = today.ToString("h:mm tt");
	}
}
