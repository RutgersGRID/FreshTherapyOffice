using UnityEngine;
using System.Collections;
using UnityEditor;

public class JibeWelcome : EditorWindow
{
    public Texture jibeLogo;
    public GUISkin gskin;

    private bool _sceneChangeDetected = true;
    private string _currentScene = "";

    private string _serverIP = "";
    private int _roomCount = 1;
    private string[] _availableRooms;
    private string _defaultRoom = "";
    private string _zone = "";
    private string _port = "";
    private int _jibePort = 0;
    private string _password = "";

    private string _installedVersion = "1.1";
    private string _mostRecentEdition = "??";
    private bool _hasCheckedForUpdates = false;
    private bool _saveSettingsAsDefault = false;
    public JibeConfig configSettings;

    // Add menu named "Jibe - Welcome" to the Window menu
    [MenuItem("Window/Jibe/Welcome")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        JibeWelcome window = (JibeWelcome)EditorWindow.GetWindow(typeof(JibeWelcome), true, "Welcome");
        window.position = new Rect(350, 200, 550, 500);
        window.ShowUtility();
    }
    
    public void OnEnable()
    {
        DetectJibeConfiguration();
    }

    public void OnSceneGUI()
    {
        if (_currentScene != EditorApplication.currentScene)
        {
            _currentScene = EditorApplication.currentScene;
            _sceneChangeDetected = true;
        }
    }
    
    private WWW w;
    private void DoUpdateCheck()
    {
        _mostRecentEdition = "Checking...";
        w = new WWW("http://jibemix.com/currentjibeversion.aspx");        
    }


    private void DetectJibeConfiguration()
    {
        if (FindSceneObjectsOfType(typeof(JibeConfig)) != null)
        {
            JibeConfig detectedConfig = null;
            foreach (JibeConfig config in FindSceneObjectsOfType(typeof(JibeConfig)))
            {
                detectedConfig = config;
                break;
            }
            if (configSettings == null && detectedConfig != null)
            {
                configSettings = detectedConfig;
            }
            if (configSettings != null)
            {
                _serverIP = configSettings.ServerIP;
                _defaultRoom = configSettings.Room;
                _roomCount = configSettings.RoomList.Length;                
                _availableRooms = configSettings.RoomList;
                ConfigureAvailableRooms();
                _zone = configSettings.Zone;
                _jibePort = configSettings.ServerPort;
                _port = _jibePort.ToString();
                _password = configSettings.RoomPassword;
                _installedVersion = configSettings.GetVersion();
            }
        }
    }

    private void ConfigureAvailableRooms()
    {        
        if (_roomCount == 0)
        {
            _roomCount = 1;
            _availableRooms = new string[_roomCount];
            _availableRooms[0] = _defaultRoom;
        }
        
        if (_roomCount != _availableRooms.Length)
        {
            string[] newRooms = new string[_roomCount];
            for (int i = 0; i < _roomCount; i++)
            {
                if (i <= _availableRooms.Length - 1)
                {
                    newRooms[i] = _availableRooms[i];
                }
            }
            _availableRooms = newRooms;
        }
        bool defaultInList = false;
        foreach (string room in _availableRooms)
        {
            if (room == _defaultRoom)
            {
                defaultInList = true;
                break;
            }
        }
        if (!defaultInList)
        {
            _roomCount++;
            string[] newRooms = new string[_roomCount];
            newRooms[0] = _defaultRoom;
            for (int i = 1; i < _roomCount; i++)
            {
                if (i <= _availableRooms.Length)
                {
                    newRooms[i] = _availableRooms[i-1];
                }
            }
            _availableRooms = newRooms;
        }
    }

    private void UpdateJibeConfigurationFromStoredPrefs()
    {
        _serverIP = EditorPrefs.GetString("ServerIP");
        string allRooms = EditorPrefs.GetString("AvailableRooms");
        string[] availableRooms = allRooms.Split(',');
        _availableRooms = availableRooms;
        _defaultRoom = _availableRooms[0];
        _roomCount = _availableRooms.Length;
        ConfigureAvailableRooms();
        _zone = EditorPrefs.GetString("Zone");
        _jibePort = EditorPrefs.GetInt("ServerPort");
        _port = _jibePort.ToString();
        _password = EditorPrefs.GetString("Password");
    }

    private void UpdateJibeConfigurationScriptsInScene()
    {
        if (configSettings != null)
        {            
            configSettings.ServerIP = _serverIP;
            int.TryParse(_port, out _jibePort);
            configSettings.ServerPort = _jibePort;
            configSettings.RoomPassword = _password;
            configSettings.Zone = _zone;
            configSettings.Room = _availableRooms[0];
            configSettings.RoomList = _availableRooms;           
        }
    }

