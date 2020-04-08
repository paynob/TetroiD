using UnityEngine;
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

        public void BlockHitted() { blockHitted++; UpdateHUDPoints(); }
        public void BlockDestroyed() { blockDestroyed++; UpdateHUDPoints(); }
        public void PieceDestroyed() { pieceDestroyed++; UpdateHUDPoints(); }
        public void PieceAccelerated() { pieceAccelerated++; UpdateHUDPoints(); }
        public void PieceRotated() { pieceRotated++; UpdateHUDPoints(); }
        public void PieceRotatedSuccessfully() { pieceRotatedSuccessfully++; UpdateHUDPoints(); }
        public void PieceFreezed() { pieceFreezed++; UpdateHUDPoints(); }
        public void RowsCompleted() { rowsCompleted++; UpdateHUDPoints(); }
        public void PiecePlaced() { piecePlaced++; UpdateHUDPoints(); }
        public void BlockPlaced() { blockPlaced++; UpdateHUDPoints(); }
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
        private PieceType pieceType;

        //public int p1Points, p2Points;
        

        private IEnumerator Start()
        {
            Initialize();
            yield return new WaitForSeconds( 4 ); // Duration of the Animation
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
            TimeScale = 1f;

            Time.timeScale = TimeScale;
            UpdateHUDPoints();
            RandomBall();
            RandomPiece();
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
    }
}
