using UnityEngine;
using UnityEngine.Networking;

public class OnlineHUD : NetworkBehaviour {

	public MyNetworkManager manager;
	public GameObject ballPrefab;
	NetworkClient myClient;
	[SyncVar]
	
	int posX,posY,width, height;
	float startRoundTime;
	bool roundStarted = false;
	
	void Start (){
		manager = GameObject.FindWithTag ("NetworkManager").GetComponent<MyNetworkManager> ();
		UpdateNames ();
		//Debug.Log ("Player: " + gameObject.name + "   PCID: " + playerControllerId);
	}

	public override void OnStartLocalPlayer(){
		if (!GameManager.singleton.online && playerControllerId == 1)
			return;
		myClient = NetworkClient.allClients [NetworkClient.allClients.Count - 1];
		myClient.RegisterHandler (MyNetworkManager.MyMsgType.RoundStart, OnRoundStart);
	}

	public void OnRoundStart(NetworkMessage netMsg){
		MyNetworkManager.RoundMessage msg = netMsg.ReadMessage<MyNetworkManager.RoundMessage>();
		startRoundTime = msg.startTime + Time.time;
		CmdSendRoundStart (msg.startTime);
	}

	void UpdateNames () {
		if (GameManager.singleton.online) {
			GameObject pn = GameObject.FindWithTag ("PlayerName");
			if (isServer) {
				GameManager.singleton.p1Name = pn.name;
			} else {
				CmdUpdatePlayer2Name (pn.name);
			}
		} else {
			GameManager.singleton.p1Name = "Tetris";
			GameManager.singleton.p2Name = "Arkanoid";
		}
	}

	[Command]
	void CmdSendRoundStart(float startTime){
		SendMessage ("_OnRoundStart", startTime);
	}
	[Command]
	void CmdUpdatePlayer2Name(string name){
		GameManager.singleton.p2Name = name;
	}
	
	void OnGUI (){
		if (!isLocalPlayer)
			return;
		if (!GameManager.singleton.online && playerControllerId == 1)
			return;
		posX = Screen.width / 5;
		posY = Screen.height / 6;
		width = Screen.width *3/ 5;
		height = Screen.height / 12;
		if (GameManager.MatchReady ()) {
			//Match Ready -> START!!
			if (!roundStarted){
				if (isServer){
					GameManager.RandomPiece ();
					GameManager.RandomBall ();
					StartRound();
				}
			}
			if (GameManager.singleton.roundOver) {
				string msg;
				if (isServer){
					if ( GameManager.singleton.online){
						msg = Languages.Translate ("_LOSE_");
					}else{
						msg = Languages.Translate ("_AWIN_");
					}
				}else{
					msg = Languages.Translate ("_WIN_");
				}
				if ( GUI.Button( new Rect ( Screen.width/4, Screen.height/2-Screen.width/8, Screen.width/2, Screen.width/4), msg ) ){
					//New Round....blablabla
				}
			}else{
				if ( startRoundTime >= Time.time )
					GUI.Button ( new Rect ( Screen.width/4, Screen.height/2-Screen.width/8, Screen.width/2, Screen.width/4), Languages.Translate ("_ROUNDSTARTS_")+"\n"+(int)(startRoundTime-Time.time));
			}
		}else{
			if ( GameManager.singleton.online)
				GUI.Button (new Rect (posX, posY, width, height), Languages.Translate ("_ROOMNAME_")+": " + manager.matchName);
			else
				GUI.Button (new Rect (posX, posY, width, height), Languages.Translate ("_ROOMNAME_")+": "+Languages.Translate ("_LOCAL_"));
			posY += height*2;
			
			GUI.Button (new Rect (posX, posY, width / 2, height), GameManager.singleton.p1Name);
			if ( GameManager.singleton.p2Name != "" )
				GUI.Button (new Rect (posX + width / 2, posY, width / 2, height), GameManager.singleton.p2Name);
			posY += height*5;
			if (GUI.Button (new Rect (posX, posY, width / 2, height), Languages.Translate ("_READY_"))) {
				if (GameManager.singleton.online) {
					if (isServer)
						GameManager.singleton.p1Ready = true;
					else{
						CmdSetPlayer2Ready();
					}
				}else{
					GameManager.singleton.p1Ready = true;
					GameManager.singleton.p2Ready = true;
				}
			}
			if (GUI.Button (new Rect (posX + width / 2, posY, width / 2, height), Languages.Translate ("_BACK_"))) {
				NetworkManager.Shutdown ();
			}
		}
	}
	
	[Command]
	void CmdSetPlayer2Ready(){
		GameManager.singleton.p2Ready = true;
	}
	
	void StartRound (){
		roundStarted = true;
		MyNetworkManager.RoundMessage msg = new MyNetworkManager.RoundMessage();
		msg.round = 1;
		msg.startTime = 5;
		if (GameManager.singleton.online || playerControllerId == 0 ){
				GameObject ball = (GameObject)Instantiate (ballPrefab);
				NetworkServer.Spawn (ball);
		}
		NetworkServer.SendToAll(MyNetworkManager.MyMsgType.RoundStart, msg );
	}
}
