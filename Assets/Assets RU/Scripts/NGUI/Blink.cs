using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour {
	private bool blink = false;
	private UISlicedSprite myLabel;
	// Use this for initialization
	void Start () {
		StartCoroutine("DoBlink");
		myLabel=GetComponentInChildren<UISlicedSprite>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void BlinkMe(bool doBlink)
	{
		blink = doBlink;
	}
	IEnumerator DoBlink() {
		while(true)
		{
			if(blink)
			{
				yield return new WaitForSeconds(.5f);
				myLabel.color = Color.red;
			}
			yield return new WaitForSeconds(.5f);
			myLabel.color = Color.white;
		}
	}
}
