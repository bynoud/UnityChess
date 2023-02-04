﻿using System;
using System.Collections.Generic;
using UnityChess;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityChess.GlobalVar;
using static UnityChess.SquareUtil;

public class BoardManager : MonoBehaviourSingleton<BoardManager> {
	private const int BOARD_SIZE = FILE_MAX * RANK_MAX;

	private readonly GameObject[] allSquaresGO = new GameObject[BOARD_SIZE];
	private Dictionary<Square, GameObject> positionMap;

	public GameObject HighlighPref = null;
	private Queue<GameObject> HighlightsGO = new();

    private const string PieceModelPath = "XiangqiPieces/Default/";
	//private const string PieceModelPath = "PieceSets/Marble/";
	private const float BoardFileExpectedLength = 16f;
    private float BoardFileSideStart = -8f; // measured square at File=1
    private float BoardFileSideEnd = 8f; // measured square at File=<last>
    private float BoardRankSideStart = -8.8f;
    private float BoardRankSideEnd = 8.8f;
    private float BoardHeight = 1.2f;
    private float BoardPickupHeight = 3f;
    //private readonly System.Random rng = new System.Random();



    private void Awake() {
		GameManager.NewGameStartedEvent += OnNewGameStarted;
		GameManager.GameResetToHalfMoveEvent += OnGameResetToHalfMove;
		
		positionMap = new Dictionary<Square, GameObject>(BOARD_SIZE);
		//Transform boardTransform = transform;
		//Vector3 boardPosition = boardTransform.position;

        Transform boardTransform = InstanceTheBoard();
        Vector3 boardPosition = boardTransform.position;


        for (int file = 1; file <= FILE_MAX; file++) {
			for (int rank = 1; rank <= RANK_MAX; rank++) {
				//GameObject squareGO = new GameObject(SquareToString(file, rank)) {
				//	transform = {
				//		position = new Vector3(
				//			boardPosition.x + FileToSidePosition(file),
				//			boardPosition.y + BoardHeight,
				//			boardPosition.z + RankToSidePosition(rank)),
				//		parent = boardTransform
				//	},
				//	tag = "Square"
				//};
				GameObject squareGO = new GameObject(SquareToString(file, rank))
				{
					transform = {
						position = new Vector3(
							boardPosition.x + FileToSidePosition(file),
							boardPosition.y + BoardHeight,
							boardPosition.z + RankToSidePosition(rank)),
					},
					tag = "Square"
				};
				squareGO.transform.SetParent(boardTransform, false);

                positionMap.Add(new Square(file, rank), squareGO);
				allSquaresGO[(file - 1) * RANK_MAX + (rank - 1)] = squareGO;
			}
		}
	}

	private Transform InstanceTheBoard()
	{
        GameObject boardGO = Instantiate(
            Resources.Load(PieceModelPath + "BoardWrap") as GameObject,
            transform
        );
		Vector3 tr = boardGO.transform.Find("CornerStart").position;
		BoardFileSideStart = tr.x;
        BoardRankSideStart = tr.z;
		BoardHeight = tr.y;
        tr = boardGO.transform.Find("CornerEnd").position;
        BoardFileSideEnd = tr.x;
        BoardRankSideEnd = tr.z;
        tr = boardGO.transform.Find("Pickup").position;
		BoardPickupHeight = tr.y;

		float boardScale = BoardFileExpectedLength / (BoardFileSideEnd - BoardFileSideStart);
		boardGO.transform.localScale = new Vector3(boardScale, boardScale, boardScale);

		return boardGO.transform.Find("Chessboard");
    }


    private void OnNewGameStarted() {
		ClearBoard();
		
		foreach ((Square square, Piece piece) in GameManager.Instance.CurrentPieces) {
			CreateAndPlacePieceGO(piece, square);
		}

		EnsureOnlyPiecesOfSideAreEnabled(GameManager.Instance.SideToMove);
	}

	private void OnGameResetToHalfMove() {
		ClearBoard();

		foreach ((Square square, Piece piece) in GameManager.Instance.CurrentPieces) {
			CreateAndPlacePieceGO(piece, square);
		}

		GameManager.Instance.HalfMoveTimeline.TryGetCurrent(out HalfMove latestHalfMove);
		if (latestHalfMove.CausedCheckmate || latestHalfMove.CausedStalemate) SetActiveAllPieces(false);
		else EnsureOnlyPiecesOfSideAreEnabled(GameManager.Instance.SideToMove);
	}

