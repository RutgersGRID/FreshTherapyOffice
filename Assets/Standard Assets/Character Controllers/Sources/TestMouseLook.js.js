#pragma strict
private var motor : CharacterMotor;
private var localPlayer : GameObject;
private var charController : CharacterController;
private var forwardMessage;
private var netController;
private var mouseLook;
private var cam;
public static var controlsEnabled=true;
private var persistentForwardMessage=false;
private var characterToAnimate : GameObject; //the gameobject that has the player's animations attached to it.

// Use this for initialization
function Awake () {
	mouseLook = GetComponent("MouseLook");

}

function Start () {
	
}

function Update () {
	
}
