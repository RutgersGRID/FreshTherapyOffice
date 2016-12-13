using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// Very simple example of how to use a TextList with a UIInput for chat.
/// </summary>

[RequireComponent(typeof(UIInput))]
[AddComponentMenu("NGUI/Examples/Chat Input")]
public class ChatInput : MonoBehaviour
{
	public Dictionary<string, UITextList> textList = new Dictionary<string, UITextList>();
	public bool fillWithDummyData = false;
	UIInput mInput;
	public static bool mIgnoreNextEnter = false;
	public string myName = "Me";
	public NetworkController netController;
	public string currentGroup = "GlobalChat";
	public UITextList publicTextList;
	public UITextList unionTextList;
	public UITextList managementTextList;
	UILabel myLabel;
	public GameObject button;
	public bool isChattingPrivately;
	public Transform textListPrefab;
	public Transform chatWindow;
	public Transform chatTabPrefab;
	public Transform chatTabParent;
	public GameObject unionChatTab;
	public GameObject managementChatTab;
	public GameObject allChatTab;
	public UISlicedSprite mySlicedSprite;
	public Dictionary<string, GameObject> labels = new Dictionary<string, GameObject>();
	/// <summary>
	/// Add some dummy text to the text list.
	/// </summary>
	
	void Start ()
	{
		netController=GameObject.Find("NetworkController").GetComponent<NetworkController>();
		textList["Union"] = unionTextList;
		textList["Management"] = managementTextList;
		textList["GlobalChat"] = publicTextList;
		labels["Union"] = unionChatTab;
		labels["Management"] = managementChatTab;
		labels["GlobalChat"] = allChatTab;
		mInput = GetComponent<UIInput>();
		myLabel = GetComponentInChildren<UILabel>();
		mySlicedSprite = GetComponentInChildren<UISlicedSprite>();
		if (fillWithDummyData && textList != null)
		{
			for (int i = 0; i < 51; ++i)
			{
				textList[currentGroup].Add(((i % 2 == 0) ? "[FFFFFF]" : "[AAAAAA]") +
					"This is an example paragraph for the text list, testing line " + i + "[-]");
			}
		}
	}
	
	void JibeInit ()
	{
		myName=netController.GetMyName();
	}
	/// <summary>
	/// Pressing 'enter' should immediately give focus to the input field.
	/// </summary>

	void Update ()
	{
		if (Input.GetKeyUp(KeyCode.Return))
		{
			if (!mIgnoreNextEnter && !mInput.selected)
			{
				button.SendMessage("Toggle", false);
				mInput.selected = true;
			}
		}
	}

	/// <summary>
	/// Submit notification is sent by UIInput when 'enter' is pressed or iOS/Android keyboard finalizes input.
	/// </summary>
	void OnSelect (bool isSelected)
	{
		Debug.Log("Selecting chat window");
		if(isSelected)
		{
			mySlicedSprite.spriteName="Chat window";
		}
		else
		{
			mySlicedSprite.spriteName="TextEntry";
		}
	}
	void OnSubmit ()
	{
		if (textList != null)
		{
			// It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
			string text = NGUITools.StripSymbols(mInput.text);

			if (!string.IsNullOrEmpty(text))
			{
				if(text=="/dance")
				{
					GameObject.Find("localPlayer").BroadcastMessage("StartDancing");
				}
				else
				{
					text= myName+": " + text;
					textList[currentGroup].Add(text);
					if(!isChattingPrivately)
					{
						SendMessage(currentGroup, text);
					}
					else
					{
						SendPrivateChatMessage(text, Int32.Parse(currentGroup));
					}
				}
				mInput.text = "";
			}
		}
		//mIgnoreNextEnter = true;
	}
	
	public void AddMessage (string chatMessage, string sendingUser, string groupName)
	{
		//the sender should have already appended their name to the chat message
		textList[groupName].Add(chatMessage);
		if(!groupName.Equals(currentGroup))
		{
			labels[groupName].SendMessage("BlinkMe", true);
		}
		button.SendMessage("BlinkMe", true);
	}
	void SendMessage(string groupName, string chatMessage)
	{
		netController.SendChatMessage(chatMessage);
	}
	public void AddPrivateChatMessage (string chatMessage, string sendingUser, int sendingUserID)
	{
		//THIS IS TRIGGERED WHEN YOU RECEIVE A PRIVATE CHAT
		Debug.Log("Received private chat: " + chatMessage + "from user:" + sendingUserID);
		if(!textList.ContainsKey(""+sendingUserID))
		{
			AddNewChatTab(sendingUser, ""+sendingUserID);
		}
		textList[""+sendingUserID].Add(chatMessage);
		if(!currentGroup.Equals(""+sendingUserID))
		{
			labels[""+sendingUserID].SendMessage("BlinkMe", true);
		}
		button.SendMessage("BlinkMe", true);
	}
	public void SendPrivateChatMessage (string chatMessage, int recipientID)
	{
        netController.SendPrivateChatMessage(chatMessage, recipientID);
	}
	public bool IsChatting()
	{
		return mInput.selected;
	}
	public void AddDebugMessage(string message)
	{
		textList[currentGroup].Add("Debug:" + message);
	}
	public void InitiatePrivateChat(string idToPrivateChatWith, string nameToPrivateChatWith)
	{
		Debug.Log("Opening private  chat with user who has id : " + idToPrivateChatWith);
		if(!textList.ContainsKey(idToPrivateChatWith))
		{
			AddNewChatTab(nameToPrivateChatWith, idToPrivateChatWith);
		}
		GameObject.FindWithTag("WindowLabel").GetComponent<UILabel>().text=nameToPrivateChatWith;
		isChattingPrivately=true;
		currentGroup=idToPrivateChatWith;
		DisableOtherGroups();
	}
	private void AddNewChatTab(string name, string id)
	{
		Transform newTextList = GameObject.Instantiate(textListPrefab, new Vector3(3f, 78.16923f, 0f), Quaternion.identity) as Transform;
		newTextList.name=id;
		newTextList.parent=chatWindow;
		newTextList.transform.localScale = new Vector3(1f, 0.7868515f, 1f);
		textList[id] = newTextList.gameObject.GetComponent<UITextList>();
		newTextList.GetComponent<ToggleMe>().startPosition = new Vector3(3f, 78.16923f, 0f);
		Transform newChatTab = GameObject.Instantiate(chatTabPrefab) as Transform;
		Vector3 position = new Vector3(0f,0f, -15f);
		Vector3 scale = new Vector3(.475f, .7f, 1f);
		newChatTab.parent = chatTabParent;
		newChatTab.localPosition=position;
		newChatTab.localScale=scale;
		newChatTab.GetComponentInChildren<UILabel>().text=name;
		newChatTab.GetComponentInChildren<ChangeGroup>().groupToJoin = id;
		newChatTab.GetComponentInChildren<ChangeGroup>().isPrivateGroup = true;
		newChatTab.GetComponentInChildren<ChangeGroup>().nameToDisplay = name;
		chatTabParent.GetComponentInChildren<UIGrid>().repositionNow=true;
		labels["" + id] = newChatTab.gameObject;
	}
	public void DisableOtherGroups()
	{
		foreach(string key in textList.Keys)
		{
			if(!key.Equals(currentGroup))
			{
				textList[key].SendMessage("Toggle", true);
				if(labels[key]!=null) //double check that we didn't delete the tab.
				{
					labels[key].GetComponentInChildren<UISlicedSprite>().spriteName="LabelBackground";
				}
			}
			else
			{
				textList[key].SendMessage("Toggle", false);
			}
		}
	}
}