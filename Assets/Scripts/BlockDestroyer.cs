using UnityEngine;

namespace Tetroid
{
    [RequireComponent( typeof( Animator ) )]
    public class BlockDestroyer : MonoBehaviour
    {
        private Animator m_Animator;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        public void StartDestroyAnimation()
        {
            m_Animator.SetTrigger( "Destroy" );
        }

        public void DestroyGameObject()
        {
            Destroy( gameObject );
        }
    }

}