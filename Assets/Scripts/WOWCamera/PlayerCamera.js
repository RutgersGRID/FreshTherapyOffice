// PlayerCamera.js revision 1.4.1.1108.22

var r : float; //Radius of the sphere around the target on which the camera moves
var theta : float; //Theta angle used to calculate the y-z position of the camera
var phi : float; //Phi angle used to calculate the x-z position of the camear
private var thetaMax : float = Mathf.PI; //Max value that the theta angle can have
private var thetaEpsilon : float = 0.1; //A small value subtracted from clamping calculations to allow looking at the target from almost the top without spinning over
private var thetaRange : float = Mathf.PI; //Total range of the theta angle, 0 to 180
private var phiMax : float = Mathf.PI * 2; //Max vaue that the phi angle can have
private var phiRange : float = Mathf.PI * 2; //Total range of the phi angle, 0 to 360

var lerpDamp : float; //Damping of the camera movement
private var lockedToBack : boolean = true; //Whether the camera is locked to the back of the character
private var cameraEnabled : boolean = true;

var mouseHorizontalSpeed : float; //Horizontal rotation speed of the camera
var mouseVerticalSpeed : float; //Vertical rotation speed of the camera
var mousePivotRotationSpeed : float; //Rotation speed of the player movement pivot when using Right Mouse Button

var invertVertical : boolean; //Inverts the vertical rotation of the camera
var invertHorizontal : boolean; //Inverts the horizontal rotation of the camera

var camCollisionRadius : float; //Collision radius of the camera, used for preventing the camera clipping against surfaces that are very close
var camNoLerpAngle : float; //Angle below which the camera should lock to the back of the character instead of lerping

private var freeLook : boolean = false; //Whether the camera can freely rotate around the character even when there's movement
private var startingLocalPosition : Vector3; //Camera's starting position relative to the target

private var desiredWorldPosition : Vector3; //World position we want to be if we were to be exactly behind the target
private var relativePosition : Vector3; //Position of the camera relative to the target, in world space

private var cam : Camera; //Reference to the Camera component of the gameObject
private var guiLayer : GUILayer; //Reference to the GUILayer component of the gameObject
private var t : Transform; //Reference to the Transform of the gameObject
var target : Transform; //The target the camera is following and looking at
var defaultTarget : Transform;
var flyCamEnabled : boolean = false;
var storedCamPos : Vector3;
var storedCamRot : Quaternion;
var movementPivot : Transform; //The movement pivot used by the player character
private var cursor : Cursor; //Reference to the Cursor in the scene
private var isChatting =  false;
var distance = 1.0;  //ZPWH 1.0
var minDistance = 0.7; 
var maxDistance = 5.0;

var guiParent : GameObject;
private var isReady = false;
private var initialDistance = 1.0;

function SetupCamera() {
    //Grab references
	cam = GetComponent.<Camera>();
	guiLayer = gameObject.GetComponent(GUILayer);
	t = transform;
    initialDistance = t.localPosition.z * -1;
	cursor = FindObjectOfType(Cursor);
    relativePosition = t.position - target.position;
	startingLocalPosition = target.InverseTransformPoint(t.position);
	//Calculate theta and phi from current relative position
	CalculateSphericalParametersFromPosition(relativePosition);
    isReady = true;
}
function Update() 
{
	//ZPWH
	if((Input.GetKeyDown("a"))||(Input.GetKeyDown("d")))
	{
		SnapToBack();
	}
	if((Input.GetKey("a"))||(Input.GetKey("d")))
	{
		SnapToBack();
	}
    if (isReady)
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (distance > minDistance)
            {
                distance = distance - 0.1;;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (distance < maxDistance)
            {
                distance = distance + 0.1;
            }
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("leftalt + click");
            var mouseRay : Ray = GetComponent.<Camera>().ScreenPointToRay(Input.mousePosition);
            var hit : RaycastHit;
            if (Physics.Raycast(mouseRay, hit, 50))
            {
                Debug.Log("hit " + hit.transform.name + ", tag: " + hit.transform.tag);
                if (hit.transform.name.ToLower() != "terrain" && hit.transform.tag != "Player")
                {
                    if (target == defaultTarget)
                    {
                        // just started flycam
                        storedCamPos = t.position;
                        storedCamRot = t.rotation;
                    }
                    target = hit.transform;
                    SetupCamera();
                    SendMessageUpwards("SetFlyCamEnabled", true);
                    flyCamEnabled = true;
                }
            }
        }
        if (Input.GetKey(KeyCode.Escape))
        {                
            if (flyCamEnabled)
            {
                Debug.Log("Reset to default position");
                target = defaultTarget;
                t.position = storedCamPos;
                t.rotation = storedCamRot;
                SetupCamera();            
                //SnapToBack();
                SendMessageUpwards("SetFlyCamEnabled", false);
                flyCamEnabled = false;
            }
            else if (CameraEnabled())
            {
                SnapToBack();
            }
        }        
    }
}


