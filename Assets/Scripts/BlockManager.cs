using UnityEngine;
using UnityEngine.Networking;

public class BlockManager : NetworkBehaviour {

	[SyncVar]
	string myName;

	//----------------------------------
	void Start () {
		//	rb2d = GetComponent<Rigidbody2D> ();
		if (!isLocalPlayer) {
			var parent = GameObject.FindWithTag("Piece");
			if (parent != null )
				transform.SetParent(parent.transform);
			tag = "Block";
			gameObject.layer = LayerMask.NameToLayer("Tetris");  // 0-31
		}
		if ( isServer )
			myName = name;
		else
			name = myName;

		Material mat = null;
		if ( mat == null ){
			mat = GetComponent<MeshRenderer>().material;
			mat = Instantiate ( mat );
			switch(GameManager.singleton.piece){
			case GameManager.PieceType.L:
				mat.color = new Color( 1,0.6f,0);
				break;
			case GameManager.PieceType.J:
				mat.color = Color.blue;
				break;
			case GameManager.PieceType.T:
				mat.color = Color.magenta;
				break;
			case GameManager.PieceType.Z:
				mat.color = Color.red;
				break;
			case GameManager.PieceType.S:
				mat.color = Color.green;
				break;
			case GameManager.PieceType.O:
				mat.color = Color.yellow;
				break;
			case GameManager.PieceType.I:
				mat.color = Color.cyan;
				break;
			}
		}
		GetComponent<MeshRenderer>().material = mat;
	}

	void Update (){
		transform.rotation = Quaternion.identity;
	}
	void OnCollisionEnter2D ( Collision2D coll){
		if (!isServer)
			return;
		if (coll.gameObject.tag == "Ball" && tag=="Block") {
			GameManager.singleton.blockHitted ++;
			switch (GameManager.singleton.ball) {
			case GameManager.BallType.Accelerate:
				SendMessageUpwards ("Accelerate");
				break;
			case GameManager.BallType.Destroy:
				SendMessageUpwards ("DestroyBlock",gameObject);
				break;
			case GameManager.BallType.Freeze:
				SendMessageUpwards ("Freeze");
				break;
			case GameManager.BallType.Rotate:
				SendMessageUpwards ("Rotate");
				break;
			}
		}
	}
}
