/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * PlayerCharacter.cs Revision 1.4.1106.11
 * Controls how a player moves */

using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour
{
    // Character Properties
    public float groundSpeed = 6.0f; //Force applied when grounded
    public float airSpeed = 2.0f; //Force applied when in air
    public float jumpSpeed = 30.0f; //Force applied when jumping
    public float horizontalRotationSpeed = 100.0f; //Horizontal rotation speed of the player
    public float onGroundDrag = 2.0f;//Rigidbody drag used on ground
    public float inAirDrag = .1f; //Rigidbody drag used in air

    private bool onGround = false;
    private bool isFlying = false;
    private bool jump = false;
    private Rigidbody rb;
    private Transform t;

    private PlayerMovement movementController;
    private bool isReady = false;
    private bool isRotating = false;
    private bool isMoving = false;
    private Transform movementPivot;
    private bool usePhysicalCharacter = true;
    private bool enableFly = true;

    // Non-physical Character Properties
    private CharacterController character;
    private Vector3 movement; //Movement vector
    private Vector3 gravity; //Gravity vector
    private float forwardFacingSpeed = 0.2f;
    private bool hasLanded = false;
    private bool _flyCamEnabled = false;
	public int verticalmodifier = 0;
	public bool changeUp = false;
	public bool changeDown = false;
	public bool walkForward = false;
	public bool walkBackward = false;
	public bool rotateRight = false;
	public bool rotateLeft = false;
	//ZPWH
	//see if avatar is flying on change scene
	public void UpModify()
	{
		if(changeUp)
			changeUp = false;
		else changeUp = true;
	}
	public void DownModify()
	{
		if(changeDown)
			changeDown = false;
		else changeDown = true;
	}
	public void WalkForward()
	{ 
		if(walkForward)
			walkForward = false;
		else walkForward = true;
	}
	public void WalkBackward()
	{ 
		if(walkBackward)
			walkBackward = false;
		else walkBackward = true;
	}
	public void RotateRight()
	{
		if(rotateRight)
			rotateRight = false;
		else rotateRight = true;
	}
	public void RotateLeft()
	{
		if(rotateLeft)
			rotateLeft = false;
		else rotateLeft = true;
	}
	public bool IsAvatarFlying()
	{
		return isFlying;
	}
	//on return to scene, if was flying, make fly again
	public void MakeAvatarFly()
	{
		rb.useGravity = false; 
        isFlying = true;
	}


    public void ConfigurePlayerCharacter(bool physicalCharacter, bool enableFlying)
    {
        usePhysicalCharacter = physicalCharacter;
        enableFly = enableFlying;

//        Debug.Log("Configuring player - character set to physical = " + usePhysicalCharacter);
        float capheight = 1.7f;
        float capradius = 0.25f;
        Vector3 capcenter = new Vector3(0, 0.85f, 0);
        if (usePhysicalCharacter)
        {
            rb = GetComponent<Rigidbody>(); //Cache rigidbody component
            if (rb == null)
            {
//                Debug.Log("No rigidbody on this object, adding");
                character = gameObject.GetComponent<CharacterController>();
                if (character != null)
                {
                    capheight = character.height;
                    capradius = character.radius;
                    capcenter = character.center;
                    Destroy(character);
                }
                rb = gameObject.AddComponent<Rigidbody>();
                rb.inertiaTensor = new Vector3(capradius, capheight, capradius);
                rb.mass = 0.25f;
                rb.drag = 0.0f;
                rb.angularDrag = 0.05f;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            CapsuleCollider cap = gameObject.GetComponent<CapsuleCollider>();
            if (cap == null)
            {
                cap = gameObject.AddComponent<CapsuleCollider>();
                cap.isTrigger = false;
                cap.radius = capradius;
                cap.height = capheight;
                cap.direction = 1;
                cap.center = capcenter;
            }
        }
        else
        {
            Destroy(GetComponent<Rigidbody>());
            CapsuleCollider cap = gameObject.GetComponent<CapsuleCollider>();
            if (cap != null)
            {
                capheight = cap.height;
                capradius = cap.radius;
                capcenter = cap.center;
                Destroy(GetComponent<CapsuleCollider>());
            }
            character = gameObject.GetComponent<CharacterController>();
            if (character == null)
            {
                character = gameObject.AddComponent<CharacterController>();
                character.height = capheight;
                character.radius = capradius;
                character.slopeLimit = 60.0f;
                character.stepOffset = .3f;
                Vector3 offset = character.center;
                offset.y = 0.95f;
                character.center = capcenter;
            }
        }
        gravity = Physics.gravity;
        t = transform; //Cache transform component
        movementController = GetComponentInChildren<PlayerMovement>();
        movementPivot = GameObject.Find("PlayerMovementPivot").transform;
        if (movementController != null)
        {
            isReady = true;
        }
        else
        {
//            Debug.Log("No movement controller");
        }
    }

    public void SetFlyCamEnabled(bool enabled)
    {
//        Debug.Log("Flycam set enabled = " + enabled + ", is chatting = " + movementController.IsChatting());
        _flyCamEnabled = enabled;
        this.enabled = !enabled;
        movementController.enabled = !enabled;
    }

    void Update()
    {
        if (isReady)
        {
            if (!movementController.IsChatting() && !_flyCamEnabled)
            {
                isRotating = Input.GetAxis("Horizontal") != 0 || Input.GetMouseButton(1) || rotateRight || rotateLeft;
                isMoving = Input.GetAxis("Vertical") != 0 || (Input.GetMouseButton(0) && Input.GetMouseButton(1));
                if (usePhysicalCharacter)
                {
                    MovePhysicalCharacter();
                }
                else
                {
                    MoveRegularCharacter();
                }
            }
        }
    }

    private void MovePhysicalCharacter()
    {
        if (isRotating || isMoving)
        {
            //Since we're not applying rotation as torque, we don't need to do it in FixedUpdate
            float horizontal = Input.GetAxis("Horizontal");
            if (Input.GetMouseButton(1))
            {
                horizontal = Input.GetAxisRaw("Mouse X");
            }
			if(rotateRight && rotateLeft)
			{
				horizontal=0;
			}
			else if(rotateRight)
			{
				horizontal=1;
			}
			else if(rotateLeft)
			{
				horizontal=-1;
			}
            rb.MoveRotation(rb.rotation * Quaternion.Euler(Vector3.up * horizontal * horizontalRotationSpeed * Time.deltaTime));
            movementController.UpdateTransform(t);
        }
		else if(isFlying)
		{
			movementController.UpdateTransform(t);
		}
		
		//this allows the user to hold down the jump button and to continue jumping
		if ((Input.GetButton("Jump"))&&(isFlying==false))
            {
                if (onGround)
                {
                    jump = true; //The player will jump when next FixedUpdate is called
                }
            }
    }

    private void MoveRegularCharacter()
    {
        if (isRotating || isMoving || !character.isGrounded)
        {
            movement = Vector3.zero; //Zero-out movement vector
            movementPivot.Rotate(0, horizontalRotationSpeed * Input.GetAxis("Horizontal") * Time.deltaTime, 0); //Update movement pivot's rotation
            //Face towards pivot
            Quaternion facePivot = t.rotation;
            facePivot.SetLookRotation(movementPivot.forward);
            t.rotation = Quaternion.Lerp(t.rotation, facePivot, forwardFacingSpeed);

            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                //If both LMB and RMB are held down, the character should move forward
                movement += movementPivot.forward;

                if (Input.GetAxis("Vertical") < 0)
                {
                    //If the S key is also held down, the character should stay in place
                    movement += Input.GetAxis("Vertical") * movementPivot.forward;
                }
            }
            else
            {
                //Regular movement using WASD keys
                movement += Input.GetAxis("Vertical") * movementPivot.forward;
            }
            if (movement.sqrMagnitude > 1)
            {
                //Normalize the movement vector so the player doesn't go faster when going diagonally
                movement.Normalize();
            }
            character.Move(movement * groundSpeed * Time.deltaTime); //Move the character
            movementController.UpdateTransform(t);
        }
    }
    void FixedUpdate()
    {
        if (isReady && !_flyCamEnabled && !movementController.IsChatting())
        {
            if (usePhysicalCharacter)
            {
                float currentForwardSpeed = movementController.GetSpeed();
				if (walkBackward || walkForward) 
					currentForwardSpeed = 6.0f;
                float forward = Input.GetAxisRaw("Vertical");
                float horizontal = Input.GetAxisRaw("Horizontal");
				
				if(Input.GetMouseButton(0) && Input.GetMouseButton(1))
				{ //If both LMB and RMB are held down, the character should move forward
					forward = 1;
					horizontal = Input.GetAxisRaw("Mouse X");
				}
				if(walkBackward)
				{
					forward = 1;
				}
				if (walkForward)
                { 
					forward = -1;
                }
                //Calculate and normalize movement vector
                Vector3 movement = new Vector3(horizontal, forward, 0);
                if (movement.sqrMagnitude > 1)
                    movement.Normalize();


                //Add force in the direction of movement
                rb.AddRelativeForce(movement.y * Vector3.forward * ((onGround) ? currentForwardSpeed : airSpeed));
                rb.AddRelativeForce(movement.x * Vector3.right * ((onGround) ? currentForwardSpeed : airSpeed));
                if ((jump)&&(isFlying==false))
                {
                    rb.AddForce(t.up * jumpSpeed); //Add upwards force to make player jump
                    onGround = false; //Player is no longer grounded
                    jump = false;
                    rb.drag = inAirDrag; //Change rigidbody drag to allow for better movement in air
                }
				
				if(changeUp)
				{
					enableFly = true;
				}
				else if(changeDown)
				{
					enableFly = true;
				}
				
                if (enableFly)
                {
                    if((Input.GetKey("e"))&&(Input.GetKey("c")))
						verticalmodifier = 0;
					else if((Input.GetKey("e"))&&changeDown)
						verticalmodifier = 0;
					else if((Input.GetKey("c"))&&changeUp)
						verticalmodifier = 0;
					else if ((Input.GetKey("e"))||changeUp) 
						verticalmodifier = 1;
                    else if ((Input.GetKey("c"))||changeDown) 
						verticalmodifier = -1;
					else verticalmodifier = 0;
                }
				else verticalmodifier=0;
				
                if (verticalmodifier != 0)
                {
                    rb.useGravity = false; // start flying
                    isFlying = true;
                }
                Vector3 upmovement = new Vector3(0, 0, verticalmodifier);
                rb.AddRelativeForce(upmovement.z * Vector3.up * airSpeed);

            }
            else
            {
                if (!character.isGrounded)
                {
                    hasLanded = false;
                    
                    int verticalmodifier = 0;
                    if (enableFly)
                    {
						Debug.Log("Checks flying");
                        if ((Input.GetKey("e"))||changeUp) verticalmodifier = 1;
                        if ((Input.GetKey("c"))||changeDown) verticalmodifier = -1;
                    }
                    if (verticalmodifier != 0)
                    {
                        // flying
                        isFlying = true;                        
                        character.Move(gravity * -1 * Time.deltaTime);
                    }
                    else if (/*verticalmodifier < 0 || */ isFlying)
                    {
                        isFlying = false;
                        // Apply downwards gravity
                        character.Move(gravity * Time.deltaTime);
                    }
                }
                else if (!hasLanded)
                {
                    movementController.UpdateTransform(t);
                    hasLanded = true;
                }
            }
        }
    }
    //Check for collisions to determine when the player is on the ground
    void OnCollisionEnter(Collision collision)
    {
        float collisionAngle = Vector3.Angle(collision.contacts[0].normal, Vector3.up); //Calculate angle between the up vector and the collision contact's normal
        if (collisionAngle < 40.0)
        { //If the angle difference is small enough, accept collision as hitting the ground
            if (!onGround) // just landed
                movementController.UpdateTransform(t);
            onGround = true; //Player is grounded
            rb.useGravity = true;
            isFlying = false;
            rb.drag = onGroundDrag; //Restore original drag value
        }
        if (collisionAngle > 70.0)
        { //hitting a wall or a step, try a small increase in elevation
            //rb.AddForce(t.up * jumpSpeed);
        }
    }
	
	void OnCollisionStay(Collision collision)
	{
		float collisionAngle = Vector3.Angle(collision.contacts[0].normal, Vector3.up);
		if(collisionAngle < 40.0)
		{
			onGround = true;
			rb.useGravity = true;
			isFlying = false;
			rb.drag = onGroundDrag;
		}
	}

    void SetMovementPivot(Transform newPivot)
    {
        if (newPivot)
        {
            movementPivot = newPivot;
        }
    }
}