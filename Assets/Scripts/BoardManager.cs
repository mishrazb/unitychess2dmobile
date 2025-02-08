using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PositionPiecePair
{
    public Vector3 position;
    public PieceController piece;
}

public class BoardManager : MonoBehaviour
{
    // Serialized list for Inspector debugging.
    [SerializeField]
    private List<PositionPiecePair> occupiedPositionsList = new List<PositionPiecePair>();

    // The actual dictionary for board state.
    private Dictionary<Vector3, PieceController> occupiedPositions = new Dictionary<Vector3, PieceController>();

    // Cached reference to the PieceMovement component.
    public PieceMovement pieceMovement;

    // Singleton instance.
    public static BoardManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // Optionally cache PieceMovement (or set via Inspector).
        if (pieceMovement == null)
            pieceMovement = GetComponent<PieceMovement>();

        UpdateOccupiedPositionsList();
    }

    /// <summary>
    /// Standardizes the given position using ChessUtilities.
    /// </summary>
    private Vector3 Standardize(Vector3 pos)
    {
        return ChessUtilities.BoardPosition(pos);
    }

    /// <summary>
    /// Returns true if the standardized board position is occupied.
    /// </summary>
    public bool IsOccupied(Vector3 pos)
    {
        pos = Standardize(pos);
        return occupiedPositions.ContainsKey(pos);
    }

    /// <summary>
    /// Gets the piece at the standardized board position.
    /// </summary>
    public PieceController GetPieceAt(Vector3 pos)
    {
        pos = Standardize(pos);
        occupiedPositions.TryGetValue(pos, out PieceController piece);
        return piece;
    }

    /// <summary>
    /// Adds a piece at the specified position.
    /// If the position is already occupied, a warning is logged.
    /// </summary>
    public void AddPiece(Vector3 pos, PieceController piece)
    {
        pos = Standardize(pos);
        if (occupiedPositions.ContainsKey(pos))
        {
            Debug.LogWarning($"Attempted to add a piece at an occupied position: {pos}");
            return;
        }
        occupiedPositions.Add(pos, piece);
        UpdateOccupiedPositionsList();
    }

    /// <summary>
    /// Updates a piece's position in the board state.
    /// </summary>
    public void UpdatePiecePosition(Vector3 oldPos, Vector3 newPos, PieceController piece)
    {
        oldPos = Standardize(oldPos);
        newPos = Standardize(newPos);
        occupiedPositions.Remove(oldPos);
        occupiedPositions[newPos] = piece;
        UpdateOccupiedPositionsList();
    }

    /// <summary>
    /// Moves a piece to a new position.
    /// Note: Capture logic should be handled externally if the target is occupied.
    /// </summary>
    public void MovePiece(PieceController piece, Vector3 newPosition)
    {
        newPosition = Standardize(newPosition);
        Vector3 currentPos = Standardize(piece.transform.position);

        if (IsOccupied(newPosition))
        {
            Debug.Log($"Position {newPosition} is occupied. Capture logic required.");
            return;
        }

        occupiedPositions.Remove(currentPos);
        piece.transform.position = newPosition;
        occupiedPositions[newPosition] = piece;
        UpdateOccupiedPositionsList();
    }

    /// <summary>
    /// Rebuilds the debug list from the dictionary.
    /// </summary>
    private void UpdateOccupiedPositionsList()
    {
        occupiedPositionsList.Clear();
        foreach (var kvp in occupiedPositions)
        {
            occupiedPositionsList.Add(new PositionPiecePair
            {
                position = kvp.Key,
                piece = kvp.Value
            });
        }
    }
    public void RemovePieceAt(Vector3 pos)
{
    pos = ChessUtilities.BoardPosition(pos);
    if (occupiedPositions.ContainsKey(pos))
    {
         occupiedPositions.Remove(pos);
         UpdateOccupiedPositionsList();
    }
}
}