    void Update()
    {
        if (_sceneChangeDetected)
        {
            DetectJibeConfiguration();
            _sceneChangeDetected = false;
        }
        if (!_hasCheckedForUpdates)
        {
            if (w != null && w.isDone && w.error == null)
            {
                _mostRecentEdition = w.text;
                Debug.Log("Retrieved latest Jibe version from server: " + _mostRecentEdition);
                GUI.changed = true;
                _hasCheckedForUpdates = true;
            }
            else if (w != null && w.error != null)
            {
                _mostRecentEdition = w.error;
                Debug.Log("Unable to retrieve latest Jibe version from server");
                _hasCheckedForUpdates = true;
            }
        }
    }

    void OnGUI()
    {
        if (gskin == null)
        {
            gskin = EditorGUIUtility.Load("jibeEditorSkin.GUISkin") as GUISkin;
        }
        GUI.skin = gskin;

        if (jibeLogo == null)
        {
            jibeLogo = EditorGUIUtility.Load("jibe1.png") as Texture;
        }
        GUI.color = new Color(1, 1, 1, 1);
        GUI.contentColor = Color.white;
        Rect logoRect = new Rect(position.width - 80, position.height - 100, 80, 100);
        //GUI.Box(logoRect, "");
        GUI.DrawTexture(logoRect, jibeLogo);
        GUI.color = Color.white;

        GUILayout.BeginVertical();
            GUILayout.Label("Welcome To Jibe", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
                GUILayout.Label("Installed: " + _installedVersion);
                GUILayout.Label("Latest: " + _mostRecentEdition);
                if (_mostRecentEdition == "??" || _mostRecentEdition == "")
                {
                    if (GUILayout.Button("Check for updates"))
                    {
                        _hasCheckedForUpdates = false;
                        DoUpdateCheck();
                    }
                }
            GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                    GUILayout.Space(10);
                    GUILayout.Label("To configure your Jibe installation, please enter the following settings:", EditorStyles.boldLabel);
                    GUILayout.Space(4);
                    GUILayout.BeginHorizontal();
                        GUILayout.Label("");
                        GUILayout.Space(2);                                        
                        if (GUILayout.Button("Load Default Preferences", GUILayout.Width(200)))
                        {
                            UpdateJibeConfigurationFromStoredPrefs();
                        }                    
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("Server IP");
                        GUILayout.Space(2);
                        _serverIP = GUILayout.TextField(_serverIP);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("Server Port");
                        GUILayout.Space(2);
                        _port = GUILayout.TextField(_port);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("Password");
                        GUILayout.Space(2);
                        _password = GUILayout.TextField(_password);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("Zone");
                        GUILayout.Space(2);
                        _zone = GUILayout.TextField(_zone);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("Rooms");
                        GUILayout.Space(2);                        
                        GUILayout.Label(_roomCount.ToString());
                        if (GUILayout.Button("+", GUILayout.Width(20)))
                        {
                            _roomCount++;
                            ConfigureAvailableRooms();
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            if (_roomCount > 1) _roomCount--;
                            ConfigureAvailableRooms();
                        }
                        
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < _availableRooms.Length; i++ )
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Room " + (i + 1).ToString());
                        GUILayout.Space(2);
                        if (_availableRooms[i] == null)
                            _availableRooms[i] = string.Empty;
                        _availableRooms[i] = GUILayout.TextField(_availableRooms[i]);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Label("", EditorStyles.miniBoldLabel);

                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Save", GUILayout.Width(100)))
                        {
                            UpdateJibeConfigurationScriptsInScene();
                            if (_saveSettingsAsDefault) StoreJibeConfigurationPrefs();
                            EditorApplication.SaveScene(EditorApplication.currentScene);                            
                        }                    
                        _saveSettingsAsDefault = GUILayout.Toggle(_saveSettingsAsDefault, "Set as default configuration");
                    GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                    GUILayout.Label("Jibe, from ReactionGrid", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("ReactionGrid Homepage", "Hyperlink"))
                    {
                        Help.BrowseURL("http://reactiongrid.com/");
                    }
                    GUILayout.Space(8);
                    if (GUILayout.Button("Documentation and Patches", "Hyperlink"))
                    {
                        Help.BrowseURL("http://jibemix.com/jibedownloads/");
                    }
                    GUILayout.Space(8);
                    if (GUILayout.Button("Help and Support", "Hyperlink"))
                    {
                        Help.BrowseURL("http://metaverseheroes.com/");
                    }
                    GUILayout.Space(8);        
                GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void StoreJibeConfigurationPrefs()
    {                
        EditorPrefs.SetString("ServerIP", _serverIP);
        EditorPrefs.SetInt("ServerPort", _jibePort);
        EditorPrefs.SetString("Password", _password);
        EditorPrefs.SetString("Zone", _zone);
        EditorPrefs.SetString("AvailableRooms", string.Join(",", _availableRooms));
    }
}
