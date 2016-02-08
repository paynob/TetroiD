using UnityEngine;
using System.Collections;

public class TestBall : MonoBehaviour {

	float speed = 600f;
	Vector2 velocity = Vector2.zero;
	Vector2 v2arkanoid;
	Vector3 lastPos;
	Rigidbody2D rb2d;

	Collider2D[] colliders;
	public GameObject piece;
	public GameObject arkanoid;
	bool corner = false;
	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();
		velocity = new Vector2 (speed * Time.fixedDeltaTime, -speed * Time.fixedDeltaTime);
		v2arkanoid = arkanoid.transform.position;
		rb2d.velocity = velocity;
		StartCoroutine ("CameraZoom");
		StartCoroutine ("GoDown");
		colliders = new Collider2D[8];
	}

	IEnumerator GoDown (){
		Vector2 pos = piece.transform.position;
		pos.y -= 1;
		piece.transform.position = pos;
		yield return new WaitForSeconds(1);
		if (pos.y > 3)
			StartCoroutine ("GoDown");
	}

	void OnCollisionEnter2D (Collision2D coll ){
		if (coll.transform.parent == null)
			return;
		if (coll.transform.parent.gameObject.tag == "Piece") {
			Material mat = piece.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
			mat = Instantiate ( mat );
			mat.color = Color.cyan;
			foreach ( Transform t in piece.transform ){
				t.gameObject.GetComponent<MeshRenderer>().material = mat;
			}
		}

		BallCollision (coll);
	}

	void BallCollision (Collision2D coll){
		Vector2 pointA,pointB;
		pointA = new Vector2 (transform.position.x, transform.position.y);
		pointB = new Vector2 (transform.position.x, transform.position.y);

		pointA.x += velocity.x / speed / Time.fixedDeltaTime;
		pointA.y += velocity.y / speed / Time.fixedDeltaTime;

		pointB.x -= velocity.x / speed / Time.fixedDeltaTime;
		pointB.y += velocity.y / speed / Time.fixedDeltaTime;

		GameObject a = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		GameObject b = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		a.transform.position = pointA;
		b.transform.position = pointB;

		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID; 
		layerID = LayerMask.NameToLayer("Tetris");
		layerMask = layerMask | 1<<layerID;
		int num = Physics2D.OverlapAreaNonAlloc (pointA, pointB, colliders,layerMask);

		Debug.LogError ("");
		//Destroy (a);
		//Destroy (b);
	}

	void Update (){
		v2arkanoid.x = transform.position.x;
		if (v2arkanoid.x < -6)
			v2arkanoid.x = -6;
		if (v2arkanoid.x > 22)
			v2arkanoid.x = 22;
		arkanoid.transform.position = v2arkanoid;
	}
	/*void FixedUpdate (){
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
			int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
			int layerMask = 1 << layerID; 
			layerID = LayerMask.NameToLayer("Tetris");
			layerMask = layerMask | 1<<layerID;


			Vector2 check = transform.position;
			if ( velocity.x > 0 ){
				check.x -= transform.localScale.x;
			}else{
				check.x += transform.localScale.x;
			}

			Collider2D col = Physics2D.OverlapPoint(check,layerMask);
			if ( col != null )
				velocity.x = -velocity.x;

			check = transform.position;
			if ( velocity.y > 0 ){
				check.y -= transform.localScale.y;
			}else{
				check.y += transform.localScale.y;
			}
			col = Physics2D.OverlapPoint(check,layerMask);
			if ( col != null )
				velocity.y = -velocity.y;

			corner = true;
		}
		if (lastPos == transform.position)
			rb2d.velocity = -2 * velocity;
		else
			rb2d.velocity = velocity;
		lastPos = transform.position;
		//rb2d.velocity = velocity;

	}*/

	IEnumerator CameraZoom (){
		Vector3 position = new Vector3 (8, 0, -10);
		for (float i=8; i<30; i+=0.2f) {
			Camera.main.orthographicSize = i;
			Camera.main.transform.position = position;
			position.y -= 0.19090909090909090909090909090908f;
			yield return null;
		}
	}
}
