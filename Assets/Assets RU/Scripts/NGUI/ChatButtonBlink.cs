using UnityEngine;
using System.Collections;

public class ChatButtonBlink : MonoBehaviour {
	private bool blink = false;
	private UISlicedSprite myLabel;
	private bool isOpen;
	public GameObject window;
	// Use this for initialization
	void Start () {
		isOpen=true;
		StartCoroutine("DoBlink");
		myLabel=GetComponentInChildren<UISlicedSprite>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void BlinkMe(bool doBlink)
	{
		if(isOpen)
		{
			blink = doBlink;
		}
	}

	IEnumerator DoBlink() {
		while(true)
		{
			if(blink && window.transform.localPosition==new Vector3(-1000,-1000,-1000))
			{
				yield return new WaitForSeconds(.5f);
				myLabel.color = Color.red;
			}
			else
			{
				blink=false;
			}
			yield return new WaitForSeconds(.5f);
			myLabel.color = Color.white;
		}
	}
}
