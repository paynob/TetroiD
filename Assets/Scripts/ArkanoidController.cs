using UnityEngine;
using UnityEngine.Networking;

public class ArkanoidController : NetworkBehaviour {

	//[SyncVar]
	float speed = 10f;
	float point;

	void Start (){
		point = -GameManager.singleton.columnZero - 1.5f;
	}

	void _OnRoundStart(int startTime){
		Debug.Log ("Se ejecuta en arkanoid");
		name = GameManager.singleton.p2Name;
	}
	
	void Update (){
		if (!isLocalPlayer)
			return;
		if (!GameManager.singleton.online) {
			if (Input.GetKey (KeyCode.A)){
				CmdMove( -1 );
			}
			if (Input.GetKey (KeyCode.D)){
				CmdMove( 1 );
			}
		} else {
			CmdMove (Input.GetAxis ("Horizontal"));
		}
	}

	[Command]
	void CmdMove( float x ){
		transform.position = new Vector2 (transform.position.x, GameManager.singleton.arkanoidY);
		transform.Translate(Vector2.right * speed * x * Time.deltaTime);

		CheckMargins ();
	}

	void CheckMargins (){
		if (transform.position.x > point)
			transform.position = new Vector2 (point,transform.position.y);
		if (transform.position.x < -point)
			transform.position = new Vector2 (-point,transform.position.y);
	}
}
