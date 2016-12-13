/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * UIBase.cs Revision 1.4.1107.20
 * Main GUI script for Jibe  */

using UnityEngine;
using System.Collections;

public enum SettingsIconsAnchor {TopLeftCorner, TopRightCorner, TopLeftCornerColumn, TopRightCornerColumn };

public class UIBase : MonoBehaviour
{
    public GUISkin skin;
    GUIStyle settingsButtonStyle;
    // Icons for settings can be positioned in different places around the screen
    public SettingsIconsAnchor settingsAnchorPoint = SettingsIconsAnchor.TopRightCorner;
	private bool showClockGUI;
    private Rect settingsIconPosition;
    private Rect volIconPosition;
    private Rect miniMapIconPosition;
    private Rect hangerIconPosition;
    private Rect micIconPosition;
	private Rect questionIconPosition; 
	private Rect countdownIconPosition;
	public Rect rectInInspector = new Rect(-20,20,200,125);
	public GUIContent[] tips;
	public bool visible;
	private int index =0;
    private bool positionsCalculated = false;
    private bool mapPositionCalculated = false;

    // Clicking the settings icon shows the instructions UI
    public Texture2D settingsIcon;
    public float settingsIconSize = 22;
	public Texture2D stopwatchIcon;
    public Texture2D hangerIcon;
    // Toggle volume level - the more icons there are, the more refined the toggle
    // for example, two icons and it would be on/off. Three and you get 0, 50%, 100% of max volume
    // Four and you get 0, 33%, 66% 100
    public Texture2D[] audioIcons;
    public int volume = 1;
    private float currentVolume = 1.0f;
    private float volumeSettings;

    // The overlay image to show on top of the minimap to make the map edges appear less harsh
    public Texture2D mapOverlay;
    private bool showMapOverlay = true;

    private float miniMapX = 0.0f;
    private float miniMapY = 0.0f;
    private float mapHeight = 0.0f;
    private float mapWidth = 0.0f;

    // Control minimap visibility
    public bool showMiniMap = true;
    public Texture2D miniMapIcon;
	public Texture2D questionmarkIcon;
    // Show the current scene name on screen
    public bool showLevelName = false;

    // Graphic to use to show simple movement cheat sheet
    public Texture2D instructionsUI;
    private GameObject jibe;

    // Must reference Network Controller and MiniMap components
    public NetworkController networkController;
    public GameObject miniMap;

    // A new way to offer the choice of changing from one scene to another - 
    // enter the available scenes in here for instant transfer to next level on click
    public bool showRoomChoices = false;
    public string[] availableRooms;

    private bool showInstructionsUI = false;

    void Start()
    {
		rectInInspector.x+=Screen.width-rectInInspector.width;
        volumeSettings = audioIcons.Length - 1;
        if (networkController == null)
        {
            networkController = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
        }
        if (miniMap == null)
        {
            DetectMiniMapCamera();
        }
        CalculateMiniMapPosition();
        ToggleMiniMap();
    }

    public float GetCurrentVolume()
    {
        return currentVolume;
    }

    private void ConfigureSettingsIconStyle()
    {
        settingsButtonStyle = new GUIStyle("SettingsIcons");
        settingsButtonStyle.fixedHeight = settingsIconSize;
        settingsButtonStyle.fixedWidth = settingsIconSize;
		settingsButtonStyle.alignment=TextAnchor.MiddleCenter;
    }

    private void DetectMiniMapCamera()
    {
        if (miniMap == null)
        {
            if (GameObject.Find("MiniMapCamera") != null)
            {
                miniMap = GameObject.Find("MiniMapCamera");
            }
            else
            {
                foreach (Camera cam in FindSceneObjectsOfType(typeof(Camera)))
                {
                    if (cam.gameObject.GetComponent<MiniMap>() != null)
                    {
                        miniMap = cam.gameObject;
                        break;
                    }
                }
            }
        }
    }

