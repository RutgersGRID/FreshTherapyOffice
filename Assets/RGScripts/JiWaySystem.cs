/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * JiWaySystem.cs Revision 1.4.1107.19
 * Provides Jibe to Jibe teleport functionality */

using UnityEngine;
using System.Collections;

public class JiWaySystem : MonoBehaviour
{
    private bool menu = false;
    public GUISkin jibeSkin;
    public string customTitleMessage;
    public bool displayTitleMessage = false;
    public string webPlayerWarning = "Warning: Traveling will exit you from this world"; // only true for web clients
    public string standaloneWarning = "The JiWay will open a new Jibe world in your default web browser";
    public Texture2D background;

    //the public variables developers set via the inspector as needed
    public Texture2D[] locationPics;        //the representative pictures of the locations
    private Texture2D activeLocationPic;

    public string[] locationURLS;           //the URLs of the worlds
    private string locationUrlToLoad;

    public string[] locationName;           //simple descriptive names of the locations
    private string currentLocationName;

    public string[] locationDescription;    //descriptions of the locations
    private string currentLocationDescription;

    private string shotStyle = "JiWayLocPics";// "PictureButtonsSmall";

    private Rect menuWindow;
    private Vector2 scrollPosition;
    private int _selected = -1;
    private int _currentSelection = -1;
    public NetworkController netController;

    public bool showExitButton = false;
    public float verticalMaxHeight = 420;

    public void Start()
    {
        // Set the max height for the JiWay menu.
        // If the screen is too small vertically, ensure the menu doesn't overlap chat system
        float verticalHeight = Screen.height - 56 < verticalMaxHeight ? Screen.height - 56 : verticalMaxHeight;

        menuWindow = new Rect(40, 28, 410, verticalHeight);
        if (netController == null)
        {
            netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //a colission triggers the menu - ensure you set a collider to be a trigger for this to work
        menu = true;
    }

    void OnTriggerExit(Collider other)
    {
        HideJiWay();
    }

    void LaunchTravel()
    {
        if (netController == null)
        {
            netController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        }
        // in web player user selects destination, this triggers the url launch and exit from this world
        if (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer)
        {
            HideJiWay();
            netController.DisconnectLocalPlayer(locationUrlToLoad);
        }
        else
        {
            HideJiWay();
            Application.OpenURL(locationUrlToLoad);            
        }
    }

    void Update()
    {   
        //if a change in what location the user selected
        if (_currentSelection != _selected)
        {
            _currentSelection = _selected;
            currentLocationDescription = locationDescription[_selected];
            currentLocationName = locationName[_selected];
            locationUrlToLoad = locationURLS[_selected];
            activeLocationPic = locationPics[_selected];
        }
    }

    void OnGUI()
    {
        GUI.skin = jibeSkin;
        if (menu)
        {
            GUILayout.BeginArea(menuWindow, "", "Background");
            if (displayTitleMessage)
            {
                GUILayout.Label(customTitleMessage, "WelcomePrompt", GUILayout.MaxWidth(300));
            }
            GUILayout.Space(4);

            //start the scroll view of available worlds
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(menuWindow.width - 10), GUILayout.Height(85));
            _selected = GUILayout.Toolbar(_selected, locationPics, shotStyle);
            GUILayout.EndScrollView();

            //if there is a selection by the user
            if (_selected != -1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(1);
                if (GUILayout.Button("Travel to " + currentLocationName, "MenuButton"))
                {
                    LaunchTravel();
                }
                GUILayout.Space(4);
                if (showExitButton)
                {
                    if (GUILayout.Button("Exit the JiWay", "MenuButton"))
                    {
                        HideJiWay();
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                if (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer)
                {
                    GUILayout.Label(webPlayerWarning, GUILayout.MaxWidth(300));
                }
                else
                {
                    GUILayout.Label(standaloneWarning, GUILayout.MaxWidth(300));
                }
                GUILayout.Label(currentLocationName + ":  " + currentLocationDescription, "Label", GUILayout.MaxWidth(menuWindow.width - 10));
                GUILayout.Box(activeLocationPic, "CenteredMenu", GUILayout.Height(200));
                GUILayout.EndVertical();
            }
            else if (showExitButton)
            {
                if (GUILayout.Button("Exit JiWay", "MenuButton"))
                {
                    HideJiWay();
                }
            }
            GUILayout.EndArea();
        }
    }

    private void HideJiWay()
    {
        menu = false;
        _selected = -1;
        _currentSelection = -1;
    }

}
