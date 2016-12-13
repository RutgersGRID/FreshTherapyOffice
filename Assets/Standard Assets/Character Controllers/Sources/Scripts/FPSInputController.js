private var motor : CharacterMotor;
private var localPlayer : GameObject;
private var object: Transform;
private var charController : CharacterController;
private var forwardMessage;
private var netController: GameObject;
private var mouseLook;
private var cam;
public static var controlsEnabled=true;
private var persistentForwardMessage=false;
private var characterToAnimate : GameObject; //the gameobject that has the player's animations attached to it.
// Use this for initialization
function Awake () {
	mouseLook = GetComponent("MouseLook");
	netController = GameObject.Find("NetworkController");
	motor = GetComponent(CharacterMotor);
	localPlayer = GameObject.Find("localPlayer");
	charController = GetComponent(CharacterController);
	cam = GameObject.Find("PlayerCam");
}

function Start()
{
	this.transform.Translate(Vector3(0,0,0));
}
// Update is called once per frame
function Update () {

	if(controlsEnabled)
	{
		// Get the input vector from the controllers or from another script
		var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		if(forwardMessage)
		{
			directionVector = new Vector3(directionVector.x,0,1);
			forwardMessage=false;
		}
		if(persistentForwardMessage)
		{
			directionVector = new Vector3(directionVector.x,0,1);
		}
		//TODO: Assets/Standard Assets/Character Controllers/Sources/Scripts/FPSInputController.js(42,30): BCE0019: 'firstPerson' is not a member of 'Object'. 

		if(/*mouseLook.firstPerson &&*/ Input.GetKey(KeyCode.A))
		{
			directionVector = new Vector3(-1,0,directionVector.z);
		}
		//TODO: Assets/Standard Assets/Character Controllers/Sources/Scripts/FPSInputController.js(46,30): BCE0019: 'firstPerson' is not a member of 'Object'. 

		if(/*mouseLook.firstPerson && */ Input.GetKey(KeyCode.D))
		{
			directionVector = new Vector3(1,0,directionVector.z);
		}
		//TODO: Assets/Standard Assets/Character Controllers/Sources/Scripts/FPSInputController.js(46,30): BCE0019: 'firstPerson' is not a member of 'Object'. 
		if(/*mouseLook.firstPerson &&*/ motor.movement.gravity!=30) //gravity is 30 when we are on the ground
		{
			var camRotation = transform.localEulerAngles;
			var angle = camRotation.x;
			if(angle>=300)
			{
				angle=angle-360;
			}
			motor.movement.velocity=Vector3.zero;
			var moveVector = Vector3(directionVector.x,directionVector.z*angle/360,directionVector.z-(directionVector.z*angle/360));
			moveVector = transform.rotation * moveVector;
			moveVector=moveVector.normalized;
			moveVector*=5f;
			if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift))
			{
				moveVector*=2f;
			}
			motor.movement.velocity=moveVector;
			directionVector = Vector3.zero;  //disable movement input from other sources
		}
		/*if(Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.C))
		{
		
		}
		else if(Input.GetKey(KeyCode.E))
		{
			motor.movement.gravity=0;
			motor.movement.velocity=Vector3(motor.movement.velocity.x,0,motor.movement.velocity.z);
			charController.Move(Vector3.up* Time.deltaTime*10);
		}
		else if(Input.GetKey(KeyCode.C))
		{
			motor.movement.gravity=0;
			motor.movement.velocity=Vector3(motor.movement.velocity.x,0,motor.movement.velocity.z);
			charController.Move(Vector3.down * Time.deltaTime*10);
		}*/
		if((charController.collisionFlags & CollisionFlags.Below)!=0) //was jumping and just hit ground
		{
			motor.movement.gravity=30;
			OnGround();
			if(Input.GetKey(KeyCode.Space))
			{
				motor.jumping.lastButtonDownTime=Time.time;
			}
		}
		if((Input.GetKeyDown(KeyCode.LeftShift))||(Input.GetKeyDown(KeyCode.RightShift))) //toggling sprinting
		{
			motor.movement.maxForwardSpeed = 10;
			motor.movement.maxSidewaysSpeed = 10;
			motor.movement.maxBackwardsSpeed = 10;
		}
		if((Input.GetKeyUp(KeyCode.LeftShift))||(Input.GetKeyUp(KeyCode.RightShift)))
		{
			motor.movement.maxForwardSpeed = 5;
			motor.movement.maxSidewaysSpeed = 5;
			motor.movement.maxBackwardsSpeed = 5;
		}	
		if (directionVector != Vector3.zero) {
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			var directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
	
		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = transform.rotation * directionVector;
		motor.inputJump = Input.GetButton("Jump");
		sendMovementSynchronizationSignal(transform);
	//	localPlayer.transform.Rotate(Vector3(0,turn,0)*Time.deltaTime);
	}
}
/*function OnGUI()
{
	if(motor.movement.gravity!=30)
	{
		if(GUI.Button(Rect(Screen.width/2-50,Screen.height-50,100,25), "Stop Flying"))
		{
			motor.movement.gravity=30;
		}
	}
}*/
function OnGround()
{
	object = gameObject.Find("localPlayer").transform.GetChild(0); //finding it this way ensures that we do not select a remote player and that it works even though each avatar's gameobject is named differently
	//TODO: Assets/Standard Assets/Character Controllers/Sources/Scripts/FPSInputController.js(144,47): BCE0019: 'IsOnGround' is not a member of 'UnityEngine.Component'. 

	//object.GetComponent("PlayerMovement").IsOnGround();
}
//these functions are all here to allow other scripts to send input to this script
function moveForward()
{
	forwardMessage=true;
}
function moveForwardForever()
{
	persistentForwardMessage=!persistentForwardMessage;
}
function sendMovementSynchronizationSignal(data : Transform)
{
    netController.GetComponent("NetworkController").SendMessage("SendTransform", data);
}
function toggleControls()
{
	controlsEnabled=!controlsEnabled;
}
// Require a character controller to be attached to the same game object
@script RequireComponent (CharacterMotor)
@script AddComponentMenu ("Character/FPS Input Controller")