    public Rect GetMicIconPosition()
    {
        CalculateOffsets();
        return micIconPosition;
    }
    public GUIStyle GetSettingsButtonStyle()
    {
        if (settingsButtonStyle == null) ConfigureSettingsIconStyle();
        return settingsButtonStyle;
    }

/*    void OnGUI()
    {        
/*        GUI.skin = skin;
        if (settingsButtonStyle == null) ConfigureSettingsIconStyle();

        if (!positionsCalculated)
        {
            CalculateOffsets();
            positionsCalculated = true;
        }

        // Set depth so UI elements appear over the rest of the world
        GUI.depth = 3;
               
        if (showMiniMap && miniMap != null)
        {
            if (showMapOverlay)
            {
                if (!mapPositionCalculated)
                {
                    CalculateMiniMapPosition();
                    mapPositionCalculated = true;
                }
                GUILayout.BeginArea(new Rect(miniMapX, miniMapY, mapWidth, mapHeight), mapOverlay);
                GUILayout.EndArea();
            }
        }
        if (miniMap != null)
        {
            if (GUI.Button(miniMapIconPosition, miniMapIcon, settingsButtonStyle))
            {
                showMiniMap = !showMiniMap;
                ToggleMiniMap();
            }
        }
        if (showLevelName)
        {
            // Level / Scene name is shownon screen with this code here - edit the Rect values to change position
            GUI.Label(new Rect((2 * Screen.width / 3), 0, 149, 20), Application.loadedLevelName, "MapLocation");
        }

        Texture2D currentVol = audioIcons[volume];

        // Toggle the volume level
        if (GUI.Button(volIconPosition, currentVol, settingsButtonStyle))
        {
            if (volume < volumeSettings)
            {
                volume++;
            }
            else
            {
                volume = 0;
            }
            currentVolume = volume / volumeSettings;
            Debug.Log("Target volume: " + currentVolume);
            if (jibe == null)
            {
                // should only need to do this once
                jibe = GameObject.Find("Jibe");
            }
            if (jibe != null)
            {
                if (jibe.audio != null)
                    jibe.audio.volume = currentVolume;
            }
        }
        
        // Toggle the instructions UI
        if (GUI.Button(settingsIconPosition, settingsIcon, settingsButtonStyle))
        {
           
        }

        if (GUI.Button(hangerIconPosition, hangerIcon, settingsButtonStyle))
        {
            networkController.ChangeLevel("DressingRoom");
        }
		//RSO begin
		if(GUI.Button(questionIconPosition, questionmarkIcon, settingsButtonStyle))
		{
			visible=!visible;
            //showInstructionsUI = !showInstructionsUI;
		}
		if(visible)
		{
			Rect inspectorRect= rectInInspector;
			GUI.Box(inspectorRect, tips[index]);
			inspectorRect.width=20;
			inspectorRect.height=20;
			inspectorRect.x+=200;
			inspectorRect.y+=20;
			if(GUI.Button(inspectorRect, "X"))
			{
				visible=false;
			}
			inspectorRect.width=50;
			inspectorRect.height=20;
			inspectorRect.y+=85;
			inspectorRect.x-=105;
			if(GUI.Button(inspectorRect, "Back"))
			{
				index--;
			}
			inspectorRect.x+=50;
			if(GUI.Button(inspectorRect, "Next"))
			{
				index++;
			}
			if(index<0)
			{
				index=tips.Length-1;
			}
			if(index>=tips.Length)
			{
				index=0;
			}
		}
		if(networkController.CheckIfLocalPlayerIsInGroup("GlobalChat"))
		{
			showClockGUI = GUI.Toggle(countdownIconPosition, showClockGUI, stopwatchIcon, settingsButtonStyle);
			if(showClockGUI==true)
			{
			}
		}
		//RSO end
        if (showInstructionsUI)
        {
            GUI.DrawTexture(new Rect(Screen.width / 3, Screen.height / 6, 256, 192), instructionsUI);
        }

        // Show the available scenes for changing rooms
        if (showRoomChoices)
        {
            for (int i = 0; i < availableRooms.Length; i++)
            {
                if (Application.CanStreamedLevelBeLoaded(availableRooms[i]))
                {
                    // These buttons are set to start from the left edge, and assume buttons are 25 pixels high
                    // To edit positions, edit the values in this Rect
                    if (GUI.Button(new Rect(0, (i * 25), 100, 25), availableRooms[i]))
                    {
                        networkController.ChangeLevel(availableRooms[i]);
                    }
                }

            }
        }*/
//    }

