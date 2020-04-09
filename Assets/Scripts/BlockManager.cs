using UnityEngine;

namespace Tetroid
{
    [RequireComponent(typeof(Renderer))]
    public class BlockManager : MonoBehaviour
    {
        private PieceController pieceController;
        private bool destroying = false;
        private bool inRespawnZone = false;

        public void Initialize(PieceController parent, GameManager.PieceType type )
        {
            if( destroying )
                return;
            this.transform.SetParent( parent.transform );
            this.pieceController = parent;
            this.tag = "Block";
            this.gameObject.layer = LayerMask.NameToLayer( "Tetris" ); // Tetris Layer is only for the piece is moving

            Material mat = GetComponent<Renderer>().material;
            switch( type )
            {
                case GameManager.PieceType.L:
                    mat.color = new Color( 1, 0.6f, 0 );
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

        public void Destroy()
        {
            if( destroying )
                return;
            destroying = true;

            GetComponent<BlockDestroyer>().StartDestroyAnimation();
            Destroy( this );
        }

        void OnCollisionEnter2D( Collision2D coll )
        {
            if( destroying || inRespawnZone)
                return;
            if( coll.gameObject.CompareTag("Ball") )
            {
                GameManager.Instance.BlockHitted();

                BallMove ball = coll.gameObject.GetComponent<BallMove>();
                
                if ( ball != null )
                {
                    switch( ball.BallType )
                    {
                        case GameManager.BallType.Accelerate:
                            pieceController.Accelerate();
                            break;
                        case GameManager.BallType.Destroy:
                            pieceController.DestroyBlock( this );
                            break;
                        case GameManager.BallType.Freeze:
                            pieceController.Freeze();
                            break;
                        case GameManager.BallType.Rotate:
                            if ( pieceController.Rotate() )
                                GameManager.Instance.PieceRotated();
                            break;
                    }
                }
            }
        }

        private void OnTriggerEnter2D( Collider2D collision )
        {
            if( collision.CompareTag( "Respawn" ) )
                inRespawnZone = true;
        }

        private void OnTriggerExit2D( Collider2D collision )
        {
            if( collision.CompareTag( "Respawn" ) )
                inRespawnZone = false;
        }
    }
}
