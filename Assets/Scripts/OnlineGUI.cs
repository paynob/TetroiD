using UnityEngine;
using UnityEngine.Networking;

public class OnlineGUI : NetworkBehaviour {

	public MyNetworkManager manager;
	public GameObject[] prefabs;
	NetworkClient myClient;
	[SyncVar]

	int posX,posY,width, height;

	//bool matchStarted = false;
	bool roundStarted = false;

	void Start (){
		manager = GameObject.FindWithTag ("NetworkManager").GetComponent<MyNetworkManager> ();
		//Invoke ("UpdateNames", 0.2f);
	}

	void OnGUI (){
		if (!isLocalPlayer)
			return;
		posX = Screen.width / 8;
		posY = Screen.height / 6;
		width = Screen.width *3/ 4;
		height = Screen.height / 12;
		GUILayout.Label ("Is Server: " + isServer);
		if (GameManager.MatchReady ()) {
			//Match Ready -> START!!
			if (!roundStarted){
				if (isServer){
					GameManager.RandomPiece ();
					StartRound();
				}
			}
		}else{
			GUI.Button (new Rect (posX, posY, width, height), "Room: " + manager.matchName);
			posY += height*2;
			
			GUI.Button (new Rect (posX, posY, width / 2, height), GameManager.singleton.p1Name);
			if ( GameManager.singleton.p2Name != "" )
				GUI.Button (new Rect (posX + width / 2, posY, width / 2, height), GameManager.singleton.p2Name);
			posY += height*5;
			if (GUI.Button (new Rect (posX, posY, width / 2, height), "Ready")) {
				if (isServer)
					GameManager.singleton.p1Ready = true;
				else{
					CmdSetPlayer2Ready();
				}
			}
			if (GUI.Button (new Rect (posX + width / 2, posY, width / 2, height), "Back")) {
				
			}
		}
	}

	[Command]
	void CmdSetPlayer2Ready(){
		GameManager.singleton.p2Ready = true;
	}

	void StartRound(){
		roundStarted = true;
	}
	/*
	public override void OnStartServer(){
		Debug.Log ("OnStartServer");
	}

	public override void OnStartClient(){
		Debug.Log ("Se ha conectado cliente");
		myClient = NetworkClient.allClients [NetworkClient.allClients.Count - 1];
		myClient.RegisterHandler (MyNetworkManager.MyMsgType.RoundStart, OnRoundStart);
	}

	void OnRoundStart(NetworkMessage netMsg){
		MyNetworkManager.RoundMessage msg = netMsg.ReadMessage<MyNetworkManager.RoundMessage>();
		if (isServer) {
			GameObject childObject = GameObject.FindWithTag ("Arkanoid");
			child = childObject.transform;
		} else {

		}
	}

	void UpdateNames () {
		GameObject pn = GameObject.FindWithTag ("PlayerName");
		if ( isServer ) {
			GameManager.singleton.p1Name = pn.name;
		} else {
			CmdUpdatePlayer2Name( pn.name );
		}
	}

	[Command]
	void CmdUpdateChild(){
		GameObject childObject = GameObject.FindWithTag ("Piece");
		child = childObject.transform;
	}
	[Command]
	void CmdUpdatePlayer2Name(string name){
		GameManager.singleton.p2Name = name;
	}

	void StartRound (){
		roundStarted = true;
		GameObject obj;
		foreach (var pref in prefabs) {
			obj = (GameObject)Instantiate ( pref);
			NetworkServer.Spawn(obj);
		}
		MyNetworkManager.RoundMessage msg = new MyNetworkManager.RoundMessage();
		msg.round = 1;
		msg.startTime = 5;
		NetworkServer.SendToAll(MyNetworkManager.MyMsgType.RoundStart, msg );
	}*/
}
