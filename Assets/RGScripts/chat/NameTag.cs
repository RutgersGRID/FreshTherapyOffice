using UnityEngine;
using System.Collections;
using ReactionGrid.Jibe;

public class NameTag : MonoBehaviour
{

    public GUISkin skin;
    public IJibePlayer localPlayer;
    public bool showNameTag = true;
    public float nameTagHeight = 2.1f;
    private GameObject playerCam;
	public Vector3 offSet = Vector3.zero;
    void OnGUI()
    {
        if (showNameTag && localPlayer != null)
        {
            GUI.skin = skin;
            GUIContent content = new GUIContent(localPlayer.Name);
            Vector2 textSize = skin.GetStyle("NameTag").CalcSize(content);
            Vector3 bubblePos = transform.position + new Vector3(0, nameTagHeight, 0) + offSet;
            if (playerCam == null)
            {
                playerCam = GameObject.FindGameObjectWithTag("MainCamera");
            }
            if (playerCam.GetComponent<Camera>().enabled)
            {
                Vector3 screenPos = playerCam.GetComponent<Camera>().WorldToScreenPoint(bubblePos);
                // We render our text only if it's in the screen view port	
                if (screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height && screenPos.z >= 0)
                {

                    Vector2 pos = GUIUtility.ScreenToGUIPoint(new Vector2(screenPos.x, Screen.height - screenPos.y));
                    if (localPlayer.Voice != JibePlayerVoice.IsSpeaking)
                    {
                        GUI.Box(new Rect(pos.x - (textSize.x / 2), pos.y, textSize.x + 5, textSize.y + 20), content);
                    }
                    else
                    {
                        GUI.Box(new Rect(pos.x - (textSize.x / 2), pos.y, textSize.x + 5, textSize.y + 20), content, "SpeakingBubble");
                    }
                }
            }
        }
		else if(localPlayer==null)
		{
			NetworkController netController = GameObject.Find("NetworkController").GetComponent("NetworkController") as NetworkController;
			localPlayer=netController.localPlayer;
		}
    }
}



