using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Tetroid {
    public class PieceController : MonoBehaviour
    {

        [SerializeField]
        private BlockManager blockPrefab = null;
        [SerializeField]
        private Transform startPointTransform = null, tetrisStaticBlocks = null;

        int blocks = 4;
        public float frequency = 1f;
        const float baseFrequency = 1f;
        Vector2 startPoint;
        Collider2D[] collidersNonAlloc = new Collider2D[20];
        
        bool freezed, rotated, accelerated, destroyed;

        private void Awake()
        {
            startPoint = startPointTransform.position;
        }
        private void Start()
        {
            StartCoroutine( PieceGoDown() );
        }

        public void Move( Vector2 direction )
        {
            if( CanMoveTo( direction ) )
                transform.Translate( direction, Space.World );
        }

        public void StopPiece()
        {

            while ( transform.childCount > 0 )
            {
                Transform child = transform.GetChild( 0 );
                child.tag = "Wall";
                child.gameObject.layer = LayerMask.NameToLayer( "Tetroid" );  // 0-31
                child.name = "Block";
                child.SetParent( tetrisStaticBlocks );
                Destroy( child.GetComponent<BlockManager>() );
            }

            StartCoroutine( UpdateLines() );
        }

        bool CanMoveTo( Vector2 direction )
        {
            int totalBlocks = transform.childCount;
            Vector2 positionToCheck;
            Collider2D[] colliders;
            int layerID = LayerMask.NameToLayer( "Tetroid" );  // 0-31
            int layerMask = 1 << layerID;                    // 2^layerID
            for( int i = 0; i < totalBlocks; i++ )
            {
                positionToCheck = new Vector2( transform.GetChild( i ).transform.position.x + direction.x, transform.GetChild( i ).transform.position.y + direction.y );
                colliders = Physics2D.OverlapPointAll( positionToCheck, layerMask );
                foreach( var col in colliders )
                    if( col.gameObject.tag != "Block" )
                        return false;
            }

            return true;
        }

        IEnumerator PieceGoDown()
        {
            while( true )
            {
                yield return new WaitForSeconds( 1 / Time.timeScale );

                freezed = false;
                rotated = false;
                destroyed = false;
                accelerated = false;

                if( CanMoveTo( Vector2.down ) )
                {
                    transform.Translate( Vector2.down, Space.World );
                } 
                else
                {
                    //Cant go down, so stop piece and respawn new one
                    StopPiece();
                    GameManager.Instance.PiecePlaced();
                    for( int i = 0; i < blocks; i++ )
                        GameManager.Instance.BlockPlaced();
                }
            }
        }

        IEnumerator UpdateLines()
        {
            Dictionary<int, List<Transform>> lines = new Dictionary<int, List<Transform>>();
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach ( Transform child in tetrisStaticBlocks)
            {
                if( !lines.ContainsKey( (int)child.position.y ) )
                    lines[(int)child.position.y] = new List<Transform>();
                lines[(int)child.position.y].Add( child );

                if( (int)child.position.y > max )
                    max = (int)child.position.y;
                if( (int)child.position.y < min )
                    min = (int)child.position.y;
            }

            for( int i=min; i<= max; i++ )
            {
                if ( lines.ContainsKey(i) && lines[i].Count == 12 )
                {
                    foreach( var t in lines[i] )
                    {
                        Destroy( t.gameObject );
                    }

                    GameManager.Instance.RowsCompleted();

                    yield return new WaitForSeconds( 0.1f );

                    for( int j=i+1; j<= max; j++ )
                    {
                        if( !lines.ContainsKey( j ) )
                            continue;

                        foreach(var t in lines[j] )
                        {
                            t.Translate( Vector2.down, Space.World );
                        }
                        yield return new WaitForSeconds( 0.1f );
                    }
                    yield return new WaitForSeconds( 0.2f );
                }
            }

            GameManager.Instance.SetNewValues();
        }

        public void Accelerate()
        {
            if( accelerated )
                return;
            accelerated = true;

            GameManager.Instance.PieceAccelerated();

            Time.timeScale = Mathf.Clamp( Time.timeScale * 3f, 1, 100 );
        }

        public bool Rotate()
        {
            //Rotate the piece
            transform.Rotate( 0, 0, 90, Space.World );

            //Check if it's in a legal position
            int layerID = LayerMask.NameToLayer( "Tetroid" );  // 0-31
            int layerMask = 1 << layerID;
            bool canRotate = true;

            foreach (Transform child in transform)
            {
                if( 0 < Physics2D.OverlapBoxNonAlloc( child.position, child.lossyScale * 0.95f, 0, collidersNonAlloc, layerMask ) )
                {
                    canRotate = false;
                    break;
                }
            }

            if( !canRotate )
            {//if can not rotate, then rotate back to the original rotation
                transform.Rotate( 0, 0, -90, Space.World );
                rotated = true;
            }

            return canRotate;
        }

        public void Freeze()
        {
            if( freezed )
                return;
            freezed = true;
            GameManager.Instance.PieceFreezed();
            StopPiece();
        }

        public void DestroyBlock( BlockManager block )
        {
            if( destroyed )
                return;
            destroyed = true;
            StartCoroutine( DestroyBlockDelayed( block ) );
        }

        IEnumerator DestroyBlockDelayed( BlockManager block )
        {
            yield return new WaitForSeconds( Time.fixedDeltaTime );
            block.Destroy();   //Destroy the block
            GameManager.Instance.BlockDestroyed();
            blocks--;
            if( blocks == 0 )
            {
                GameManager.Instance.PieceDestroyed();
                StopPiece();
            }
        }

        public bool Spawn( GameManager.PieceType piece)
        {
            Time.timeScale = GameManager.Instance.TimeScale;
            transform.position = startPoint;
            this.blocks = 4;

            //------------------------- Check if can spawn a new piece --------------------------------------
            int layerID = LayerMask.NameToLayer( "Tetroid" );  // 0-31
            int layerMask = 1 << layerID;
            
            if( Physics2D.OverlapArea( new Vector2( startPoint.x, startPoint.y - 1 ), new Vector2( startPoint.x + 1, startPoint.y + 2 ), layerMask ) != null )
            { // Can not spawn. End of round
                return false;
            }
            //------------------------ End of check if can spawn a new piece --------------------------------

            BlockManager[] blocks = new BlockManager[4];
            for( int i = 0; i < 4; i++ )
            {
                blocks[i] = Instantiate( blockPrefab );
                blocks[i].Initialize( this, piece );
                blocks[i].name = "" + (char)('a' + i);
            }
            //Repositioning each block[i]
            switch( piece )
            {
                case GameManager.PieceType.L:
                    blocks[0].transform.localPosition = new Vector2( 0, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 0, -1 );
                    blocks[3].transform.localPosition = new Vector2( 1, -1 );
                    break;
                case GameManager.PieceType.J:
                    blocks[0].transform.localPosition = new Vector2( 1, 1 );
                    blocks[1].transform.localPosition = new Vector2( 1, 0 );
                    blocks[2].transform.localPosition = new Vector2( 1, -1 );
                    blocks[3].transform.localPosition = new Vector2( 0, -1 );
                    break;
                case GameManager.PieceType.T:
                    blocks[0].transform.localPosition = new Vector2( 0, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 0, -1 );
                    blocks[3].transform.localPosition = new Vector2( 1, 0 );
                    break;
                case GameManager.PieceType.Z:
                    blocks[0].transform.localPosition = new Vector2( 1, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 0, -1 );
                    blocks[3].transform.localPosition = new Vector2( 1, 0 );
                    break;
                case GameManager.PieceType.S:
                    blocks[0].transform.localPosition = new Vector2( 0, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 1, -1 );
                    blocks[3].transform.localPosition = new Vector2( 1, 0 );
                    break;
                case GameManager.PieceType.O:
                    blocks[0].transform.localPosition = new Vector2( 0, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 1, 0 );
                    blocks[3].transform.localPosition = new Vector2( 1, 1 );
                    break;
                case GameManager.PieceType.I:
                    blocks[0].transform.localPosition = new Vector2( 0, 1 );
                    blocks[1].transform.localPosition = new Vector2( 0, 0 );
                    blocks[2].transform.localPosition = new Vector2( 0, -1 );
                    blocks[3].transform.localPosition = new Vector2( 0, 2 );
                    break;
            }

            return true;
        }
    }
}
