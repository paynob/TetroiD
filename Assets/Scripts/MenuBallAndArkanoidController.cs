using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tetroid
{
    public class MenuBallAndArkanoidController : MonoBehaviour
    {

        public GameObject piece;
        public GameObject arkanoid;
        public Button button;

        float speed = 12f;
        Vector2 velocity = Vector2.zero;
        Vector2 v2arkanoid;
        Vector3 lastPos;
        Rigidbody2D rb2d;

        public void StartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene( 1 );
        }

        // Use this for initialization
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            velocity = new Vector2( speed, -speed );
            v2arkanoid = arkanoid.transform.position;
            rb2d.velocity = velocity;
            StartCoroutine( "GoDown" );
        }

        IEnumerator GoDown()
        {
            Vector2 pos = piece.transform.position;
            pos.y -= 1;
            piece.transform.position = pos;
            yield return new WaitForSeconds( 1 );
            if( pos.y > 24 )
                StartCoroutine( "GoDown" );
            else
            {
                button.gameObject.SetActive( true );
                yield return ShowButton();
            }
        }

        IEnumerator ShowButton()
        {
            Color c = button.image.color;

            while( c.a < 1 )
            {
                c.a += 0.01f;
                button.image.color = c;
                yield return null;
            }
        }

        void OnCollisionEnter2D( Collision2D coll )
        {

            foreach( var c in coll.contacts )
            {
                velocity = velocity + c.normal * speed * 2f;
                if( velocity.x < 0 )
                    velocity.x = -speed;
                else if( velocity.x > 0 )
                    velocity.x = speed;
                if( velocity.y < 0 )
                    velocity.y = -speed;
                else if( velocity.y > 0 )
                    velocity.y = speed;

                rb2d.velocity = velocity;
            }

            if( coll.transform.parent == null )
                return;

            if( coll.transform.parent.gameObject.tag == "Piece" )
            {
                Material mat = piece.transform.GetChild( 0 ).gameObject.GetComponent<MeshRenderer>().material;
                mat = Instantiate( mat );
                mat.color = Color.cyan;
                foreach( Transform t in piece.transform )
                {
                    t.gameObject.GetComponent<MeshRenderer>().material = mat;
                }
            }
        }

        void Update()
        {
            v2arkanoid.x = Mathf.Clamp( transform.position.x, -15, 15 );

            arkanoid.transform.position = v2arkanoid;
        }
    }
}