	public void CastleRook(Square rookPosition, Square endSquare) {
		GameObject rookGO = GetPieceGOAtPosition(rookPosition);
		rookGO.transform.parent = GetSquareGOByPosition(endSquare).transform;
		rookGO.transform.localPosition = Vector3.zero;
	}

	public void CreateAndPlacePieceGO(Piece piece, Square position) {
		string modelName = $"{piece.Owner} {piece.GetType().Name}";
		GameObject pieceGO = Instantiate(
			Resources.Load(PieceModelPath + modelName) as GameObject,
			positionMap[position].transform
		);
		pieceGO.AddComponent<VisualPiece>();
		pieceGO.GetComponent<VisualPiece>().enabled = true;
		pieceGO.GetComponent<VisualPiece>().PieceColor = piece.Owner;
        pieceGO.GetComponent<VisualPiece>().PickupYRaise = BoardPickupHeight;

        /*if (!(piece is Knight) && !(piece is King)) {
			pieceGO.transform.Rotate(0f, (float) rng.NextDouble() * 360f, 0f);
		}*/
    }

	public void GetSquareGOsWithinRadius(List<GameObject> squareGOs, Vector3 positionWS, float radius) {
		float radiusSqr = radius * radius;
		foreach (GameObject squareGO in allSquaresGO) {
			if ((squareGO.transform.position - positionWS).sqrMagnitude < radiusSqr)
				squareGOs.Add(squareGO);
		}
	}

	public void SetActiveAllPieces(bool active) {
		VisualPiece[] visualPiece = GetComponentsInChildren<VisualPiece>(true);
		foreach (VisualPiece pieceBehaviour in visualPiece) pieceBehaviour.enabled = active;
	}

	public void EnsureOnlyPiecesOfSideAreEnabled(Side side) {
		VisualPiece[] visualPiece = GetComponentsInChildren<VisualPiece>(true);
		//Debug.Log($"enable check ${side} {visualPiece.Length}");
		foreach (VisualPiece pieceBehaviour in visualPiece) {
			Piece piece = GameManager.Instance.CurrentBoard[pieceBehaviour.CurrentSquare];

			//bool hasLegalMoves = GameManager.Instance.HasLegalMoves(piece);
			//pieceBehaviour.enabled = pieceBehaviour.PieceColor == side && hasLegalMoves;
			pieceBehaviour.enabled = pieceBehaviour.PieceColor == side; // just let the piece pickup, even without possible moves
        }

    }

	public void TryDestroyVisualPiece(Square position) {
		VisualPiece visualPiece = positionMap[position].GetComponentInChildren<VisualPiece>();
		if (visualPiece != null) DestroyImmediate(visualPiece.gameObject);
	}
	
	public GameObject GetPieceGOAtPosition(Square position) {
		GameObject square = GetSquareGOByPosition(position);
		return square.transform.childCount == 0 ? null : square.transform.GetChild(0).gameObject;
	}

	public void HighlightSquares(ICollection<Movement> moves)
	{
		string msg = "";
		foreach (Movement move in moves)
		{
			GameObject hlGO = Instantiate(HighlighPref, positionMap[move.End].transform);
			hlGO.tag = "SquareHighlight";
			HighlightsGO.Enqueue(hlGO);
			msg += $" {move.End}";
		}
		Debug.Log($"Higlighted {msg}");
	}

	public void StopHighlight()
	{
		Debug.Log("Stoping hight");
		while (HighlightsGO.Count > 0)
		{
			DestroyImmediate(HighlightsGO.Dequeue());
		}
	}
	
	// private static float FileOrRankToSidePosition(int index) {
	// 	float t = (index - 1) / 7f;
	// 	return Mathf.Lerp(-BoardPlaneSideHalfLength, BoardPlaneSideHalfLength, t);
	// }
	private float FileToSidePosition(int index) {
		float t = (index - 1) / (float)(FILE_MAX-1);
		return Mathf.Lerp(BoardFileSideStart, BoardFileSideEnd, t);
	}
	private float RankToSidePosition(int index) {
		float t = (index - 1) / (float)(RANK_MAX-1);
		return Mathf.Lerp(BoardRankSideStart, BoardRankSideEnd, t);
	}
	
	private void ClearBoard() {
		VisualPiece[] visualPiece = GetComponentsInChildren<VisualPiece>(true);

		foreach (VisualPiece pieceBehaviour in visualPiece) {
			DestroyImmediate(pieceBehaviour.gameObject);
		}
	}

	public GameObject GetSquareGOByPosition(Square position) => Array.Find(allSquaresGO, go => go.name == SquareToString(position));
}