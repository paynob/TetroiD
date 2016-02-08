using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class MyNetworkManager : NetworkManager {

	public GameObject arkanoidPref,tetrisPref;
	public class MyMsgType {
		public static short RoundStart = MsgType.Highest + 1;
		public static short PieceChange = MsgType.Highest + 2;
	};

	public class RoundMessage : MessageBase
	{
		public int round;
		public int startTime;
	}

	public class PieceChangeMessage : MessageBase
	{
		public GameManager.BallType ball;
		public GameManager.PieceType piece; 
	}

	public override void OnMatchCreate (CreateMatchResponse matchInfo ){
		if (matchInfo.success) {
			base.OnMatchCreate(matchInfo );

			GameObject gameManager = (GameObject)Instantiate( spawnPrefabs[0], Vector3.zero, Quaternion.identity);
			DontDestroyOnLoad(gameManager);
			NetworkServer.Spawn(gameManager);
			//Network.Instantiate(playerPrefab, Vector2.zero, Quaternion.identity, 0);
		}
	}
	public override void OnStartHost(){
		myMsg += "OnStartHost\n";
		myMsg += "Local: "+Application.loadedLevelName+"\n";
		GameObject gameManager = (GameObject)Instantiate( spawnPrefabs[0], Vector3.zero, Quaternion.identity);
		//DontDestroyOnLoad(gameManager);
		GameManager.singleton.online = false;
		base.OnStartHost ();
	}

	public void OnStartClient(NetworkClient client){
		myMsg += "OnStartClient\n";
		base.OnStartClient (client);
	}
	public override void OnStartServer ()
	{
		myMsg += "OnStartServer\n";
		base.OnStartServer ();
	}
	public override void OnStopClient(){
		myMsg += "OnStopClient\n";
		base.OnStopClient ();
		myMsg += "\t\tActivatedEnabled: " + isActiveAndEnabled + "\n\t\tClient Connected:" + IsClientConnected() + "\n\t\tNetwork Active:" + isNetworkActive+"\n";
	}
	public override void OnStopServer ()
	{
		myMsg += "OnStopServer\n";
		base.OnStopServer ();
		myMsg += "\t\tActivatedEnabled: " + isActiveAndEnabled + "\n\t\tClient Connected:" + IsClientConnected() + "\n\t\tNetwork Active:" + isNetworkActive+"\n";
	}
	public override void OnStopHost(){
		myMsg += "OnStopHost\n";
		foreach (var client in Network.connections) {
			myMsg += "\t"+client.guid+"\n";
		}
		base.OnStopHost ();
		myMsg += "\t\tActivatedEnabled: " + isActiveAndEnabled + "\n\t\tClient Connected:" + IsClientConnected() + "\n\t\tNetwork Active:" + isNetworkActive+"\n";
	}

	public override void OnServerSceneChanged (string sceneName){myMsg += "Server: "+Application.loadedLevelName+"\n";}
	public override void OnClientSceneChanged (
		NetworkConnection conn){myMsg += "Client: "+Application.loadedLevelName+"\n---------------------------------------\n";
		foreach (var client in Network.connections) {
			myMsg += "\t"+client.guid+"\n";
		}
	}

	string myMsg = "-------------------------------------------\n";
	
	// called when a new player is added for a client
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		myMsg += "\t\t\tOnServerAddPlayer1: " + conn.connectionId + "\n";
		GameObject player;
		float arkanoidY = -7.5f;
		if (GameManager.singleton.online) {
			if (conn.connectionId > -1)
				player = (GameObject)GameObject.Instantiate (arkanoidPref, new Vector2 (0, arkanoidY), Quaternion.identity);
			else
				player = (GameObject)GameObject.Instantiate (tetrisPref, new Vector2 (0, 12), Quaternion.identity);
			NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
		} else {
			player = (GameObject)GameObject.Instantiate (arkanoidPref, new Vector2 (0, arkanoidY), Quaternion.identity);
			GameObject player2 = (GameObject)GameObject.Instantiate (tetrisPref, new Vector2 (0, 12), Quaternion.identity);
			NetworkServer.AddPlayerForConnection (conn, player2, playerControllerId);
			playerControllerId++;
			NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
		}
		GameManager.singleton.arkanoidY = arkanoidY;
		myMsg += "\t\t\tOnServerAddPlayer2: " + conn.connectionId + "\n";
	}

	void OnGUI (){
		//GUILayout.Label (myMsg);
	}

	// called when connected to a server
	public override void OnClientConnect(NetworkConnection conn)
	{
		myMsg += "\t\t\tOnClienConnect1" + conn.connectionId + "\n";
		ClientScene.Ready (conn);
		ClientScene.AddPlayer (0);
		myMsg += "\t\t\tOnClienConnect2" + conn.connectionId + "\n";
	}
}
