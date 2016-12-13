using UnityEngine;
using System.Collections;

public class LinkLabels : MonoBehaviour {
	public UILabel labelToCopyFrom;
	private string lastText;
	private UILabel myLabel;
	// Use this for initialization
	void Start () {
		lastText=labelToCopyFrom.text;
		myLabel=GetComponent<UILabel>();
		myLabel.text=lastText;
	}
	
	// Update is called once per frame
	void Update () {
		if(labelToCopyFrom.text!=lastText)
		{
			lastText=labelToCopyFrom.text;
			myLabel.text=lastText;
		}
	}
}