    private void CalculateOffsets()
    {
        // Settings Icons will be presented in the following order: 1. Settings GUI. 2. Volume. 3. Audio Icon Volume.
        // For more than three icons, you will need to tweak the positioning by changing the following integers:
        int micIconOffset = 3;
        int hangerIconOffset = 2;
        int settingsIconOffset = -2;
        int volumeIconOffset = 1;
        int miniMapIconOffset = 0;
		int questionIconOffset= 0;
		int countdownIconOffset = -1;
        Debug.Log("Calculating icon positions");

        switch (settingsAnchorPoint)
        {
            case SettingsIconsAnchor.TopLeftCorner:
                settingsIconPosition = new Rect((settingsIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                volIconPosition = new Rect((volumeIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                miniMapIconPosition = new Rect((miniMapIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                micIconPosition = new Rect((micIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                hangerIconPosition = new Rect((hangerIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                questionIconPosition = new Rect((questionIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                countdownIconPosition = new Rect((countdownIconOffset * settingsIconSize), miniMapY, settingsIconSize, settingsIconSize);
                break;
            case SettingsIconsAnchor.TopLeftCornerColumn:
                settingsIconPosition = new Rect(0,  (settingsIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                volIconPosition = new Rect(0, (volumeIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                miniMapIconPosition = new Rect(0, (miniMapIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                micIconPosition = new Rect(0, (micIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                hangerIconPosition = new Rect(0, (hangerIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                questionIconPosition = new Rect(0, (questionIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                countdownIconPosition = new Rect(0, (countdownIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                break;
            case SettingsIconsAnchor.TopRightCornerColumn:
                settingsIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (settingsIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                volIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (volumeIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                miniMapIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (miniMapIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                micIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (micIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                hangerIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (hangerIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                questionIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (questionIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                countdownIconPosition = new Rect(Screen.width - settingsIconSize, miniMapY + (countdownIconOffset * settingsIconSize), settingsIconSize, settingsIconSize);
                break;
            case SettingsIconsAnchor.TopRightCorner:
                settingsIconPosition = new Rect(Screen.width - settingsIconSize - (settingsIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                volIconPosition = new Rect(Screen.width - settingsIconSize - (volumeIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                miniMapIconPosition = new Rect(Screen.width - settingsIconSize - (miniMapIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                micIconPosition = new Rect(Screen.width - settingsIconSize - (micIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                hangerIconPosition = new Rect(Screen.width - settingsIconSize - (hangerIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                questionIconPosition = new Rect(Screen.width - settingsIconSize - (questionIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                countdownIconPosition = new Rect(Screen.width - settingsIconSize - (countdownIconOffset * settingsIconSize), 0, settingsIconSize, settingsIconSize);
                break;
        }
    }

    private void CalculateMiniMapPosition()
    {
        if (miniMap != null && miniMap.GetComponent<MiniMap>() != null)
        {
            float mapPadding = miniMap.GetComponent<MiniMap>().GetMapPadding();
            mapWidth = miniMap.GetComponent<MiniMap>().GetMapWidth() + mapPadding;
            mapHeight = miniMap.GetComponent<MiniMap>().GetMapHeight() + mapPadding;
            MapAnchor miniMapAnchor = miniMap.GetComponent<MiniMap>().GetMapAnchor();
            switch (miniMapAnchor)
            {
                case MapAnchor.BottomLeft:
                    miniMapX = 0;
                    miniMapY = Screen.height - mapHeight - mapPadding;
                    break;
                case MapAnchor.BottomRight:
                    miniMapX = Screen.width - mapWidth;
                    miniMapY = Screen.height - mapHeight - mapPadding;
                    break;
                case MapAnchor.TopLeft:
                    miniMapX = 0;
                    miniMapY = 0;
                    break;
                case MapAnchor.TopRight:
                    miniMapX = Screen.width - mapWidth - mapPadding;
                    miniMapY = 0;
                    break;
                case MapAnchor.TopCenter:
                    miniMapX = (Screen.width / 2) - (mapWidth / 2) - (mapPadding / 2);
                    miniMapY = 0;
                    break;
                default:
                    break;
            }
        }
    }
    private void ToggleMiniMap()
    {
        if (miniMap != null)
        {
            miniMap.GetComponent<Camera>().enabled = showMiniMap;
        }
    }
}
