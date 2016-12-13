#pragma strict

// Cursor.js revision 1.4.1106.22

var cursorTexture : Texture2D; //Regular cursor style
var hoverSitCursorTexture : Texture2D; // Sit hover style
var privateChatCursorTexture : Texture2D; // Private chat hover style
var teleportCursorTexture : Texture2D; // Teleport cursor style
var plaqueCursorTexture : Texture2D;
var alwaysUseTextureCursor = false;
var cursorEnabled = true;
var useSitCursor = false;
var usePrivateChatCursor = false;
var useTeleportCursor = false;
var usePlaqueCursor = false;
function Awake() {
	UnityEngine.Cursor.visible = true; //Turn off OS cursor
}


function OnGUI () {    
 if (cursorEnabled)
    {
        GUI.depth = 0;
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || alwaysUseTextureCursor)
        {                    
           	UnityEngine.Cursor.visible = false; //Turn off OS cursor
            GUI.DrawTexture (Rect (Input.mousePosition.x - (cursorTexture.width / 6), Screen.height - (Input.mousePosition.y + (cursorTexture.height / 6)), cursorTexture.width, cursorTexture.height), cursorTexture);       
        }
        else
        {
            UnityEngine.Cursor.visible = true; //Turn on OS cursor
        }  
      
        if (useSitCursor)
        {
            GUI.DrawTexture (Rect (Input.mousePosition.x - (cursorTexture.width / 6), Screen.height - (Input.mousePosition.y + (cursorTexture.height / 6)), cursorTexture.width, cursorTexture.height), hoverSitCursorTexture);
        }
        else if (usePrivateChatCursor)
        {
            GUI.DrawTexture (Rect (Input.mousePosition.x - (cursorTexture.width / 6), Screen.height - (Input.mousePosition.y + (cursorTexture.height / 6)), cursorTexture.width, cursorTexture.height), privateChatCursorTexture);
        }
        else if (useTeleportCursor)
        {
            GUI.DrawTexture (Rect (Input.mousePosition.x - (cursorTexture.width / 6), Screen.height - (Input.mousePosition.y + (cursorTexture.height / 6)), cursorTexture.width, cursorTexture.height), teleportCursorTexture);
        }
        else if (usePlaqueCursor)
        {
        	GUI.DrawTexture (Rect (Input.mousePosition.x - (cursorTexture.width / 6), Screen.height - (Input.mousePosition.y + (cursorTexture.height / 6)), cursorTexture.width, cursorTexture.height), plaqueCursorTexture);
    	}
    }
}

function ShowPlaqueCursor(hovering : boolean)
{
	usePlaqueCursor = hovering;
}
function SetEnabled(enabled : boolean) {
	cursorEnabled = enabled;
}

function ShowSitCursor(hovering : boolean)
{
    useSitCursor = hovering;
}

function ShowPrivateChatCursor(hovering : boolean)
{
    usePrivateChatCursor = hovering;
}
function ShowTeleportCursor(hovering : boolean)
{
    useTeleportCursor = hovering;
}