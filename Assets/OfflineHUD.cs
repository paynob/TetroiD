using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;

public class OfflineHUD : MonoBehaviour {

	public MyNetworkManager manager;
	public GameObject gameManagerPref;
	public GUISkin skin;
	int posX,posY,width, height;
	bool exist = true;
	
	string sufix = "";
	int sufixInt = 1;
	string roomName = "roomName";
	string playerName = "nickname";

	void Start (){
		Languages.SetLanguage (Languages.Language.Spanish);
		roomName = Languages.Translate ("_ROOMNAME_");
			
	}
	void OnGUI (){
		posX = Screen.width / 4;
		posY = Screen.height / 2;
		width = Screen.width / 2;
		height = Screen.height / 12;
		skin.button.fontSize = height/2;
		skin.textField.fontSize = height / 2;
		skin.textField.alignment = TextAnchor.MiddleCenter;

		if (Time.timeSinceLevelLoad < 2.25f) //My Camera zoom out duration
			return;

		if (GUI.Button (new Rect (posX, posY-height, width, height), Languages.Translate("_LOCAL_"),skin.button)) {
			manager.StartHost();
		}
		posY += height * 2;
		roomName = GUI.TextField (new Rect (posX, posY, width, height), roomName,skin.textField);
		if (roomName.Length > 10) {
			roomName = roomName.Substring (0, 10);
			//make a sound or something
		}
		posY += height;
		if (GUI.Button (new Rect (posX, posY, width/2, height), Languages.Translate("_CREATE_"),skin.button)) {
			manager.StartMatchMaker ();
			manager.matchMaker.ListMatches (0, 20, "", OnMatchListAndCreate);
		}
		posX += width/2;
		if (GUI.Button (new Rect (posX, posY, width/2, height), Languages.Translate("_JOIN_"),skin.button)) {
			manager.StartMatchMaker ();
			manager.matchMaker.ListMatches (0, 20, "", OnMatchListAndJoin);
		}
		if ( !exist ) {
			posY += height;
			GUI.Label (new Rect (posX, posY, width, height), "The match doesnt exist");
		}
	}
	
	void CreateNick (){
		GameObject nick = new GameObject ();
		nick.name = playerName;
		nick.tag = "PlayerName";
		DontDestroyOnLoad (nick);
	}
	
	void Create (){
		if (manager.matchInfo == null) {
			if (manager.matches == null) {
				
			} else {
				if (manager.matches.Count > 0) {
					
					do{
						exist = false;
						foreach (var match in manager.matches) {
							if (match.name == (roomName+sufix) ) {
								sufix = ""+sufixInt;
								roomName += sufix;
								sufixInt++;
								exist = true;
								break;
							}
						}
					}while ( exist );
				}
			}
			CreateNick();
			manager.matchName = roomName;
			manager.matchMaker.CreateMatch (manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
			
		}
	}
	
	void OnMatchListAndCreate(ListMatchResponse matchList)
	{
		manager.matches = matchList.matches;
		Create ();
	}
	
	void OnMatchListAndJoin(ListMatchResponse matchList)
	{
		manager.matches = matchList.matches;
		Join ();
	}
	
	void Join (){
		exist = false;
		if (manager.matchInfo == null) {
			if ( manager.matches != null ){
				if ( manager.matches.Count > 0 ){
					foreach ( var match in manager.matches ){
						if ( match.name == roomName ){
							CreateNick();
							manager.matchName = match.name;
							manager.matchSize = (uint)match.currentSize;
							manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
							exist = true;
							break;
						}
					}
				}
			}
		}
		
	}
}
