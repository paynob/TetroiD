using UnityEngine;
using System.Collections;

public class BallMovementTest : MonoBehaviour {
	Rigidbody2D rb2d;
	public Vector2 velocity;
	public float speed = 300f;
	float baseSpeed;
	public bool pausePerFrame = true;
	bool collisioned;
	Vector2[] normals;
	int n = 0;
	void Start (){
		rb2d = GetComponent<Rigidbody2D> ();
		baseSpeed = speed;
		velocity = new Vector2 (speed , -speed);
		rb2d.velocity = velocity * Time.fixedDeltaTime;

	}
	void FixedUpdate()
	{
		normals = new Vector2[2];
		collisioned = false;
		n++;
		if ( pausePerFrame )
			Debug.LogError ("Frame: "+velocity);

		rb2d.velocity = velocity * Time.fixedDeltaTime * speed / baseSpeed;
	}

	void OnCollisionEnter2D (Collision2D coll){
		Vector2 direction = new Vector2(0,0);
		//GameObject go;
		foreach(ContactPoint2D hit in coll.contacts)
		{
			direction = hit.point - direction;
			//go =	GameObject.CreatePrimitive(PrimitiveType.Sphere);
			//go.transform.position = hit.point;
			//go.transform.localScale *= 0.1f;
		}
		if (collisioned) {
			normals [1] = direction;
			if ( normals[0].magnitude > normals[1].magnitude )
			{
				direction = normals[0];
			}else{
				if ( normals[0].x == 0) 
					velocity.x = -velocity.x;
				if ( normals[0].y == 0)
					velocity.y = -velocity.y;
			}
		}else {
			collisioned = true;
			normals[0] = direction;
		}

		if (direction.x == 0) {
			velocity.x = -velocity.x;
		}
		if (direction.y == 0) {
			velocity.y = -velocity.y;
		}
		//Debug.Log ("Dir: " + direction.normalized);
	}

	void OxnCollisionEnter2D (Collision2D coll){
		int xPositive=0, yPositive=0;
		GameObject go;
		foreach(ContactPoint2D hit in coll.contacts)
		{
			Vector2 hitPoint = hit.point;
			Debug.Log( hitPoint );
			go =	GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.transform.position = hitPoint;
			go.transform.localScale *= 0.1f;
			if (hitPoint.x > transform.position.x)
				xPositive ++;
			else
				xPositive --;
			if (hitPoint.y > transform.position.y)
				yPositive ++;
			else
				yPositive --;
		}

		switch (coll.contacts.Length) {
		case 1:
			float xy = Mathf.Abs ( transform.position.x - coll.transform.position.x);
			xy -= Mathf.Abs ( transform.position.y - coll.transform.position.y);

			if ( xy > 0 ){
				Debug.Log("Case 1.0");
				velocity.x = -velocity.x;
			}else if ( xy < 0){
				Debug.Log("Case 1.1");
				velocity.y = -velocity.y;
			}else{
				Debug.Log("Case 1.2");
				velocity = -velocity;
			}
			break;
		case 2:
			if ( xPositive != 0 ){
				Debug.Log("Case 2.0:  "+xPositive);
				velocity.x = -velocity.x;
			}
			if ( yPositive != 0 ){
				Debug.Log("Case 2.1:  "+yPositive);
				velocity.y = -velocity.y;
			}
			break;
		case 3:
			Debug.Log("Case 3.0");
			velocity = -velocity;
			break;
		default:
			Debug.LogError ("More than 3 contacts");
			break;
		}

	}
	/*
	void OnCollisionEnter2D (Collision2D coll ){
		if (collisioned)
			return;
		collisioned = true;
		Vector2 pointA,pointB;
		pointA = new Vector2 (transform.position.x, transform.position.y);
		pointB = new Vector2 (transform.position.x, transform.position.y);

		pointA.x += (scale+0.01f) * Mathf.Sign (velocity.x);
		pointA.y += (scale+0.01f) * Mathf.Sign (velocity.y);
		pointB.x -= scale * Mathf.Sign (velocity.x);
		pointB.y += scale * Mathf.Sign (velocity.y);

		GameObject a = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		GameObject b = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		a.transform.position = pointA;
		b.transform.position = pointB;
		a.name = "A" + n+coll.gameObject.name;
		b.name = "B" + n+coll.gameObject.name;

		
		int layerID = LayerMask.NameToLayer("Tetroid");  // 0-31
		int layerMask = 1 << layerID; 
		layerID = LayerMask.NameToLayer("Tetris");
		layerMask = layerMask | 1<<layerID;
		int num = Physics2D.OverlapAreaNonAlloc (pointA, pointB, colliders,layerMask);

		pointB = new Vector2 (transform.position.x, transform.position.y);
		pointB.x += scale * Mathf.Sign (velocity.x);
		pointB.y -= scale * Mathf.Sign (velocity.y);
		GameObject c = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		c.transform.position = pointB;
		c.name = "C" + n+coll.gameObject.name;

		Collider2D col1 = colliders[0];
		
		if (num == 2) {
			velocity.y = -velocity.y;
		} else if (num == 1) {
			col1 = colliders [0];
		}

		int num2 = Physics2D.OverlapAreaNonAlloc (pointA, pointB, colliders,layerMask);

		if (num2  == 2) {
				velocity.x = -velocity.x;
		}
		if (num == 1 && num2 == 1) {
			if ( col1 != colliders[0] )
				velocity *= -1;
			else{
				if ( Mathf.Abs(transform.position.x - coll.transform.position.x) > Mathf.Abs(transform.position.y - coll.transform.position.y)){
					if ( coll.transform.localScale.x > 1 )
						velocity.y = -velocity.y;
					else
						velocity.x = -velocity.x;
				}else if ( Mathf.Abs(transform.position.y - coll.transform.position.y) > Mathf.Abs(transform.position.x - coll.transform.position.x)){
					if ( coll.transform.localScale.y > 1 )
						velocity.x = -velocity.x;
					else
						velocity.y = -velocity.y;

				}else{
					velocity *= -1;
				}
			}
		}else if ( num == 1 && num2 == 0){
			velocity.y = -velocity.y;
		}else if ( num == 0 && num2 == 1){
			velocity.x = -velocity.x;
		}
		a.transform.localScale = a.transform.localScale * 0.1f;
		b.transform.localScale = b.transform.localScale * 0.1f;
		c.transform.localScale = c.transform.localScale * 0.1f;
		n++;
	}

	void FixedUpdate (){
		collisioned = false;
		rb2d.velocity = velocity;
		if ( pausePerFrame )
			Debug.LogError ("Frame: "+rb2d.velocity);
		GameObject[] allGameObjects  = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (var go in allGameObjects) {
			if (go.tag=="Untagged") {
				Destroy(go);
			}
		}
	}*/
}
