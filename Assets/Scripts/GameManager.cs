using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

namespace Tetroid
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {

        [SerializeField]
        private BallMove m_Ball;

        [SerializeField]
        private PieceController m_PieceController;

        [SerializeField]
        private ArkanoidController m_Arkanoid;

        [SerializeField]
        private Transform m_StaticBlocksParent = null;

        [SerializeField]
        private Animator m_Animator = null;

        [SerializeField]
        private GameObject m_ContinueButton = null;

        [SerializeField]
        private Transform m_ArkanoidLifes = null;

        [SerializeField]
        private Button m_ArkanoidSpecialButton = null;

        [SerializeField]
        private TextMeshProUGUI m_ArkanoidPoints = null, m_TetrisPoints = null, m_ArkanoidResult = null, m_TetrisResult = null;

        //--------------Singleton pattern-----------------------
        #region Singleton-Pattern
        private static GameManager _singleton;
        private static bool _destroying = false;

        public static GameManager Instance
        {
            get {
                if( _singleton == null )
                {
                    GameManager[] gms = Resources.FindObjectsOfTypeAll<GameManager>();

                    // Just one GameManager exists in the scene
                    if( gms.Length == 1 )
                    {
                        if( _destroying )
                            return gms[0];
                        else
                            _singleton = gms[0];
                    }
                    // The user has not placed a GameManager in the scene
                    else if( gms.Length == 0 )
                        throw new Exception( "No GameManager was found in the scene." );
                    // The user has placed more than one GameManager in the scene
                    else if( gms.Length > 1 )
                    {
                        foreach( var gm in gms )
                            Debug.Log( $"Found : {gm} child of ({gm.transform.parent})" );
                        throw new Exception( "More than one GameManager was found in the scene." );
                    }
                }
                return _singleton;
            }

            private set {
                if( _singleton != null && value != null && _singleton != value )
                    throw new Exception( "Already One GameManager exists." );
                // Update the _singleton value
                if( _singleton != value )
                    _singleton = value;
            }
        }
        void Awake()
        {
            Instance = this;
            GC.Collect();
        }
        private void OnDestroy()
        {
            if( _singleton == this )
            {
                _destroying = true;
                _singleton = null;
            }
        }
        #endregion Singleton-Pattern
        //--------------End Singleton pattern-----------------------

        private float m_PausedScaleTime;

        public enum BallType { Destroy, Accelerate, Freeze, Rotate };
        public enum PieceType { L, J, T, S, Z, I, O };

        private class Points
        {
            public const int PerRow = 100;
            public const int PerBlockDestroyed = 10;
            public const int PerPieceHitted = 15;
            public const int PerPieceDestroyed = 50;
            public const int PerBlockPlaced = 5;
        }


        //--------------Vars for statistics purpose-----------------------
        private int blockHitted = 0;
        private int blockDestroyed = 0;
        private int pieceDestroyed = 0;
        private int pieceAccelerated = 0;
        private int pieceRotated = 0; //by ball
        private int pieceRotatedSuccessfully = 0;//again only by ball
        private int pieceFreezed = 0;
        private int rowsCompleted = 0;
        private int piecePlaced = 0;
        private int blockPlaced = 0;

        public void BlockHitted() {
            blockHitted++;
            UpdateHUDPoints(); }
        public void BlockDestroyed() {
            blockDestroyed++;
            UpdateHUDPoints();
            UpdateHUDCharge( 0.15f ); }
        public void PieceDestroyed() {
            pieceDestroyed++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( 0.30f ); }
        public void PieceAccelerated() {
            pieceAccelerated++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( 0.25f );}
        public void PieceRotated() {
            pieceRotated++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( 0.15f );}
        public void PieceRotatedSuccessfully() {
            pieceRotatedSuccessfully++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( 0.20f );}
        public void PieceFreezed() {
            pieceFreezed++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( 0.35f );}
        public void RowsCompleted() {
            rowsCompleted++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( -0.5f );}
        public void PiecePlaced() {
            piecePlaced++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( -0.03f );}
        public void BlockPlaced() {
            blockPlaced++;
            UpdateHUDPoints(); 
            UpdateHUDCharge( -0.01f );}
        //--------------End vars for statistics purpose-------------------

        /// <summary>
        /// 
        /// </summary>
        public int TetrisPoints
        {
            get {
                return rowsCompleted * Points.PerRow + blockPlaced * Points.PerBlockPlaced;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ArkanoidPoints
        {
            get {
                return blockHitted * Points.PerPieceHitted + blockDestroyed * Points.PerBlockDestroyed + pieceDestroyed * Points.PerPieceDestroyed;
            }
        }

        public float TimeScale { get; private set; }
        public bool RoundOver { get; private set; }

        private BallType ballType;
        private BallType arkanoidChargeType;
        private PieceType pieceType;
        private SVGImage specialButtonArea;

        //public int p1Points, p2Points;
        

        private IEnumerator Start()
        {
            yield return new WaitForSeconds( 2) ;
            Initialize();
            yield return new WaitForSeconds( 2 ); // Duration of the Animation
            m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            m_Animator.SetBool( "Playing", !RoundOver );
            m_Arkanoid.gameObject.SetActive( !RoundOver );
            m_Ball.gameObject.SetActive( !RoundOver );
            m_PieceController.gameObject.SetActive( !RoundOver );

            SetNewValues();
        }

        // Use this for initialization
        void Initialize()
        {
            TimeScale = 1.1f;

            Time.timeScale = TimeScale;

            m_ArkanoidSpecialButton.interactable = false;
            specialButtonArea = m_ArkanoidSpecialButton.transform.GetChild( 0 ).GetChild( 0 ).GetComponent<SVGImage>();
            specialButtonArea.transform.localScale = Vector3.zero;

            ResetArkanoidLifes();
            UpdateHUDPoints();
            RandomBall();
            RandomPiece();
            RandomCharge();
        }

        private void UpdateHUDCharge(float add)
        {
            float scale = specialButtonArea.transform.localScale.x;

            scale = Mathf.Clamp( scale + add, 0, 0.9f );

            specialButtonArea.transform.localScale = scale * Vector3.one;

            m_ArkanoidSpecialButton.interactable = scale >= 0.9f;
        }

        private void ResetArkanoidLifes()
        {
            foreach (Transform t in m_ArkanoidLifes)
                t.gameObject.SetActive( true );
        }

        public void DropArkanoidLife()
        {
            Transform child = m_ArkanoidLifes.GetChild( 0 );
            if ( child.gameObject.activeSelf )
            {
                child.SetAsLastSibling();
                child.gameObject.SetActive( false );
                m_Ball.transform.position = m_Arkanoid.transform.position + Vector3.down;
            } else
            {
                RoundOver = true;
                m_ContinueButton.SetActive( false );
                m_Ball.gameObject.SetActive( false );
                m_Arkanoid.gameObject.SetActive( false );

                if( ArkanoidPoints > TetrisPoints )
                {
                    m_ArkanoidResult.text = "You Win";
                    m_TetrisResult.text = "You Lose";
                }
                else if( ArkanoidPoints < TetrisPoints )
                {
                    m_ArkanoidResult.text = "You Lose";
                    m_TetrisResult.text = "You Win";
                } else
                {
                    m_ArkanoidResult.text = "You Lose";
                    m_TetrisResult.text = "You Lose";
                }

                m_ArkanoidResult.gameObject.SetActive( true );
                m_TetrisResult.gameObject.SetActive( true );

                foreach( Transform t in m_StaticBlocksParent )
                    Destroy( t.gameObject );
                m_Animator.SetBool( "Playing", !RoundOver );
            }
        }

        public void UseArkanoidCharge()
        {
            m_ArkanoidSpecialButton.interactable = false;
            specialButtonArea.transform.localScale = Vector3.zero;

            switch( arkanoidChargeType )
            {
                case BallType.Destroy:
                    foreach( Transform t in m_PieceController.transform )
                    {
                        if ( UnityEngine.Random.Range(0,2) == 0  )
                            t.GetComponent<BlockManager>().Destroy();
                    }
                    break;
                case BallType.Accelerate:
                    Time.timeScale *= 2;
                    break;
                case BallType.Freeze:
                    m_PieceController.Freeze (4f );
                    break;
                case BallType.Rotate:
                    foreach ( Transform t in m_StaticBlocksParent)
                    {
                        if( UnityEngine.Random.Range( 0, 6 ) == 0 )
                            t.GetComponent<BlockDestroyer>().StartDestroyAnimation();
                    }
                    break;
            }

            RandomCharge();
        }

        private void UpdateHUDPoints()
        {
            m_ArkanoidPoints.text = ArkanoidPoints.ToString();
            m_TetrisPoints.text = TetrisPoints.ToString();
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.A ) )
                MoveTetrisLeft();
            else if( Input.GetKeyDown( KeyCode.D ) )
                MoveTetrisRight();
            else if( Input.GetKeyDown( KeyCode.W ) )
                RotateTetris();
            else if( Input.GetKey( KeyCode.LeftArrow ) )
                m_Arkanoid.MoveLeft();
            else if( Input.GetKey( KeyCode.RightArrow ) )
                m_Arkanoid.MoveRight();
#if UNITY_EDITOR
            else if( Input.GetKeyDown( KeyCode.Escape ) )
                Pause();
            else if ( Input.GetKeyDown( KeyCode.Space) )
                Time.timeScale += 0.1f;
#endif
        }

        public void Pause()
        {
            m_PausedScaleTime = Time.timeScale;
            m_Animator.SetBool( "Playing", false );
            Time.timeScale = 0;
        }

        public void Unpause()
        {
            m_Animator.SetBool( "Playing", true );
            Time.timeScale = m_PausedScaleTime;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene( 1 );
        }

        public void SetNewValues()
        {
            if( !m_PieceController.Spawn( RandomPiece() ) )
            {
                RoundOver = true;
                m_ContinueButton.SetActive( false );
                m_Ball.gameObject.SetActive( false );
                m_Arkanoid.gameObject.SetActive( false );

                if ( ArkanoidPoints > TetrisPoints )
                {
                    m_ArkanoidResult.text = "You Win";
                    m_TetrisResult.text = "You Lose";
                }else if( ArkanoidPoints < TetrisPoints )
                {
                    m_ArkanoidResult.text = "You Lose";
                    m_TetrisResult.text = "You Win";
                } else
                {
                    m_ArkanoidResult.text = "You Lose";
                    m_TetrisResult.text = "You Lose";
                }

                m_ArkanoidResult.gameObject.SetActive( true );
                m_TetrisResult.gameObject.SetActive( true );

                foreach( Transform t in m_StaticBlocksParent )
                    Destroy( t.gameObject );
                m_Animator.SetBool( "Playing", !RoundOver );

                return;
            }

            TimeScale += 0.02f;
            m_Ball.ChangeBall( RandomBall() );
        }

        public void MoveTetrisLeft()
        {
            m_PieceController.Move( Vector2.left );
        }

        public void MoveTetrisRight()
        {
            m_PieceController.Move( Vector2.right );
        }

        public void RotateTetris()
        {
            m_PieceController.Rotate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private BallType RandomBall()
        {
            ballType = (BallType)UnityEngine.Random.Range( 0, Enum.GetValues( typeof( BallType ) ).Length );
            //ballType = BallType.Destroy; // TODO Delete
            return ballType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private PieceType RandomPiece()
        {
            pieceType = (PieceType)UnityEngine.Random.Range( 0, Enum.GetValues( typeof( PieceType ) ).Length );
            return pieceType;
        }

        private BallType RandomCharge()
        {
            arkanoidChargeType = (BallType)UnityEngine.Random.Range( 0, Enum.GetValues( typeof( BallType ) ).Length );
            //arkanoidChargeType = BallType.Freeze; // TODO Delete

            switch( arkanoidChargeType )
            {
                case BallType.Destroy:
                    specialButtonArea.color = Color.red;
                    break;
                case BallType.Accelerate:
                    specialButtonArea.color = Color.green;
                    break;
                case BallType.Freeze:
                    specialButtonArea.color = Color.cyan;
                    break;
                case BallType.Rotate:
                    specialButtonArea.color = Color.yellow;
                    break;
            }
            return arkanoidChargeType;
        }
    }
}
