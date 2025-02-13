using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckController : MonoBehaviour
{
     // Reference to a prefab used to indicate check (assign via Inspector).
    public GameObject checkIndicatorPrefab;

    // The instance of the check indicator.
    private GameObject checkIndicatorInstance;

    // Cached reference to the king's PieceController.
    private PieceController kingPiece;

    void Start()
    {
        kingPiece = GetComponent<PieceController>();
        if (kingPiece == null)
        {
            Debug.LogError("CheckScript: No PieceController found on the King prefab.");
        }

        // Instantiate the check indicator as a child of the king and disable it initially.
        if (checkIndicatorPrefab != null)
        {
            checkIndicatorInstance = Instantiate(checkIndicatorPrefab, transform.position, Quaternion.identity, transform);
            checkIndicatorInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("CheckScript: No check indicator prefab assigned.");
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
                    return true;
                }
            }
        }
    }
    return false;
}

   /// <summary>
    /// Explcitly Deactivates the check indicator.
    /// </summary>
    public void DeactivateIndicator()
    {
        if (checkIndicatorInstance != null && checkIndicatorInstance.activeSelf)
        {
            checkIndicatorInstance.SetActive(false);
        }
    }

}
