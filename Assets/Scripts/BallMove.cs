using UnityEngine;

namespace Tetroid
{
    [RequireComponent(typeof(Rigidbody2D), typeof(ParticleSystem), typeof(Renderer))]
    public class BallMove : MonoBehaviour
    {
        [SerializeField]
        private Material destroy = null, freeze = null, rotate = null, accelerate = null;
        [SerializeField]
        float speed = 12f;


        Vector2 velocity = Vector2.zero;
        float magnitude;
        Rigidbody2D rb2d;

        ParticleSystem.MainModule particleSystemMainModule;
        Renderer m_Renderer;
        /// <summary>
        /// Type of the Ball
        /// - Accelerate
        /// - Destroy
        /// - Freeze
        /// - Rotate
        /// </summary>
        public GameManager.BallType BallType { get; private set; }

        void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            particleSystemMainModule = GetComponent<ParticleSystem>().main;
            m_Renderer = GetComponent<Renderer>();

        }

        private void Start()
        {
            if( velocity == Vector2.zero )
            {
                velocity = new Vector2( -speed, -speed );
                magnitude = velocity.magnitude;
                rb2d.velocity = velocity;
            }
        }

        public void ChangeBall( GameManager.BallType ballType )
        {
            //this.BallType = this.BallType == GameManager.BallType.Destroy ? GameManager.BallType.Freeze : GameManager.BallType.Destroy;
            this.BallType = ballType;

            switch( this.BallType )
            {
                case GameManager.BallType.Accelerate:
                    m_Renderer.material = accelerate;
                    particleSystemMainModule.startColor = accelerate.color;
                    break;
                case GameManager.BallType.Destroy:
                    m_Renderer.material = destroy;
                    particleSystemMainModule.startColor = destroy.color;
                    break;
                case GameManager.BallType.Freeze:
                    m_Renderer.material = freeze;
                    particleSystemMainModule.startColor = freeze.color;
                    break;
                case GameManager.BallType.Rotate:
                    m_Renderer.material = rotate;
                    particleSystemMainModule.startColor = rotate.color;
                    break;
            }
        }

        void OnCollisionEnter2D( Collision2D coll )
        {
            //Debug.LogWarning( coll.GetContact( 0 ).normal );
            // Reflect velocity in the direction of the collision surface normal
            velocity +=  coll.GetContact(0).normal * 2f * speed;

            if( velocity.x > 0 )
                velocity.x = speed;
            else
                velocity.x = -speed;

            if( velocity.y > 0 )
                velocity.y = speed;
            else
                velocity.y = -speed;

            if ( velocity == -rb2d.velocity )
            {
                foreach( var c in coll.contacts )
                    Debug.LogError( $"Contact : N.{c.normal} , V.{c.relativeVelocity}" );

            }
            // Apply velocity to rigidbody
            rb2d.velocity = velocity;
        }

        private void OnTriggerEnter2D( Collider2D collision )
        {
            if ( collision.CompareTag("Finish") )
                GameManager.Instance.DropArkanoidLife();
        }
    }
}