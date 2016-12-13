using UnityEngine;
using System.Collections;
using ReactionGrid.Jibe;
using System;
using System.Linq;

public class nameTag : MonoBehaviour {
    private GameObject localPlayer;
    private GameObject playerCamera;
    string currentUser;
    Transform thisTransform;
    float minWidth =0;
    float maxWidth =0;

	// Use this for initialization
	void Start () {
	}

    public void SetDisplayName(IJibePlayer user)
    {
        currentUser = user.Name;

        new GUIStyle("Label").CalcMinMaxWidth(new GUIContent(currentUser), out minWidth, out maxWidth);
        maxWidth += 10;
    }

    public static GameObject FindInChildren(GameObject gameObject, string name)
    {
        foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
        {
            if (t.name == name)
                return t.gameObject;
        }

        return null;
    }
	
    void OnGUI()
    {
        Vector3 pos;
        playerCamera = GameObject.Find("PlayerCam");
        pos = FindInChildren(gameObject, "mesh_node_0").transform.position;
       
        
         
        pos.y += 2.5f;
        //Debug.Log(pos);
        pos = Camera.main.WorldToScreenPoint(pos);
        //Debug.Log(pos);

        GUI.Box(new Rect(pos.x - maxWidth / 2, Screen.height - pos.y, maxWidth, 20), currentUser);

	}


    void Update()
    {
    }
}
