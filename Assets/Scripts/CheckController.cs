using System.Collections.Generic;
using UnityEngine;

public class CheckController : MonoBehaviour
{
    // Reference to a prefab used to indicate check (assign via Inspector).
    public GameObject checkIndicatorPrefab;

    // The instance of the check indicator.
    private GameObject checkIndicatorInstance;

    // Cached reference to the king's PieceController.
    public PieceController kingPiece;

    // Optional: static instance for easy global access (if only one king of a given type is expected).
    public static CheckController Instance;

    void Awake()
    {
        // If you want a singleton instance (assuming one CheckController per king), assign it here.
        Instance = this;
    }

    void Start()
    {
        // Cache the king's PieceController.
        kingPiece = GetComponent<PieceController>();
        if (kingPiece == null)
        {
            Debug.LogError("CheckController: No PieceController found on the King prefab.");
        }

        // Instantiate the check indicator as a child of the king and disable it initially.
        if (checkIndicatorPrefab != null)
        {
            checkIndicatorInstance = Instantiate(checkIndicatorPrefab, transform.position, Quaternion.identity, transform);
            checkIndicatorInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("CheckController: No check indicator prefab assigned.");
        }
    }
   
    private void OnEnable()
    {
        // Subscribe to the GameManager's move event.
        GameManager.OnMoveCompleted += UpdateCheckIndicator;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks or unwanted calls.
        GameManager.OnMoveCompleted -= UpdateCheckIndicator;
    }

    /// <summary>
    /// Called by the GameManager event when a move is completed.
    /// </summary>
    public void UpdateCheckIndicator()
    {
        bool inCheck = IsInCheck();
        if (checkIndicatorInstance != null)
        {
            checkIndicatorInstance.SetActive(inCheck);
        }
    }

    /// <summary>
    /// Forces an update of the check indicator. Call this if you need to refresh manually.
    /// </summary>
    public void ForceUpdateIndicator()
    {
        UpdateCheckIndicator();
    }

    /// <summary>
    /// Checks whether the king is under attack by any opposing piece.
    /// </summary>
    /// <returns>True if the king is in check, false otherwise.</returns>
    public bool IsInCheck()
    {
        
        // Standardize the king's position.
        Vector3 kingPos = ChessUtilities.BoardPosition(transform.position);

        // Iterate over all pieces on the board.
        foreach (KeyValuePair<Vector3, PieceController> kvp in BoardManager.Instance.GetOccupiedPositions())
        {
            PieceController enemyPiece = kvp.Value;
            // Only consider enemy pieces.
            if (enemyPiece.isWhite != kingPiece.isWhite)
            {
                Vector3[] enemyMoves = enemyPiece.GetValidMoves();
                foreach (Vector3 move in enemyMoves)
                {
                    if (ChessUtilities.BoardPosition(move) == kingPos)
                    {
                        Debug.Log("Checking check true");
                        return true;
                        
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Explicitly deactivates the check indicator.
    /// </summary>
    public void DeactivateIndicator()
    {
        if (checkIndicatorInstance != null && checkIndicatorInstance.activeSelf)
        {
            checkIndicatorInstance.SetActive(false);
        }
    }

    /// <summary>
    /// Returns true if the king is in checkmate.
    /// It first confirms the king is in check, then simulates all legal moves for all friendly pieces.
    /// </summary>
    public bool IsCheckMate()
    {
        // If the king isn't in check, then it's not checkmate.
       if (!IsInCheck())
        {
              Debug.Log("NOT IN CHECK");
            return false;
       }
        Debug.Log("Is IN CHECK RETURNED FALSE MOVING ON");
        // Retrieve all friendly pieces from the board.
        List<PieceController> friendlyPieces = new List<PieceController>();
        foreach (KeyValuePair<Vector3, PieceController> kvp in BoardManager.Instance.GetOccupiedPositions())
        {
            PieceController piece = kvp.Value;
            if (piece.isWhite == kingPiece.isWhite)
            {
                friendlyPieces.Add(piece);
            }
        }

        // For each friendly piece, check if any valid move will not leave the king in check.
        foreach (PieceController piece in friendlyPieces)
        {
            Vector3[] validMoves = piece.GetValidMoves();
            foreach (Vector3 move in validMoves)
            {
                // Use GameManager's simulation method to test the move.
                if (GameManager.Instance.IsMoveLegalConsideringCheck(piece, move))
                {
                    // Found at least one legal move: not checkmate.
                    return false;
                }
            }
        }
        // No legal moves found and the king is in check: checkmate.
        return true;
    }

    /// <summary>
    /// Static helper method that finds and returns the king for the given color.
    /// Assumes that kings are identifiable by their PieceType.
    /// </summary>
    /// <param name="isWhite">True for white king, false for black king.</param>
    /// <returns>The king's PieceController if found; otherwise, null.</returns>
    public static PieceController GetKingFor(bool isWhite)
    {
        foreach (KeyValuePair<Vector3, PieceController> kvp in BoardManager.Instance.GetOccupiedPositions())
        {
            PieceController piece = kvp.Value;
            if (piece.isWhite == isWhite && piece.selectedPieceType == PieceController.pieceType.King)
            {
                return piece;
            }
        }
        return null;
    }
}