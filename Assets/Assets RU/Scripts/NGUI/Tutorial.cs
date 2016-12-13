using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Tutorial : MonoBehaviour {
	public List<string> tutorialSprites;
	public List<string> adminSprites;
	public UIAtlas theAtlas;
	private UISlicedSprite mySlicedSprite;
	public int index = 0; //change this if you don't want it to start at the first image
	// Use this for initialization
	void Start () {
		mySlicedSprite = GetComponent<UISlicedSprite>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Next() {
		index++;
		if(index>=tutorialSprites.Count)
		{
			index=0;
		}
		mySlicedSprite.sprite=mySlicedSprite.atlas.GetSprite(tutorialSprites[index]);
		mySlicedSprite.spriteName=tutorialSprites[index];
	}
	public void Back() {
		index--;
		if(index<0)
		{
			index=tutorialSprites.Count-1;
		}
		mySlicedSprite.sprite=mySlicedSprite.atlas.GetSprite(tutorialSprites[index]);
		mySlicedSprite.spriteName=tutorialSprites[index];
	}
	
	public void GroupInit(string groupName)
	{
		if(groupName=="GlobalChat")
		{
			tutorialSprites.AddRange(adminSprites);
		}
	}
}
