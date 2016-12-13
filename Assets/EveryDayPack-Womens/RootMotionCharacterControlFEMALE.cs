		
using UnityEngine;
using System.Collections;


[AddComponentMenu("Mixamo/Demo/Root Motion Character")]
public class RootMotionCharacterControlFEMALE: MonoBehaviour
{
	public float turningSpeed = 90f;
	public RootMotionComputer computer;
	public CharacterController character;
	
	void Start()
	{
		// validate component references
		if (computer == null) computer = GetComponent(typeof(RootMotionComputer)) as RootMotionComputer;
		if (character == null) character = GetComponent(typeof(CharacterController)) as CharacterController;
		
		// tell the computer to just output values but not apply motion
		computer.applyMotion = false;
		// tell the computer that this script will manage its execution
		computer.isManagedExternally = true;
		// since we are using a character controller, we only want the z translation output
		computer.computationMode = RootMotionComputationMode.ZTranslation;
		// initialize the computer
		computer.Initialize();
		
		// set up properties for the animations
		GetComponent<Animation>()["talking01"].layer = 0; GetComponent<Animation>()["talking01"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["sexywalk"].layer = 1; GetComponent<Animation>()["sexywalk"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["goofyrun"].layer = 1; GetComponent<Animation>()["goofyrun"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["femtoughwalk"].layer = 3; GetComponent<Animation>()["femtoughwalk"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["no"].layer = 3; GetComponent<Animation>()["no"].wrapMode = WrapMode.Once;
		GetComponent<Animation>()["sassywalk"].layer = 3; GetComponent<Animation>()["sassywalk"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["standinggreeting"].layer = 3; GetComponent<Animation>()["standinggreeting"].wrapMode = WrapMode.Once;
		GetComponent<Animation>()["talking02"].layer = 3; GetComponent<Animation>()["talking02"].wrapMode = WrapMode.Once;
		GetComponent<Animation>()["tellsecret"].layer = 3; GetComponent<Animation>()["tellsecret"].wrapMode = WrapMode.Once;
		
		GetComponent<Animation>().Play("talking01");
		
	}
	
	void Update()
	{
		float targetMovementWeight = 0f;
		float throttle = 0f;
		
		// turning keys
		if (Input.GetKey(KeyCode.A)) transform.Rotate(Vector3.down, turningSpeed*Time.deltaTime);
		if (Input.GetKey(KeyCode.D)) transform.Rotate(Vector3.up, turningSpeed*Time.deltaTime);
		
		// forward movement keys
		// ensure that the locomotion animations always blend from idle to moving at the beginning of their cycles
		if (Input.GetKeyDown(KeyCode.W) && 
			(GetComponent<Animation>()["sexywalk"].weight == 0f || GetComponent<Animation>()["run"].weight == 0f))
		{
			GetComponent<Animation>()["sexywalk"].normalizedTime = 0f;
			GetComponent<Animation>()["goofyrun"].normalizedTime = 0f;
		}
		if (Input.GetKey(KeyCode.W))
		{
			targetMovementWeight = 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) throttle = 1f;
				
		// blend in the movement

		GetComponent<Animation>().Blend("goofyrun", targetMovementWeight*throttle, 0.5f);
		GetComponent<Animation>().Blend("sexywalk", targetMovementWeight*(1f-throttle), 0.5f);
		// synchronize timing of the footsteps
		GetComponent<Animation>().SyncLayer(1);
		
		// all the other animations, such as punch, kick, attach, reaction, etc. go here
		if (Input.GetKeyDown(KeyCode.Alpha1)) GetComponent<Animation>().CrossFade("femtoughwalk", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha2)) GetComponent<Animation>().CrossFade("no", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha3)) GetComponent<Animation>().CrossFade("sassywalk", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha4)) GetComponent<Animation>().CrossFade("standinggreeting", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha5)) GetComponent<Animation>().CrossFade("talking02", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha6)) GetComponent<Animation>().CrossFade("tellsecret", 0.2f);

	}
	
	void LateUpdate()
	{
		computer.ComputeRootMotion();
		
		// move the character using the computer's output
		character.SimpleMove(transform.TransformDirection(computer.deltaPosition)/Time.deltaTime);
	}
}