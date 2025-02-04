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
    public static Dictionary<Vector3, PieceController> occupiedPositions = new Dictionary<Vector3, PieceController>();  // To track occupied squares
    
    [SerializeField]
    private float positionZ = -1f; // Depth for pieces to be above board

    void Start()
    {
        PlacePieces();
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

    // Place all pawns in a row
    private void PlacePawns(GameObject pawnPrefab, int row)
    {
        for (int col = 0; col < 8; col++)
        {
            PlacePiece(pawnPrefab, row, col);
        }
    }

    // Place a single piece at a specific row and column
    private void PlacePiece(GameObject piecePrefab, int row, int col)
    {
        Vector3 position = new Vector3(col, row, positionZ);
        
        // Avoid adding duplicate keys to the dictionary
        if (occupiedPositions.ContainsKey(position))
        {
            Debug.LogWarning($"Attempted to place a piece at an occupied position: {position}");
            return;
        }

        GameObject piece = Instantiate(piecePrefab, position, Quaternion.identity);
        PieceController pieceController = piece.GetComponent<PieceController>();

        if (pieceController != null)
        {
            occupiedPositions.Add(position, pieceController);
        }
        else
        {
            Debug.LogError($"Piece at {position} is missing a PieceController component.");
        }
    }

    // Check if a position is occupied
    public bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.ContainsKey(position);
    }

    // Get the piece at a specific position
    public PieceController GetPieceAtPosition(Vector3 position)
    {
        if (occupiedPositions.TryGetValue(position, out PieceController piece))
        {
            return piece;
        }
        return null;
    }

    // Move a piece to a new position
    public void MovePiece(PieceController piece, Vector3 newPosition)
    {
        if (IsPositionOccupied(newPosition))
        {
            // Handle piece capture logic
            Debug.Log($"Position {newPosition} is occupied. Capture logic required.");
            return;
        }

        // Remove piece from old position
        occupiedPositions.Remove(piece.transform.position);

        // Move the piece
        piece.transform.position = newPosition;

        // Update the dictionary with the new position
        occupiedPositions.Add(newPosition, piece);
    }
}