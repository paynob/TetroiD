using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class PieceController : NetworkBehaviour {

	public GameObject blockPrefab;
	[SyncVar]
	int blocks = 4;
	public float frequency = 1f;
	const float baseFrequency = 1f;
	Vector2 startPoint = new Vector2 (0, 12);
	string myMsg = "";

	bool freezed,rotated,accelerated,destroyed;

	void Update (){
		if (!isLocalPlayer)
			return;
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			//Move Right
			CmdMove(Vector2.right);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			//Move Left
			CmdMove(Vector2.left);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			//Rotate Piece
			CmdRotate();
		}
		if (Input.GetKey(KeyCode.DownArrow)) {
			//Move Down
			CmdMove(Vector2.down);
		}
	}
	void OnGUI(){
		//GUI.Label (new Rect (10, 100, Screen.width, Screen.height), myMsg);

	}
	//-------------------------------- Commands --------------------------------------
	[Command]
	void CmdRotate (){
		Rotate ();
	}

	[Command]
	void CmdMove ( Vector2 direction){
		if (CanMoveTo(direction))
			transform.Translate (direction, Space.World);
	}
	//------------------------------- End of Commands -------------------------------

	//------------------------------- RPC -------------------------------------------
	[ClientRpc]
	void RpcStopPiece(){
		StopPiece ();
	}
	//------------------------------- End of RPC ------------------------------------

	void StopPiece(){
		myMsg += "StopPiece called on server("+isServer+")\n";
		int totalBlocks = transform.childCount;
		for (int i=0; i<totalBlocks; i++) {
			transform.GetChild(i).tag = "Wall";
			transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Tetroid");  // 0-31
			transform.GetChild(i).name = "Block";
		}
		transform.DetachChildren ();
		if (isServer || (!GameManager.singleton.online && playerControllerId==0)) {
			UpdateLines ();
			Respawn();
		}
	}

	bool CanMoveTo( Vector2 direction){
		int totalBlocks = transform.childCount;
		Vector2 positionToCheck;
		Collider2D[] colliders;
		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID;                    // 2^layerID
		for (int i=0; i<totalBlocks; i++) {
			positionToCheck = new Vector2 ( transform.GetChild(i).transform.position.x + direction.x, transform.GetChild(i).transform.position.y + direction.y);
			colliders = Physics2D.OverlapPointAll( positionToCheck, layerMask );
			foreach (var col in colliders) 
				if ( col.gameObject.tag != "Block")
					return false;
		}

		return true;
	}


	//---------------------------------------------------------------
	// SERVER SIDE FUNCTIONS
	//---------------------------------------------------------------
	IEnumerator _OnRoundStart(int startTime){
		myMsg+= ("Se ejecuta en tetris\n");
		//if ( !isServer )
		if (!isServer && !(!GameManager.singleton.online && playerControllerId==0)) 
			yield break;

		Invoke ("Respawn", startTime);
		yield return new WaitForSeconds (startTime);
		StartCoroutine("PieceGoDown");//InvokeRepeating ("PieceGoDown",startTime, frequency);
	}
	
	IEnumerator PieceGoDown(){
		if (!isServer)
			yield break;
		yield return new WaitForSeconds( 1/Time.timeScale );
		freezed = false;
		rotated = false;
		destroyed = false;
		accelerated = false;
		if (CanMoveTo (Vector2.down)) {
			transform.Translate(Vector2.down, Space.World);
		} else {//Cant go down, so stop piece and respawn new one
			RpcStopPiece ();
			float max = -1000;
			foreach ( Transform child in transform ){
				if ( child.position.y > max )
					max = child.position.y;
			}
			if ( max > GameManager.singleton.arkanoidY - 2.5f )
				GameManager.singleton.arkanoidY = max+2.5f;
		}
		StartCoroutine("PieceGoDown");
	}

	void UpdateLines(){
		if (!isServer)
			return;
		int lineZero = GameManager.singleton.lineZero;
		int colZero = GameManager.singleton.columnZero;
		Collider2D[] colliders = new Collider2D[20];
		int walls;
		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID;  
		int i = lineZero;
		while( i<=-lineZero) {
			walls = 0;
			walls = Physics2D.OverlapAreaNonAlloc(new Vector2(colZero,i-0.25f),new Vector2(-colZero,i+0.25f),colliders,layerMask);
			if (walls == (-colZero)*2+1 ){// Line completed
				GameManager.singleton.rowsCompleted ++;
				GameManager.singleton.arkanoidY -= 1;
				frequency += 0.01f;
				Time.timeScale = frequency;
				for ( int a=0; a<walls; a++){
					NetworkServer.Destroy(colliders[a].gameObject);
				}
				for(int j=i+1; j<=-lineZero; j++){
					walls = Physics2D.OverlapAreaNonAlloc(new Vector2(colZero,j-0.25f),new Vector2(-colZero,j+0.25f),colliders,layerMask);
					for ( int a=0; a<walls; a++){
						colliders[a].transform.Translate(Vector2.down, Space.World);
					}
				}
			}else{
				i++;
			}
		}
	}

	public void Accelerate (){
		if (!isServer)
			return;
		if (accelerated)
			return;
		accelerated = true;
		GameManager.singleton.pieceAccelerated++;
		float newScale = Time.timeScale * 3;
		if (newScale > 100)
			newScale = 100;
		Time.timeScale = newScale;
		myMsg += ("Accelerate Piece");
	}


	public void Rotate (){
		if (!isServer)
			return;
		if (rotated)
			return;

		//Rotate the piece
		transform.Rotate (0, 0, 90, Space.World);

		//Check if it's in a legal position
		int childsNum = transform.childCount;
		Vector3 pos;
		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID; 
		Collider2D[] colliders;
		bool canRotate = true;

		for (int i=0; i<childsNum; i++) {
			pos = transform.GetChild(i).transform.position;
			colliders = Physics2D.OverlapPointAll( pos, layerMask);
			foreach ( var col in colliders){
				if ( col.tag != "Block" )
					canRotate = false;
			}
		}
		GameManager.singleton.pieceRotated ++;

		if (!canRotate) {//if can not rotate, then rotate back to the original rotation
			transform.Rotate (0, 0, -90, Space.World);
			rotated = true;
		}
		myMsg += ("Rotate Piece\n");
	}

	public void Freeze (){
		if (!isServer)
			return;
		if (transform.position.y > -GameManager.singleton.lineZero )
			return;
		if (freezed)
			return;
		freezed = true;
		RpcStopPiece ();
		myMsg +=("Freeze Piece\n");
	}

	public void DestroyBlock (GameObject block){
		if (!isServer)
			return;
		if (destroyed)
			return;
		destroyed = true;
		StartCoroutine(DestroyBlockDelayed(block));
		myMsg += ("Destroy Block\n");
	}

	IEnumerator DestroyBlockDelayed(GameObject block) {
		yield return new WaitForSeconds(Time.fixedDeltaTime);
		NetworkServer.Destroy (block);	//Destroy the block
		blocks--;
		if (blocks == 0)
			RpcStopPiece ();
	}

	void Respawn (){
		myMsg += "Respawn\n";
		if (!isServer)
			return;

		Time.timeScale = frequency;
		transform.position = startPoint;
		this.blocks = 4;

		//-------------------------Check if can spawn a new piece--------------------------------------
		Collider2D[] colliders = new Collider2D[10];
		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID;  
		int walls;
		walls = Physics2D.OverlapAreaNonAlloc(new Vector2(startPoint.x,startPoint.y-1),new Vector2(startPoint.x+1,startPoint.y+2),colliders,layerMask);
		if (walls > 0) { //End of round
			GameManager.singleton.roundOver = true;
			string msg2 = "Bloquean el respawn:\n";
			for (int a=0; a<walls; a++){
				msg2 += "\t"+colliders[a].gameObject.name +"\n";
			}
			Debug.Log ( msg2 );
			return;
		}
		//------------------------End of check if can spawn a new piece --------------------------------

		//-------Sends a msg to the BallMove to change the material of the ball-------------
		MyNetworkManager.PieceChangeMessage msg = new MyNetworkManager.PieceChangeMessage();
		msg.ball = GameManager.RandomBall ();
		NetworkServer.SendToAll(MyNetworkManager.MyMsgType.PieceChange, msg);
		//-------End of the message send to BallMove ---------------------------------------
		
		GameManager.PieceType piece = GameManager.RandomPiece ();
		GameObject[] blocks = new GameObject[4];
		for (int i=0; i<4; i++) {
			blocks[i] = Instantiate(blockPrefab);
			blocks[i].transform.parent = transform;
			blocks[i].name = ""+(char)('a'+i);
			blocks[i].tag = "Block";
		}
		//Repositioning each block[i]
		switch(piece){
		case GameManager.PieceType.L:
			blocks[0].transform.localPosition = new Vector2 ( 0 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 0 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 1 , -1 );
			break;
		case GameManager.PieceType.J:
			blocks[0].transform.localPosition = new Vector2 ( 1 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 1 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 1 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 0 , -1 );
			break;
		case GameManager.PieceType.T:
			blocks[0].transform.localPosition = new Vector2 ( 0 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 0 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 1 , 0 );
			break;
		case GameManager.PieceType.Z:
			blocks[0].transform.localPosition = new Vector2 ( 1 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 0 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 1 , 0 );
			break;
		case GameManager.PieceType.S:
			blocks[0].transform.localPosition = new Vector2 ( 0 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 1 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 1 , 0 );
			break;
		case GameManager.PieceType.O:
			blocks[0].transform.localPosition = new Vector2 ( 0 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 1 , 0 );
			blocks[3].transform.localPosition = new Vector2 ( 1 , 1 );
			break;
		case GameManager.PieceType.I:
			blocks[0].transform.localPosition = new Vector2 ( 0 , 1 );
			blocks[1].transform.localPosition = new Vector2 ( 0 , 0 );
			blocks[2].transform.localPosition = new Vector2 ( 0 , -1 );
			blocks[3].transform.localPosition = new Vector2 ( 0 , 2 );
			break;
		}
		//if (GameManager.singleton.online) {
		//	NetworkServer.SpawnWithClientAuthority (blocks [0], connectionToClient);
		//	NetworkServer.SpawnWithClientAuthority (blocks [1], connectionToClient);
		//	NetworkServer.SpawnWithClientAuthority (blocks [2], connectionToClient);
		//	NetworkServer.SpawnWithClientAuthority (blocks [3], connectionToClient);
		//} else {
			NetworkServer.Spawn(blocks[0]);
			NetworkServer.Spawn(blocks[1]);
			NetworkServer.Spawn(blocks[2]);
			NetworkServer.Spawn(blocks[3]);
		//}
	}


}