function LateUpdate () 
{
    if (isReady)
    {	
	    DetermineCursorGUILock(); //Determine whether the cursor should show or be hidden and locked

	    //Determine if freeLook should be enable and whether the camera should be locked to the back of the target
	    if(Input.GetMouseButtonDown(0) && CameraEnabled()) 
        {
		    freeLook = true;
		    lockedToBack = false;
	    } 
        else if(Input.GetMouseButtonUp(0)) 
        {
		    freeLook = false;
	    }
	
	    if(Input.GetMouseButton(1) && CameraEnabled()) 
        { //Handle Right Mouse Button type movement of the camera
		    lockedToBack = false;
		    //Only update theta, for vertical rotation of the camera
		    theta += ((invertVertical) ? 1 : -1) * Input.GetAxis("Mouse Y") * mouseVerticalSpeed;
		    ClampThetaPhiRanges();
		    //Rotate the movement pivot of the player directly using the horizontal movement of the mouse, so we're indirectly controlling the direction of the player
		    movementPivot.Rotate(0,Input.GetAxis("Mouse X") * mousePivotRotationSpeed,0);
		    SnapToBackOnlyHorizontal(); //Keep the horizontal rotation of the camera locked to the back of the character
	    } 
        else 
        { //Right Mouse Button isn't held down
		    if(lockedToBack) 
            { //If we're locked to the back of the target, we should stay locked
			    SnapToBack();
		    } 
            else if(PlayerInputOn() && !freeLook && !flyCamEnabled && !isChatting) 
            { //Otherwise, we should lerp to the back of the character if freeLook is false
			    LerpToBack();
		    } 
            else 
            {
			    if(Input.GetMouseButton(0) && CameraEnabled()) { //Free look if Left Mouse Button is held down
				    //Update and clamp theta and phi
				    theta += ((invertVertical) ? 1 : -1) * Input.GetAxis("Mouse Y") * mouseVerticalSpeed;
				    phi += ((invertHorizontal) ? 1 : -1) * Input.GetAxis("Mouse X") * mouseHorizontalSpeed;
				    ClampThetaPhiRanges();
			    }
			    t.position = target.position + CalculatePositionFromSphericalParameters(); //Set new position calculated from r, theta, and phi
		    }
	    }
	    t.LookAt(target); //Always look at target

	    CheckLineOfSight(); //Make sure nothing is in the way of our line of sight to the target
    }
}

function PlayerInputOn()
{
    return (!Mathf.Approximately(0.0, Input.GetAxis("Horizontal")) || !Mathf.Approximately(0.0, Input.GetAxis("Vertical")));
}

function CameraOnScreen()
{
    if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > 0 && Input.mousePosition.y < Screen.height && Input.mousePosition.y > 0)
        return true;
    else
        return false;
}

function CameraEnabled()
{
    if (CameraOnScreen() && cameraEnabled)
    {
        return true;
    }
    else
    {
        return false;
    }
}

function SetCameraEnabled(enable)
{
    cameraEnabled = enable;
}

function SetChatting(chatMode)
{
    isChatting = chatMode;
}
//Preserve line of sight to the target by casting a ray from the target towards the camera
//and seeing if it hits any colliders. If it does, pull the camera closer towards the target
//until that collider is no longer between the camera and the target.
function CheckLineOfSight() 
{
	var ray : Ray = Ray(target.position , (t.position - target.position).normalized);
	var hit : RaycastHit = RaycastHit();
	var layerMask1 : int = ~(1 << LayerMask.NameToLayer("PlayerSkin"));
	var layerMask2 : int = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
	var layerMask = layerMask1 & layerMask2;
	if(Physics.Raycast(ray, hit, Vector3.Distance(t.position, target.position) + camCollisionRadius,layerMask)) 
    {
		var dir = (target.position - t.position).normalized; //Calculate direction in which we'll move the camera
		var newPos = hit.point + dir * camCollisionRadius; //New position is camCollisionRadius away from the ray hit point

		t.position = newPos; //Set new position        
	}
}

function DetermineCursorGUILock() 
{
//	//Determine if the current mouse position is over any GUI elements
//	var hitGUIElement = guiLayer.HitTest(Input.mousePosition);
//	if(hitGUIElement != null) { //There is a GUI Element in the way
//		cursor.SetEnabled(true); //Keep cursor enabled
//		guiParent.SetActiveRecursively(true); //Keep GUI elements enabled
//		return;
//	}
//	if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) { //If either LMB or RMB are held down, cursor and GUI elements should be hidden
//		cursor.SetEnabled(false); //Disable cursor
//		guiParent.SetActiveRecursively(false); //Disable GUI elements
//		return;
//	}
//	//If we haven't returned until now, that means the mouse didn't hit any GUI Elements and LMB or RMB aren't held down
//	cursor.SetEnabled(true); //So enable cursor
//	guiParent.SetActiveRecursively(true); //And keep GUI elements enabled
}

