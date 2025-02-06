using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PositionPiecePair
{
    public Vector3 position;
    public PieceController piece;

}

public  class PiecePlacement : MonoBehaviour
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
   public static PiecePlacement piecePlacement = null;
    [SerializeField]
    private float positionZ = -1f; // Depth for pieces to be above board

    // Serialized List for Inspector debugging
    [SerializeField]
    public List<PositionPiecePair> occupiedPositionsList = new List<PositionPiecePair>();

    // Actual Dictionary (not directly visible in Inspector)
    public Dictionary<Vector3, PieceController> occupiedPositions = new Dictionary<Vector3, PieceController>();

 public static PiecePlacement Instance { get; private set; }

    private void Awake()
    {
         if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        SyncListToDictionary();
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

        if (occupiedPositions.ContainsKey(position))
        {
            Debug.LogWarning($"Attempted to place a piece at an occupied position: {position}");
            return;
        }

        GameObject piece = Instantiate(piecePrefab, position, Quaternion.identity);
        
       
        piece.transform.SetParent(transform);
        PieceController pieceController = piece.GetComponent<PieceController>();

        if (pieceController != null)
        {

            occupiedPositions.Add(position, pieceController);
            occupiedPositionsList.Add(new PositionPiecePair { position = position, piece = pieceController });
        }
        else
        {
            Debug.LogError($"Piece at {position} is missing a PieceController component.");
        }
    }

    public bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.ContainsKey(position);
    }

    public PieceController GetPieceAtPosition(Vector3 position)
    {
        if (occupiedPositions.TryGetValue(position, out PieceController piece))
        {
            return piece;
        }
        return null;
    }

    public void MovePiece(PieceController piece, Vector3 newPosition)
    {
        if (IsPositionOccupied(newPosition))
        {
            Debug.Log($"Position {newPosition} is occupied. Capture logic required.");
            return;
        }

        occupiedPositions.Remove(piece.transform.position);
        piece.transform.position = newPosition;
        occupiedPositions.Add(newPosition, piece);

        UpdateOccupiedPositions(); // Sync the list after moving
    }

    // Syncs the Dictionary into the List for debugging
    private void UpdateOccupiedPositions()
    {
        occupiedPositionsList.Clear();
        foreach (var kvp in occupiedPositions)
        {
            occupiedPositionsList.Add(new PositionPiecePair { position = kvp.Key, piece = kvp.Value });
        }
    }

    // Syncs the List into the Dictionary on Awake
    private void SyncListToDictionary()
    {
        occupiedPositions.Clear();
        foreach (var pair in occupiedPositionsList)
        {
            occupiedPositions[pair.position] = pair.piece;
        }
    }
}