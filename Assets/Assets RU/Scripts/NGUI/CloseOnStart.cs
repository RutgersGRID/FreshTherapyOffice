using UnityEngine;
using System.Collections;

public class CloseOnStart : MonoBehaviour {
	// Use this for initialization
	void Start () {
		StartCoroutine("Close");
	}
	
	IEnumerator Close() {
		yield return new WaitForSeconds(.01f);
		this.gameObject.SendMessage("Toggle", true);
	}
	// Update is called once per frame
	void Update () {
	
	}
}
