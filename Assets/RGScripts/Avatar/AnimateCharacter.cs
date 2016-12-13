/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * AnimateCharacter.cs Revision 1.4.1107.20
 * Controls the animation of Jibe avatar models */

using UnityEngine;
using System.Collections;
using System;

public class AnimateCharacter : MonoBehaviour
{

    public float runSpeedScale = 1.2f;
    public float walkSpeedScale = 0.2f;

    bool idleAnimOverride = false;
    float idleAnimOverrideDuration = 0.0f;
    float idleAnimOverrideCount = 0.0f;
    public string defaultIdleAnim = "idle";
    string currentIdleAnim = "idle";

    public string defaultWalkAnim = "walk";
    string currentWalkAnim = "walk";

    bool isIdle = false;
    public bool useIdleWakeupAnimation = true;
    private float idleWakeUpInterval = 45;
    public string idleWakeupAnimation = "idle2";
    float idleDuration = 0.0f;
    System.Random randomWakeUp;

    float gestureDuration = 6.0f;
	public bool walkForward=false;
	public bool walkBackward=false;
    Transform torso;
    PlayerMovement marioController;
    AnimationSynchronizer aniSync;
	MouseLook MouseLook;
	public bool GUIInputSelected=false;
	public bool isDancing=false;
	//ZPWH
	// this is all GUI stuff
	/*
	public void GuiWalkForward() //actually backwards
	{
		walkForward = true;
		PlayerCharacter character = gameObject.GetComponent<PlayerCharacter>();
		if(!character.IsAvatarFlying())
		{
			animation.CrossFade(currentWalkAnim);
		}
		
	}
	public void GuiWalkBackward() //actually forwards
	{
		walkBackward = true;
		PlayerCharacter character = gameObject.GetComponent<PlayerCharacter>();
		if(!character.IsAvatarFlying())
		{
			animation.CrossFade(currentWalkAnim);
		}
	}
	public void stopForward()
	{
		walkForward = false;
	}
	public void stopBackward()
	{
		walkBackward = false;
	}
	public void GuiStop()
	{
		if((!walkForward)&&(!walkBackward))
			animation.CrossFade(currentIdleAnim);
	}*/
    
	void StartDancing()
	{
		isDancing=true;
	}
	void Awake()
    {
        randomWakeUp = new System.Random();
        idleWakeUpInterval = randomWakeUp.Next(45) + 30;
        // By default loop all animations
        GetComponent<Animation>().wrapMode = WrapMode.Loop;
        currentIdleAnim = defaultIdleAnim;
        currentWalkAnim = defaultWalkAnim;
        // We are in full control here - don't let any other animations play when we start
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().Play(currentIdleAnim);
        marioController = GetComponent<PlayerMovement>();
        aniSync = GetComponent<AnimationSynchronizer>();
		if(GameObject.Find("localPlayer")!=null)
			MouseLook = GameObject.Find("localPlayer").GetComponent<MouseLook>();
    }

    public void WakeUp()
    {
        idleDuration = 0;
        Debug.Log("Animation Wakeup");
        GetComponent<Animation>().CrossFade(idleWakeupAnimation, 0.5f);
        SendMessage("SendAnimationMessage", idleWakeupAnimation);
        // use cunning technique to sleep this thread for the duration of the idle wakeup animation
        StartCoroutine(PlayIdleAnimationWakeup());
    }

    IEnumerator PlayIdleAnimationWakeup()
    {
        DateTime end = DateTime.Now.AddSeconds(GetComponent<Animation>()[idleWakeupAnimation].clip.length);
        while (DateTime.Now < end)
        {
            yield return 0;
        }
        Debug.Log("Finished with wakeup idle anim, reverting to normal");
        // Choose a new random interval before the next wakeup
        idleWakeUpInterval = randomWakeUp.Next(45) + 40;
        idleDuration = 0;
        isIdle = false;
    }
    public void EnableIdleWakeup(bool isEnabled)
    {
        useIdleWakeupAnimation = isEnabled;
    }

    public void DisableAC()
    {
        this.enabled = false;
        Debug.Log("Disabling AnimateCharacter");
    }

