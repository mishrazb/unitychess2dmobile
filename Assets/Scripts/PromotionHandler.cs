using UnityEngine;

public class PromotionHandler : MonoBehaviour
{
    // Reference to the promotion UI panel prefab
    public GameObject promotionUIPanel;
    public  GameObject currentPromotionalUIPanel;

    // Separate promotion prefabs for white and black.
    [Header("White Promotion Prefabs")]
    public GameObject whiteQueenPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;

    [Header("Black Promotion Prefabs")]
    public GameObject blackQueenPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;

    // Cached reference to the PieceController attached to this pawn.
    private PieceController pieceController;
    private bool promotionOffered = false;

    private void Start()
    {
        pieceController = GetComponent<PieceController>();
        
    }

    /// <summary>
    /// Checks if this pawn has reached the promotion rank.
    /// Call this after a pawn move.
    /// </summary>
    public void CheckPromotion()
    {
        Vector3 pos = ChessUtilities.BoardPosition(transform.position);
        if (pieceController.isWhite)
        {
            if (pos.y >= 7 && !promotionOffered)
            {
                OfferPromotion();
            }
        }
        else
        {
            if (pos.y <= 0 && !promotionOffered)
            {
                OfferPromotion();
            }
        }
    }

    private void OfferPromotion()
    {
        promotionOffered = true;
        if (promotionUIPanel != null)
        {
            currentPromotionalUIPanel = Instantiate(promotionUIPanel, Vector3.zero,Quaternion.identity);
            currentPromotionalUIPanel.transform.SetParent(GameObject.Find("GamePlayUICanvas").transform, false);
            // Assuming you have a reference to the PromotionUIHandler (e.g., assigned via the Inspector or found at runtime)
            currentPromotionalUIPanel.GetComponent<PromotionUIHandler>().SetPromotionUI(pieceController.isWhite, this);
           // promotionUIPanel.SetActive(true);

        }
        else
        {
            Debug.LogWarning("Promotion UI Panel is not assigned.");
        }
    }

    // UI Button callbacks:
    public void PromoteToQueen()
    {
        if (pieceController.isWhite)
            Promote(pieceController.isWhite ? whiteQueenPrefab : null);
        else
            Promote(pieceController.isWhite ? null : blackQueenPrefab);
    }

    public void PromoteToRook()
    {
        if (pieceController.isWhite)
            Promote(pieceController.isWhite ? whiteRookPrefab : null);
        else
            Promote(pieceController.isWhite ? null : blackRookPrefab);
    }

    public void PromoteToKnight()
    {
        if (pieceController.isWhite)
            Promote(pieceController.isWhite ? whiteKnightPrefab : null);
        else
            Promote(pieceController.isWhite ? null : blackKnightPrefab);
    }

    public void PromoteToBishop()
    {
        if (pieceController.isWhite)
            Promote(pieceController.isWhite ? whiteBishopPrefab : null);
        else
            Promote(pieceController.isWhite ? null : blackBishopPrefab);
    }

    /// <summary>
    /// Instantiates the new promoted piece at the pawn's position, updates board state, and deactivates the pawn.
    /// </summary>
    /// <param name="newPiecePrefab">The prefab of the promoted piece (must be non-null).</param>
    private void Promote(GameObject newPiecePrefab)
    {
        if (newPiecePrefab == null)
        {
            Debug.LogError("Promotion failed: Appropriate prefab is not assigned for this color.");
            return;
        }

        Vector3 pos = ChessUtilities.BoardPosition(transform.position);
        // Instantiate the promoted piece.
        GameObject newPiece = Instantiate(newPiecePrefab, pos, Quaternion.identity);
        newPiece.transform.SetParent(transform.parent, false);
        // Optionally, set its parent or adjust scale as needed.
        // Update board state: remove the pawn and add the new piece.
        BoardManager.Instance.RemovePieceAt(pos);
        PieceController newPieceController = newPiece.GetComponent<PieceController>();
        if (newPieceController != null)
        {
            BoardManager.Instance.AddPiece(pos, newPieceController);
        }
       
        // Hide the promotion UI.
         Debug.Log("Trying to destroy UI prmotion panel");
        if (currentPromotionalUIPanel != null)
        {
            Debug.Log("currentPromotionalUIPanel found");
            Destroy(currentPromotionalUIPanel);
        }

         // Deactivate the pawn to support undo.
        gameObject.SetActive(false);
    }
}
