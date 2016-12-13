using UnityEngine;
using System.Collections;
//testing 1 2 3
/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;
	public bool firstPerson = false;
	float rotationX = 0F;
	float rotationY = 0F;
	GameObject cam;
	Quaternion camRotation;
	Vector3 camPosition;
	Quaternion originalRotation;
	bool resetRotation=false;
	public GUIContent cursorIcon;
	public bool mouseControlOn = true;
	public bool playerControlOn = true; //
	public bool physicalCam = true;
	private Vector3 prevWorldPosition;
	private Quaternion rotationOnExitFirstPersonMode = new Quaternion(0,0,0,1);
	public bool GUIInputSelected=false;
	public static bool canZoom = true;
	void LateUpdate ()
	{
		if(!GUIInputSelected)
		{
			if(GUIUtility.keyboardControl==0)
			{
				HandleZooming();
				if(firstPerson) //first person cam controls - note the player actually moves here, but it doesn't matter because they can't see it and it isn't synced on the network
				{
					firstPersonControls();
				}
				if(Input.GetMouseButton(0)&&!firstPerson && mouseControlOn && playerControlOn)
				{
					rotationY=0;
					cam.transform.localPosition=camPosition;
					cam.transform.localRotation=camRotation;
					rotationX += Input.GetAxis("Mouse X") * sensitivityX;
					rotationX = ClampAngle (rotationX, minimumX, maximumX);
					Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
					transform.localRotation = originalRotation * xQuaternion;
				}
				if(!firstPerson && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))/* && playerControlOn*/) //doing the same thing as with just mouse 0 down - just assigning 3 as an arbitrary playtested value
				{
					Quaternion oldPlayerRotation = transform.localRotation;
					resetRotation=true;
					cam.transform.localPosition=camPosition;
					cam.transform.localRotation=camRotation;
					if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
					{
						rotationX -=3;
					}
					else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
					{
						rotationX +=3;
					}
					rotationX = ClampAngle (rotationX, minimumX, maximumX);
					Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
					transform.localRotation = originalRotation * xQuaternion;
					if(!playerControlOn)
					{
						Vector3 oldPos = cam.transform.position;
						Quaternion oldRot = cam.transform.rotation;
						transform.localRotation = oldPlayerRotation;
						cam.transform.position = oldPos;
						cam.transform.rotation = oldRot;
					}
				}
				
				if((Input.GetMouseButton(0)) && (Input.GetMouseButton(1)) && playerControlOn) //sending a message to the fpsinputcontroller script too actually move the player
				{
					SendMessage("moveForward");
				}
				else if(Input.GetMouseButton(1) && !firstPerson)
				{	
					Quaternion oldPlayerRotation = transform.localRotation;
					cam.transform.localPosition=camPosition;
					cam.transform.localRotation=camRotation;
					if (axes == RotationAxes.MouseXAndY)
					{		
						// Read the mouse input axis
						rotationX += Input.GetAxis("Mouse X") * sensitivityX;
						rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
						rotationX = ClampAngle (rotationX, minimumX, maximumX);
						rotationY = ClampAngle (rotationY, minimumY, maximumY);
						
						Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
						Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
						
						transform.localRotation = originalRotation * xQuaternion * yQuaternion;
					}
		/*			else if (axes == RotationAxes.MouseX) //look only left right
					{
						print("two");
						rotationX += Input.GetAxis("Mouse X") * sensitivityX;
						rotationX = ClampAngle (rotationX, minimumX, maximumX);
			
						Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
						transform.localRotation = originalRotation * xQuaternion;
					}
					else // look only up down
					{
						print("three");
						rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
						rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
						Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
						transform.localRotation = originalRotation * yQuaternion;
					}*/
					Vector3 oldPos = cam.transform.position;
					Quaternion oldRot = cam.transform.rotation;
					transform.localRotation = oldPlayerRotation;
					cam.transform.position = oldPos;
					cam.transform.rotation = oldRot;
				}
				else if(Input.GetMouseButton(0)&&!firstPerson && mouseControlOn && !playerControlOn)
				{
	/*				Debug.Log("OHAIYO GONZAIMAS!");
					Quaternion oldPlayerRotation = transform.localRotation;
					cam.transform.localPosition=camPosition;
					cam.transform.localRotation=camRotation;
					if (axes == RotationAxes.MouseXAndY)
					{		
						// Read the mouse input axis
						rotationX += Input.GetAxis("Mouse X") * sensitivityX;
						rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
						rotationX = ClampAngle (rotationX, minimumX, maximumX);
						rotationY = ClampAngle (rotationY, minimumY, maximumY);
						
						Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
						Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
						
						transform.localRotation = originalRotation * xQuaternion * yQuaternion;
					}
					//this part here makes it so that the person doesn't rotate along with the camera
					Vector3 oldPos = cam.transform.position;
					Quaternion oldRot = cam.transform.rotation;
					transform.localRotation = oldPlayerRotation;
					cam.transform.position = oldPos;
					cam.transform.rotation = oldRot;*/
				}
				mouseControlOn=true;
			}
			if(prevWorldPosition!=cam.transform.position) //physical cam code - should be self explanatory
			{
				if(physicalCam==true)
				{
					RaycastHit targetPoint;
					int layerMask= (1 << 8); //8 should be the Player layer and 2 is the ignoreRaycast layer
					layerMask=~layerMask; //we want to change 8 from the only layer we see to the only layer we don't see
					if(Physics.Linecast(transform.position,cam.transform.position, out targetPoint, layerMask)) 
					{
						Debug.DrawLine(transform.position,cam.transform.position, Color.red, 10);
						cam.transform.position= targetPoint.point;
					}
				}
			}
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
		originalRotation = transform.localRotation;
		cam=GameObject.Find("PlayerCam");
		camRotation=cam.transform.localRotation;
		camPosition=cam.transform.localPosition;
	}
	void OnGUI()
	{
		if(firstPerson)
		{
			GUI.Box(new Rect(Screen.width/2,Screen.height/2,16,16), cursorIcon);
		}
	}
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
	public void SetCurrentTransformAsDefault()
	{
		Debug.Log("altering default transform");
		rotationX=0;
		rotationY=0;
		cam.transform.localPosition=camPosition; //we have to zoom it in so that we do not hit the ceiling and the camera gives up
		cam.transform.localRotation=camRotation;
		originalRotation=transform.localRotation;
	}
	public void SnapCameraToDefault()
	{
		cam.transform.localPosition=camPosition; //we have to zoom it in so that we do not hit the ceiling and the camera gives up
		cam.transform.localRotation=camRotation;
		rotationX=0;
		rotationY=0;
	}
	public void GetSitRotation(Quaternion rotation)
	{
		rotationOnExitFirstPersonMode = rotation;
	}
	private void HandleZooming()
	{
		if(canZoom)
		{
			float scroll = -Input.GetAxis("Mouse ScrollWheel");
			if(scroll!=0)
			{
				Vector3 holderPosition = cam.transform.localPosition;
				cam.transform.localPosition=camPosition;
				float camPositionWeight = cam.transform.localPosition.x+cam.transform.localPosition.y+cam.transform.localPosition.z;
				Vector3 newCamPosition = new Vector3(cam.transform.localPosition.x-((cam.transform.localPosition.x/camPositionWeight)*scroll),cam.transform.localPosition.y-((cam.transform.localPosition.y/camPositionWeight)*scroll),cam.transform.localPosition.z-((cam.transform.localPosition.z/camPositionWeight)*scroll));
				float distance = Mathf.Pow((newCamPosition.x*newCamPosition.x)+(newCamPosition.y*newCamPosition.y)+(newCamPosition.z*newCamPosition.z),.5f);
				float minDistance = .6f;
				float maxDistance = 6f;
				if(distance>minDistance && distance<maxDistance) //valid zoom position, simple adjust the position of the cam
				{
					camPosition=newCamPosition;
					cam.transform.localPosition=holderPosition;
					cam.transform.localPosition = new Vector3(cam.transform.localPosition.x-((cam.transform.localPosition.x/camPositionWeight)*scroll),cam.transform.localPosition.y-((cam.transform.localPosition.y/camPositionWeight)*scroll),cam.transform.localPosition.z-((cam.transform.localPosition.z/camPositionWeight)*scroll));
				}
				/*else if(distance<minDistance &&!firstPerson) //enter firstperson mode
				{
					firstPerson=true;
					Screen.lockCursor=true;
					cam.transform.localPosition=Vector3.zero;
					camPosition=Vector3.zero;
				}
				else if(firstPerson && scroll>0) //leaving firstperson mode
				{
					firstPerson=false;
					Screen.lockCursor=false;
					if(playerControlOn)
					{
						transform.rotation = new Quaternion(0,transform.rotation.y,0,transform.rotation.w);
					}
					else
					{
						transform.rotation = rotationOnExitFirstPersonMode;
					}
					camPosition = Vector3.ClampMagnitude(new Vector3(-0.02498839f,1.223719f,-3.651375f),minDistance);
					cam.transform.localPosition= Vector3.ClampMagnitude(new Vector3(-0.02498839f,1.223719f,-3.651375f),minDistance);
					cam.transform.localRotation= new Quaternion(0f,0f,0f,1f);
					cam.transform.Rotate(new Vector3(17.87721f,0f,0f));
				}*/
			}
		}
	}
	private void firstPersonControls()
	{
		resetRotation=true;
		cam.transform.localPosition=camPosition;
		cam.transform.localRotation=camRotation;
		rotationX += Input.GetAxis("Mouse X") * sensitivityX;
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationX = ClampAngle (rotationX, minimumX, maximumX);
		rotationY = ClampAngle (rotationY, minimumY, maximumY);
		Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
		transform.localRotation = originalRotation * xQuaternion * yQuaternion;
	}
}