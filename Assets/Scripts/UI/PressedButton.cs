using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tetroid.UI
{
    [RequireComponent( typeof( Graphic ) )]
    public class PressedButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField]
        private Color m_NormalTint = Color.white, m_PressedTint = Color.HSVToRGB( 0, 0, 0.78f );
        [SerializeField]
        private UnityEvent m_OnPress = null;

        bool m_Pressed = false;
        Graphic m_Graphic;

        void Awake()
        {
            m_Graphic = GetComponent<Graphic>();
            m_Graphic.color = m_NormalTint;
        }

        void IPointerDownHandler.OnPointerDown( PointerEventData eventData )
        {
            m_Pressed = true;
            m_Graphic.color = m_PressedTint;
        }

        void IPointerUpHandler.OnPointerUp( PointerEventData eventData )
        {
            m_Pressed = false;
            m_Graphic.color = m_NormalTint;
        }

        void Update()
        {
            if( m_Pressed )
                m_OnPress?.Invoke();
        }
    }
}
