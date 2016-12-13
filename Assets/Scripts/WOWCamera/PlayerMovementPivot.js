// PlayerMovementPivot.js revision 1.0.1103.01
#pragma strict

var target : Transform; //Target to follow
var offset : Vector3; //Offset from target
private var t : Transform; //Reference to the transform component of the gameObject

function Awake() {
	t = transform; //Cache reference to the transform component
}

function LateUpdate () {
	t.position = target.position + offset; //Place at offset from target's position
	var euler = t.rotation.eulerAngles; //Get current rotation
	euler.x = 0; //Lock x rotation
	euler.z = 0; //Lock z rotation
	t.rotation = Quaternion.Euler(euler); //Set modified rotation
}

function SetTarget(newTarget : Transform)
{
    if (newTarget)
    {
        target = newTarget;
    }
}