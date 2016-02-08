using UnityEngine;
using UnityEngine.Networking;
using System;

public class GameManager : NetworkBehaviour {


	public static GameManager singleton;

	//public enum Team { Arkanoid, Tetris};
	public enum BallType { Destroy, Accelerate, Freeze, Rotate };
	public enum PieceType { L,J,T,S,Z,I,O};

	public class Points { 
		public const int PerRow = 100;
		public const int PerBlockDestroyed = 10;
		public const int PerPieceHitted = 15;
		public const int PerPieceDestroyed = 50;
		public const int PerBlockPlaced = 5;
	}
	//--------------Vars for statistics purpose-----------------------
	public int blockHitted = 0;
	public int blockDestroyed = 0;
	public int pieceDestroyed = 0;
	public int pieceAccelerated = 0;
	public int pieceRotated = 0; //by ball
	public int pieceRotatedSuccessfully = 0;//again only by ball
	public int pieceFreezed = 0;
	public int rowsCompleted = 0;
	public int piecePlaced = 0;
	public int blockPlaced = 0;
	//--------------End vars for statistics purpose-------------------

	public int Player1Points (){
		int points = rowsCompleted * Points.PerRow;
		points += blockPlaced * Points.PerBlockPlaced;

		return points;
	}
	public int Player2Points (){
		int points = blockHitted * Points.PerPieceHitted;
		points += blockDestroyed * Points.PerBlockDestroyed;
		points += pieceDestroyed * Points.PerPieceDestroyed;


		return points;
	}

	[SyncVar]
	public BallType ball;
	[SyncVar]
	public BallType nextBall;
	[SyncVar]
	public PieceType piece;
	[SyncVar]
	public PieceType nextPiece;

	[SyncVar]
	public int p1Points,p2Points;
	[SyncVar]
	public float arkanoidY;
	[SyncVar]
	public bool p1Ready;
	[SyncVar]
	public bool p2Ready;
	[SyncVar]
	public bool roundOver;

	[SyncVar]
	public string p1Name="";
	[SyncVar]
	public string p2Name="";

	//public int[,] table;
	//public int rows,columns;

	public int lineZero,columnZero;
	public bool online = true;

	void Start (){
		Initialize ();
	}
	void Awake(){
		Initialize ();
	}
	// Use this for initialization
	void Initialize() {
		lineZero = -10;
		columnZero = -6;
		singleton = this;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public static bool MatchReady (){
		return GameManager.singleton.p1Ready && GameManager.singleton.p2Ready;
	}

	public static BallType RandomBall (){
		singleton.ball = singleton.nextBall;
		singleton.nextBall = (BallType)UnityEngine.Random.Range(0,Enum.GetValues(typeof(BallType)).Length);
		return singleton.ball;
	}

	public static PieceType RandomPiece (){
		singleton.piece = singleton.nextPiece;
		singleton.nextPiece = (PieceType)UnityEngine.Random.Range(0,Enum.GetValues(typeof(PieceType)).Length);
		return singleton.piece;
	}

}
