using UnityEngine;
using UnityEngine.Networking;

public class BallMove : NetworkBehaviour {


	float speed = 600f;
	Vector2 velocity = Vector2.zero;
	Vector3 lastPos;
	Rigidbody2D rb2d;
	NetworkClient myClient;
	bool corner = false;
	public Material destroy,freeze,rotate,accelerate;

	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();
	
		myClient = NetworkClient.allClients [0];//host
		myClient.RegisterHandler (MyNetworkManager.MyMsgType.PieceChange, OnPieceChange);
	}
	
	public void OnPieceChange(NetworkMessage netMsg){
		MyNetworkManager.PieceChangeMessage msg = netMsg.ReadMessage<MyNetworkManager.PieceChangeMessage>();
		//Debug.Log ("Ball Type: " + msg.ball);
		switch (msg.ball) {
		case GameManager.BallType.Accelerate:
			GetComponent<MeshRenderer> ().material = accelerate;
			GetComponent<ParticleSystem>().startColor = accelerate.color;
			break;
		case GameManager.BallType.Destroy:
			GetComponent<MeshRenderer> ().material = destroy;
			GetComponent<ParticleSystem>().startColor = destroy.color;
			break;
		case GameManager.BallType.Freeze:
			GetComponent<MeshRenderer> ().material = freeze;
			GetComponent<ParticleSystem>().startColor = freeze.color;
			break;
		case GameManager.BallType.Rotate:
			GetComponent<MeshRenderer> ().material = rotate;
			GetComponent<ParticleSystem>().startColor = rotate.color;
			break;
		}

		if ( velocity == Vector2.zero )
			velocity = new Vector2 (-speed * Time.fixedDeltaTime, -speed * Time.fixedDeltaTime);
	}
	void FixedUpdate (){
		if (!isServer)
			return;

		if (corner) {
			velocity = -velocity;
			corner = false;
		}
		if (rb2d.velocity.x == 0) {
			velocity.x = -velocity.x;
		}
		if (rb2d.velocity.y == 0) {
			velocity.y = -velocity.y;
		}
		
		if (rb2d.velocity.x == 0 && rb2d.velocity.y == 0) {
			int layerID = LayerMask.NameToLayer ("Tetroid");  // 0-31
			int layerMask = 1 << layerID; 
			layerID = LayerMask.NameToLayer ("Tetris");
			layerMask = layerMask | 1 << layerID;
			
			
			Vector2 check = transform.position;
			if (velocity.x > 0) {
				check.x -= transform.localScale.x;
			} else {
				check.x += transform.localScale.x;
			}
			
			Collider2D col = Physics2D.OverlapPoint (check, layerMask);
			if (col != null)
				velocity.x = -velocity.x;
			
			check = transform.position;
			if (velocity.y > 0) {
				check.y -= transform.localScale.y;
			} else {
				check.y += transform.localScale.y;
			}
			col = Physics2D.OverlapPoint (check, layerMask);
			if (col != null)
				velocity.y = -velocity.y;
			
			corner = true;
		}
		if (lastPos == transform.position)
			rb2d.velocity = -2 * velocity;
		else
			rb2d.velocity = velocity;
		lastPos = transform.position;

	}
}
