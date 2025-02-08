using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlacement : MonoBehaviour
{
    [Header("White Pieces")]
    public GameObject whitePawn;
    public GameObject whiteRook;
    public GameObject whiteKnight;
    public GameObject whiteBishop;
    public GameObject whiteQueen;
    public GameObject whiteKing;

    [Header("Black Pieces")]
    public GameObject blackPawn;
    public GameObject blackRook;
    public GameObject blackKnight;
    public GameObject blackBishop;
    public GameObject blackQueen;
    public GameObject blackKing;

    [SerializeField]
    private float positionZ = -1f; // Depth for pieces to be above board

    public static PiecePlacement Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #region Piece Placement
    public void PlacePieces()
    {
        // Place pawns
        PlacePawns(whitePawn, 1);
        PlacePawns(blackPawn, 6);

        // Place rooks
        PlacePiece(whiteRook, 0, 0);
        PlacePiece(whiteRook, 0, 7);
        PlacePiece(blackRook, 7, 0);
        PlacePiece(blackRook, 7, 7);

        // Place knights
        PlacePiece(whiteKnight, 0, 1);
        PlacePiece(whiteKnight, 0, 6);
        PlacePiece(blackKnight, 7, 1);
        PlacePiece(blackKnight, 7, 6);

        // Place bishops
        PlacePiece(whiteBishop, 0, 2);
        PlacePiece(whiteBishop, 0, 5);
        PlacePiece(blackBishop, 7, 2);
        PlacePiece(blackBishop, 7, 5);

        // Place queens
        PlacePiece(whiteQueen, 0, 3);
        PlacePiece(blackQueen, 7, 3);

        // Place kings
        PlacePiece(whiteKing, 0, 4);
        PlacePiece(blackKing, 7, 4);
    }
    #endregion

    private void PlacePawns(GameObject pawnPrefab, int row)
    {
        for (int col = 0; col < 8; col++)
        {
            PlacePiece(pawnPrefab, row, col);
        }
    }

    private void PlacePiece(GameObject piecePrefab, int row, int col)
    {
        Vector3 position = new Vector3(col, row, positionZ);
        // Standardize the position.
        position = ChessUtilities.BoardPosition(position);

        // Check with BoardManager if the position is occupied.
        if (BoardManager.Instance.IsOccupied(position))
        {
            Debug.LogWarning($"Attempted to place a piece at an occupied position: {position}");
            return;
        }

        GameObject piece = Instantiate(piecePrefab, position, Quaternion.identity);
        piece.transform.SetParent(transform);
        PieceController pieceController = piece.GetComponent<PieceController>();

        if (pieceController != null)
        {
            // Instead of directly modifying BoardManager's dictionary, call AddPiece.
            BoardManager.Instance.AddPiece(position, pieceController);
        }
        else
        {
            Debug.LogError($"Piece at {position} is missing a PieceController component.");
        }
    }
}
