using UnityEngine;

namespace Tetroid
{
    [RequireComponent(typeof(Rigidbody2D), typeof(ParticleSystem), typeof(MeshRenderer))]
    public class BallMove : MonoBehaviour
    {
        [SerializeField]
        private Material destroy = null, freeze = null, rotate = null, accelerate = null;
        [SerializeField]
        float speed = 12f;


        Vector2 velocity = Vector2.zero;
        float magnitude;
        Rigidbody2D rb2d;
        bool corner = false;

        ParticleSystem.MainModule particleSystemMainModule;
        MeshRenderer meshRenderer;
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
            meshRenderer = GetComponent<MeshRenderer>();

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
            this.BallType = ballType;

            switch( this.BallType )
            {
                case GameManager.BallType.Accelerate:
                    meshRenderer.material = accelerate;
                    particleSystemMainModule.startColor = accelerate.color;
                    break;
                case GameManager.BallType.Destroy:
                    meshRenderer.material = destroy;
                    particleSystemMainModule.startColor = destroy.color;
                    break;
                case GameManager.BallType.Freeze:
                    meshRenderer.material = freeze;
                    particleSystemMainModule.startColor = freeze.color;
                    break;
                case GameManager.BallType.Rotate:
                    meshRenderer.material = rotate;
                    particleSystemMainModule.startColor = rotate.color;
                    break;
            }
        }

        void OnCollisionEnter2D( Collision2D coll )
        {
            // Reflect velocity in the direction of the collision surface normal
            velocity = Vector2.ClampMagnitude( Vector2.Reflect( velocity, coll.GetContact(0).normal ), magnitude );

            if( velocity.x > 0 )
                velocity.x = speed;
            else
                velocity.x = -speed;

            if( velocity.y > 0 )
                velocity.y = speed;
            else
                velocity.y = -speed;


            // Apply velocity to rigidbody
            rb2d.velocity = velocity;
        }
    }
}