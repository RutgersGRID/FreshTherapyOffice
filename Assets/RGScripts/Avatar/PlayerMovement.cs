/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * PlayerMovement.cs Revision 1.4.1107.20
 * Control movement messages over the network  */

using UnityEngine;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour {

    private bool isMoving;
    private bool isTurning;
    private bool isSitting = false;
    private bool isGrounded = true;
    private float horizontalInput;
    private float verticalInput;
    // Specify some general movement parameters
    public float walkSpeed = .05f;
    public float runSpeed = 1f;
    public float runAfter = 400.0f;
    private KeyCode runHotKey = KeyCode.LeftShift;
    private GameObject netController;
    private bool sendTransforms = false;

    private bool isChatting = false;
    private bool isIdle = false;
    private float idleDuration = 0.0f;
    private float idleWakeup = 30.0f;
    private CharacterController character;
    private Rigidbody rb;
    private float walkDuration = 0.0f;
    private bool isFlying = false;
    private bool isJumping = false;
    private float jumpDuration = 0.0f;
    private bool hasLanded = false;
    private double lastY = 0.0;
    private bool isMovingVertically = false;
    private string currentSitPose = "";
	private CharacterController charController;
	private bool guiForward = false;
	void Start () 
    {
		if(GameObject.Find("localPlayer")!=null)
		{
			charController = GameObject.Find("localPlayer").GetComponent<CharacterController>();
		}
        netController = GameObject.Find("NetworkController");
        character = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        lastY = transform.position.y;
	}
    public void SendTransforms(bool send)
    {
        sendTransforms = send;
    }
	void Update () 
    {
		//turn flying on
		if(Input.GetKey("e"))
		{
			isFlying = true;
		}
				
		//Debug.Log("isGrounded = " + isGrounded);
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (character != null)
        {
            isFlying = !character.isGrounded;
            isGrounded = character.isGrounded;
        }
        else if (rb != null)
        {
            isFlying = !rb.useGravity;
        }
        isMoving = Mathf.Abs(verticalInput) > 0.1;
        isMovingVertically = (lastY != Math.Round(transform.position.y));
        lastY = Math.Round(transform.position.y);
        isTurning = Mathf.Abs(horizontalInput) > 0.1;
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1) || guiForward)
        {
            isMoving = true;            
        }
/*        if (isMoving)
            walkDuration += Time.deltaTime;
        else
            walkDuration = 0.0f;*/
		if(Input.GetKeyUp(KeyCode.LeftShift))
		{
			walkDuration = 0.0f;
		}
        if (Input.GetKey(runHotKey))//When pushed down, avatar will run
        {
            walkDuration = runAfter + 0.1f;
        }
        //isGrounded = isFlying || isJumping;
        if (sendTransforms && (isMoving || isTurning))
        {
            float speed = GetSpeed();
            UpdateTransform(transform);
            idleDuration = idleWakeup + 1; // reset idle wakeup while moving so as soon as player stops moving, they send an idle packet.

            if (speed > walkSpeed)
            {
                if (!isFlying)
                {
                    GetComponent<AnimationSynchronizer>().SendAnimationMessage("run");
                }
                else
                {
                    GetComponent<AnimationSynchronizer>().SendAnimationMessage("fly");
                }
            }
            else if (speed > 0.1)
            {
                if (!isFlying)
                {
                    GetComponent<AnimationSynchronizer>().SendAnimationMessage("walk");
                }
                else
                {
                    GetComponent<AnimationSynchronizer>().SendAnimationMessage("fly");
                }
            }
            else if (speed < -0.1)
            {
                GetComponent<AnimationSynchronizer>().SendAnimationMessage("walk");
            }
            else if (isSitting)
            {
                GetComponent<AnimationSynchronizer>().SendAnimationMessage(CurrentSitPose());
            }
            else
            {
                if (!isIdle)
                {
                    GetComponent<AnimationSynchronizer>().SendAnimationMessage("idle");
                    isIdle = true;
                }
            }
        }
        if (!(isMoving || isTurning))
        {
            idleDuration += Time.deltaTime;
            if (idleDuration > idleWakeup)
            {
                if (sendTransforms) GetComponent<AnimationSynchronizer>().SendAnimationMessage("idle");
                idleDuration = 0.0f;
                isIdle = false;
            }
            else
            {
                isIdle = true;
            }
        }
        if (!isJumping && !isFlying && Input.GetButton("Jump"))
        {
			if(!charController.isGrounded)
			{
	            isJumping = true;
			}
            isGrounded = false;
        }
        else
        {
            jumpDuration += Time.deltaTime;
            if (jumpDuration > 0.2f)
            {
                isJumping = false;
                jumpDuration = 0.0f;
            }
        }
		
	}
     //Check for collisions to determine when the player is on the ground
    void OnCollisionEnter(Collision collision)
    {
        float collisionAngle = Vector3.Angle(collision.contacts[0].normal, Vector3.up); //Calculate angle between the up vector and the collision contact's normal
        if (collisionAngle < 40.0)
        { //If the angle difference is small enough, accept collision as hitting the ground
            if (!isGrounded) // just landed
            {
				Debug.Log("HIT");
                isGrounded = true; //Player is grounded
                isFlying = false;
                hasLanded = true;
            }
        }
    }
	
	//ZPWH
	//this is a band-aid since char controllers cant use OnCollision functions
	public void IsOnGround()
	{
		isGrounded = true;
		isFlying = false;
		hasLanded = true;
	}
	
    public bool IsMoving()
    {
        return isMoving;
    }
    public bool IsMovingVertically()
    {
        return isMovingVertically;
    }
    public bool IsTurning()
    {
        return isTurning;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetSpeed()
    {
        if (isMoving)
        {
            return walkSpeed;
        }
        else return 0;
    }

    public void UpdateTransform(Transform t)
    {
        netController.GetComponent("NetworkController").SendMessage("SendTransform", t);
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public bool IsFlying()
    {
        return isFlying;
    }
    public bool IsHovering()
    {
        return isJumping && !isFlying || isFlying && !isMoving;
    }
    public void SetChatting(bool chatting)
    {
        isChatting = chatting;
    }

    public bool IsChatting()
    {
        return isChatting;
    }

    public bool HasJumpReachedApex()
    {
        return false;
    }
    public bool IsGroundedWithTimeout()
    {
        return false;
    }
    public bool HasLanded()
    {
        return hasLanded;
    }
    public bool IsSitting()
    {
        return isSitting;
    }
    public string CurrentSitPose()
    {
        return currentSitPose;
    }
    public void SetSitting(bool sitting, string sitPose)
    {
        isSitting = sitting;
        currentSitPose = sitPose;
    }
	public void ForwardInput()
	{
		guiForward=!guiForward;
	}
}
