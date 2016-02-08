using UnityEngine;
using UnityEngine.Networking.Match;

public class OfflineGUI : MonoBehaviour {

	public MyNetworkManager manager;
	int posX,posY,width, height;
	bool exist = true;

	string sufix = "";
	int sufixInt = 1;
	string roomName = "random";
	string playerName = "nickname";


	void Awake (){
		manager.StartMatchMaker ();
	}

	void OnGUI (){
		posX = Screen.width / 4;
		posY = Screen.height / 3;
		width = Screen.width / 2;
		height = Screen.height / 12;

		playerName = GUI.TextField ( new Rect (posX, 20, width, height), playerName);

		if (GUI.Button (new Rect (posX, posY, width, height), "Create")) {
			manager.matchMaker.ListMatches (0, 20, "", OnMatchListAndCreate);
		}
		posY += 2*height;
		roomName = GUI.TextField ( new Rect (posX, posY, width, height), roomName);
		posY += 2*height;
		if ( GUI.Button (new Rect (posX, posY, width, height), "Join"))  {
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
								sufix = "_"+sufixInt;
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
					Debug.Log ( "matches > 0" );
					foreach ( var match in manager.matches ){
						Debug.Log ( "Match: "+match.name );
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