    void Update()
    {
		if(!GUIInputSelected)
		{
			if(MouseLook==null)
			{
				if(GameObject.Find("localPlayer")!=null)
				{
					MouseLook = GameObject.Find("localPlayer").GetComponent<MouseLook>();
				}
			}
	        if (marioController == null)
	            marioController = GetComponent<PlayerMovement>();
	
	        float currentSpeed = marioController.GetSpeed();
			
			//if not flying and on ground
	        if (!marioController.IsFlying() && marioController.IsGrounded())
	        {
	            // Fade in run
	            if (currentSpeed > marioController.walkSpeed)
	            {
					isDancing=false;
	                isIdle = false;
	                GetComponent<Animation>().CrossFade("run");
	                // We fade out jumpland quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage("run");
	            }
	            // Fade in walk
	            else if (currentSpeed > 0.1)
	            {
					isDancing=false;
	                isIdle = false;
	                GetComponent<Animation>().CrossFade(currentWalkAnim);
	                // We fade out jumpland realy quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage(currentWalkAnim);
	            }
	            else if (currentSpeed < -0.1)
	            {
					isDancing=false;
	                isIdle = false;
	                GetComponent<Animation>().CrossFade("walkback");
	                // We fade out jumpland realy quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage("walkback");
	            }
	            // Fade out walk and run
	            else
	            {
	                if (!isIdle)
	                {
	                    isIdle = true;
	                    GetComponent<Animation>().CrossFade(currentIdleAnim);
	                    aniSync.SendAnimationMessage(currentIdleAnim);
	                }
	                else
	                {
	                    // send an update at a less frequent interval
	                    // but do it via fixed update
	                }
	            }
	            //animation["run"].normalizedSpeed = runSpeedScale;
	            GetComponent<Animation>()["walk"].normalizedSpeed = walkSpeedScale;
	            if (marioController.IsJumping())
	            {
					isDancing=false;
	                isIdle = false;
	                GetComponent<Animation>().CrossFade("jump", 0.2f);
	                aniSync.SendAnimationMessage("jump");
	            }
	        }
			//if not flying and not on ground		
	        else if (!marioController.IsFlying() && !marioController.IsGrounded())
	        {
	            /*if (!marioController.IsJumping())
	            {
	                // mid-jump
	                isIdle = false;
	                animation.CrossFade("hover", 0.5f);
	                aniSync.SendAnimationMessage("hover");
	            }
	            else
	            {*/
					isDancing=false;
	                isIdle = false;
	                GetComponent<Animation>().CrossFade("jump", 0.2f);
	                aniSync.SendAnimationMessage("jump");
	        //    }
	        }
			//if flying
	        else if (marioController.IsFlying())
	        {
				/*if(MouseLook.firstPerson)
				{
					animation.CrossFade("hover", 0.4f);
				}*/
	            // Fade in run
	            if (currentSpeed > marioController.walkSpeed)
	            {
					isDancing=false;
	                isIdle = false;
					if(!MouseLook.firstPerson)
					{
		                GetComponent<Animation>().CrossFade("jump", 0.4f);
					}
	                // We fade out jumpland quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage("fly");
	            }
	            // Fade in walk
	            else if (currentSpeed > 0.1)
	            {
					isDancing = false;
	                isIdle = false;
					if(!MouseLook.firstPerson)
					{
		                GetComponent<Animation>().CrossFade("jump", 0.4f);
					}
	                // We fade out jumpland realy quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage("fly");
	            }
	            /*else if (currentSpeed < -0.1)
	            {
					isDancing=false;
	                isIdle = false;
	                animation.CrossFade("hover", 0.4f);
	                // We fade out jumpland realy quick otherwise we get sliding feet
	                //animation.Blend("jumpland", 0);
	                aniSync.SendAnimationMessage("hover");
	            }
	            // Fade out walk and run
	            else
	            {
	                if (!marioController.IsMovingVertically() && !marioController.IsSitting())
	                {
	                    if (!isIdle)
	                    {
	                        isIdle = true;
	                        animation.CrossFade("hover", 0.6f);
	                        aniSync.SendAnimationMessage("hover");
	                    }
	                    else
	                    {
	                        // send an update at a less frequent interval
	                        // but do it via fixed update
	                    }
	                }
	                else if (marioController.IsSitting())
	                {
	                    isIdle = true;
	                    SitAnimation(marioController.CurrentSitPose());
	                }
	                else
	                {
	                    isIdle = false;
						isDancing=false;
	                    animation.CrossFade("hover", 0.6f);
	                    aniSync.SendAnimationMessage("hover");
	                }
	            }*/
	
	
	        }
	
	        if (idleAnimOverride && !marioController.IsFlying())
	        {
	            idleAnimOverrideCount += Time.deltaTime;
	            if (idleAnimOverrideCount > idleAnimOverrideDuration)
	            {
	                currentIdleAnim = defaultIdleAnim;
	                idleAnimOverride = false;
	                idleAnimOverrideCount = 0.0f;
	                idleAnimOverrideDuration = 0.0f;
	            }
	        }
			if(isDancing)
			{
				GetComponent<Animation>().CrossFade("samba_dancing_1");
				isIdle = false;
				aniSync.SendAnimationMessage("samba_dancing_1");
			}
		}
    }

    void FixedUpdate()
    {
        if (isIdle && useIdleWakeupAnimation && !marioController.IsFlying())
        {
            idleDuration = idleDuration + (Time.deltaTime * 1);
            if (idleDuration > idleWakeUpInterval)
            {
                //Debug.Log(this.name + " is idle, playing animation " + currentIdleAnim);
                WakeUp();
            }
        }

    }

    void DidLand()
    {
        //animation.Play("jumpland");
        //SendMessage("SendAnimationMessage", "jumpland");
    }

    public void SetGestureLength(float duration)
    {
        gestureDuration = duration;
    }
    public void PlayGesture(string animName)
    {
		if(!marioController.IsFlying())
		{
	        idleAnimOverride = true;
	        idleAnimOverrideDuration = gestureDuration;
	        currentIdleAnim = animName;
	        GetComponent<Animation>().CrossFade(currentIdleAnim);
	        aniSync.SendAnimationMessage(currentIdleAnim);
		}
    }

    public void AnimOverride(string animName)
    {
        currentWalkAnim = animName;
        GetComponent<Animation>().CrossFade(currentWalkAnim);
        aniSync.SendAnimationMessage(currentWalkAnim);
    }

    public void SitAnimation(string animName)
    {
        currentIdleAnim = animName;
        GetComponent<Animation>().CrossFade(currentIdleAnim);
        aniSync.SendAnimationMessage(currentIdleAnim);
    }

    public void CancelSitAnimation()
    {
        currentIdleAnim = defaultIdleAnim;
        GetComponent<Animation>().CrossFade(currentIdleAnim);
        aniSync.SendAnimationMessage(currentIdleAnim);
    }

}
