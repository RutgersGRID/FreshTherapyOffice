using UnityEngine;
using System.Collections;
using UnityEditor;

public class JibePrefs : EditorWindow
{
    public Texture jibeLogo;
    public GUISkin gskin;
    string avatarModel = PlayerPrefs.GetInt("avatar").ToString();
    string username = PlayerPrefs.GetString("name");
    string dynamicroomid = PlayerPrefs.GetInt("DynamicRoomId").ToString();
    string avatarID = PlayerPrefs.GetString("useruuid");
    string skin = PlayerPrefs.GetString("skin");
    string hair = PlayerPrefs.GetString("hair");

    [MenuItem("Window/Jibe/Edit Stored Data")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        JibePrefs window = (JibePrefs)EditorWindow.GetWindow(typeof(JibePrefs), true, "Edit Stored Data");
        window.position = new Rect(350, 200, 550, 400);
        //window.ShowUtility ();        
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
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label("Edit Stored Data", EditorStyles.largeLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.Label("Select an action", EditorStyles.boldLabel);
        GUILayout.Space(25);

        //avatarid
        GUILayout.BeginHorizontal();
        GUILayout.Label("Avatar model", EditorStyles.label);
        avatarModel = GUILayout.TextField(avatarModel);
        GUILayout.EndHorizontal();

        //name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Avatar name", EditorStyles.label);
        username = GUILayout.TextField(username);
        GUILayout.EndHorizontal();

        //id
        GUILayout.BeginHorizontal();
        GUILayout.Label("Avatar UUID", EditorStyles.label);
        avatarID = GUILayout.TextField(avatarID);
        GUILayout.EndHorizontal();

        //skin
        GUILayout.BeginHorizontal();
        GUILayout.Label("Skin", EditorStyles.label);
        skin = GUILayout.TextField(skin);
        GUILayout.EndHorizontal();

        //hair
        GUILayout.BeginHorizontal();
        GUILayout.Label("Hair", EditorStyles.label);
        hair = GUILayout.TextField(hair);
        GUILayout.EndHorizontal();

        //dynRoom
        GUILayout.BeginHorizontal();
        GUILayout.Label("Dynamic Room", EditorStyles.label);
        dynamicroomid = GUILayout.TextField(dynamicroomid);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update prefs"))
        {
            UpdateAll();
        }

        if (GUILayout.Button("Clear all prefs"))
        {
            DeleteAll();
        }
        if (GUILayout.Button("Clear all editor prefs"))
        {
            DeleteAllEditor();
        }
        GUILayout.Label("", EditorStyles.miniBoldLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Jibe, from ReactionGrid", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();


        GUILayout.EndHorizontal();

        if (GUILayout.Button("ReactionGrid Homepage", "Hyperlink"))
        {
            Help.BrowseURL("http://reactiongrid.com/");
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Documentation and Patches", "Hyperlink"))
        {
            Help.BrowseURL("http://jibemix.com/jibedownloads/");
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Help and Support", "Hyperlink"))
        {
            Help.BrowseURL("http://metaverseheroes.com/");
        }
        if (GUILayout.Button("Close", GUILayout.Width(100)))
        {
            Close();
        }
    }

    private void UpdateAll()
    {
        int model = -1;
        int.TryParse(avatarModel, out model);
        int roomId = -1;
        int.TryParse(dynamicroomid, out roomId);
        PlayerPrefs.SetInt("avatar", model);
        PlayerPrefs.SetString("name", username);
        PlayerPrefs.SetInt("DynamicRoomId", roomId);
        PlayerPrefs.SetString("useruuid", avatarID);
        PlayerPrefs.SetString("skin", skin);
        PlayerPrefs.SetString("hair", hair);
    }
    void DeleteAll()
    {
        if (EditorUtility.DisplayDialog("Warning", "This will clear the playerprefs! Are you sure?", "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
            avatarModel = PlayerPrefs.GetInt("avatar").ToString();
            username = PlayerPrefs.GetString("name");
            dynamicroomid = PlayerPrefs.GetInt("DynamicRoomId").ToString();
            avatarID = PlayerPrefs.GetString("useruuid");
            skin = PlayerPrefs.GetString("skin");
            hair = PlayerPrefs.GetString("hair");
        }
    }
    void DeleteAllEditor()
    {
        if (EditorUtility.DisplayDialog("Warning", "This will clear the editorprefs! Are you sure?", "Yes", "No"))
        {
            EditorPrefs.DeleteAll();            
        }
    }
}
