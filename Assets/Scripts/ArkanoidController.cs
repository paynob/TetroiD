using UnityEngine;

namespace Tetroid {
    [RequireComponent( typeof( BoxCollider2D ) )]
    public class ArkanoidController : MonoBehaviour
    {

        [SerializeField]
        float m_Speed = 10f;
        [SerializeField]
        Transform m_LeftLimit = null, m_RightLimit = null, m_StartPosition = null;

        private void Awake()
        {
            transform.position = m_StartPosition.position;
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveLeft() { Move( -1 ); }
        /// <summary>
        /// 
        /// </summary>
        public void MoveRight() { Move( 1 ); }

        private void Move( int x )
        {
            transform.Translate( Vector2.right * m_Speed * x * Time.deltaTime );
            CheckMargins();
        }

        private void CheckMargins()
        {
            if( transform.position.x > m_RightLimit.transform.position.x )
                transform.position = new Vector2( m_RightLimit.transform.position.x, transform.position.y );

            if( transform.position.x < m_LeftLimit.transform.position.x )
                transform.position = new Vector2( m_LeftLimit.transform.position.x, transform.position.y );
        }
    }
}