//Calculate world position from r, theta, and phi
function CalculatePositionFromSphericalParameters() 
{
    r = distance * initialDistance;
	return Vector3(r * Mathf.Sin(theta) * Mathf.Cos(phi), r * Mathf.Cos(theta), r * Mathf.Sin(theta) * Mathf.Sin(phi));
}

//Calculate r, theta, and phi from relative position to the target
function CalculateSphericalParametersFromPosition(pos : Vector3) 
{
	r = pos.magnitude;
	theta = Mathf.Acos(pos.y / r);
	phi = Mathf.Atan2(pos.z, pos.x);
	ClampThetaPhiRanges();
}

//Return theta and phi from relative position to the target
function GetSphericalParametersFromPosition(pos : Vector3) : Vector2 
{
	var params : Vector2;
	params.x = Mathf.Acos(pos.y / r);
	params.y = Mathf.Atan2(pos.z, pos.x);
	//params = ClampThetaPhiRangesVector2(params);
	return params;
}

//Clamp theta and phi to their respective ranges
function ClampThetaPhiRanges() 
{
	var params : Vector2 = Vector2(theta, phi);
	params = ClampThetaPhiRangesVector2(params);
	theta = params.x;
	phi = params.y;
}

//Get clamped values for theta and phi
function ClampThetaPhiRangesVector2(params : Vector2) : Vector2 
{
	if(params.x < thetaEpsilon)
		params.x = thetaEpsilon;
	else if(params.x > thetaMax - thetaEpsilon)
		params.x = thetaMax - thetaEpsilon;
		
	if(params.y < 0)
		params.y += phiMax;
	else if (params.y > phiMax)
		params.y -= phiMax;
		
	return params;
}

//Lerp theta and phi to slowly move camera to the back of the target
function LerpToBack() 
{
	desiredWorldPosition = target.TransformPoint(startingLocalPosition);
	relativePosition = desiredWorldPosition - target.position;
	var params = GetSphericalParametersFromPosition(relativePosition);
	var angle = Vector3.Angle(desiredWorldPosition, t.position);
	lockedToBack = (angle < camNoLerpAngle);
	
	//Find the closer direction to lerp to for both angles
	var endTheta = params.x;
	var thetaDiff = theta - params.x;
	if(Mathf.Abs(thetaDiff) > thetaRange / 2.0)
		endTheta = params.x + Mathf.Sign(thetaDiff) * thetaRange;
	theta = Mathf.Lerp(theta, endTheta, lerpDamp);
	
	var endPhi = params.y;
	var phiDiff = phi - params.y;
	if(Mathf.Abs(phiDiff) > phiRange / 2.0)
		endPhi = params.y + Mathf.Sign(phiDiff) * phiRange;
	phi = Mathf.Lerp(phi, endPhi, lerpDamp);
	
	t.position = target.position + CalculatePositionFromSphericalParameters();
}

//Snap camera to the back of the target
function SnapToBack() 
{
	desiredWorldPosition = target.TransformPoint(startingLocalPosition);
	relativePosition = desiredWorldPosition - target.position;
	var params = GetSphericalParametersFromPosition(relativePosition);

	theta = params.x;
	phi = params.y;
	
	t.position = target.position + CalculatePositionFromSphericalParameters();
}

//Snap camera to the back of the target, but only horizontally, meaning only manipulating phi
function SnapToBackOnlyHorizontal() 
{
	desiredWorldPosition = target.TransformPoint(startingLocalPosition);
	relativePosition = desiredWorldPosition - target.position;
	var params = GetSphericalParametersFromPosition(relativePosition);
	
	phi = params.y;
	
	t.position = target.position + CalculatePositionFromSphericalParameters();
}

//Lerp only phi to slowly move camera to the back of the target
function LerpToBackOnlyHorizontal() 
{
	desiredWorldPosition = target.TransformPoint(startingLocalPosition);
	relativePosition = desiredWorldPosition - target.position;
	var params = GetSphericalParametersFromPosition(relativePosition);
	
	//Find the closer direction to lerp
	var endPhi = params.y;
	var phiDiff = phi - params.y;
	if(Mathf.Abs(phiDiff) > phiRange / 2.0)
		endPhi = params.y + Mathf.Sign(phiDiff) * phiRange;
	phi = Mathf.Lerp(phi, endPhi, lerpDamp);
	//phi = endPhi;
	
	t.position = target.position + CalculatePositionFromSphericalParameters();
}

function SetGuiParent(newGuiParent : GameObject)
{
    if (newGuiParent != null)
    {
        guiParent = newGuiParent;
    }
}

function SetMovementPivot(newPivot : Transform)
{
    if (newPivot != null)
    {
        movementPivot = newPivot;
    }
}
function SetTarget(newTarget : Transform)
{
    // only used on initial setup from spawn controller
    if (newTarget != null)
    {
        target = newTarget;
        defaultTarget = newTarget;
    }
}
