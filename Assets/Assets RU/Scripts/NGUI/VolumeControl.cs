using UnityEngine;
using System.Collections;

public class VolumeControl : MonoBehaviour {
    public int volume = 1;
    private float currentVolume = 1.0f;
    private float volumeSettings = 1.0f;
	public UISlicedSprite theSprite;
	public UIAtlas theAtlas;
	public GameObject jibe;
	public VivoxHud2 vivoxController;
	// Use this for initialization
	void Start () {
		vivoxController = GameObject.Find("VivoxHud").GetComponent<VivoxHud2>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPress(bool isPressed)
	{
		if(isPressed==true)
		{
            if (volume < volumeSettings)
            {
                volume++;
				vivoxController.HandleMuting(false);
            }
            else
            {
                volume = 0;
				vivoxController.HandleMuting(true);
            }
            currentVolume = volume / volumeSettings;
            Debug.Log("Target volume: " + currentVolume);
            if (jibe == null)
            {
                // should only need to do this once
                jibe = GameObject.Find("Jibe");
            }
            if (jibe != null)
            {
                if (jibe.GetComponent<AudioSource>() != null)
                    jibe.GetComponent<AudioSource>().volume = currentVolume;
            }
			if(volume==0)
			{
				theSprite.sprite=theAtlas.GetSprite("LowVolume");
			}
			else if(volume==1)
			{
				theSprite.sprite=theAtlas.GetSprite("MediumVolume");
			}
			else
			{
				theSprite.sprite=theAtlas.GetSprite("LoudVolume");
			}
		}
	}
}
