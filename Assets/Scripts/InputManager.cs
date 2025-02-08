using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Tooltip("Layer mask for board tiles")]
    public LayerMask boardLayer;
    [Tooltip("Layer mask for pieces")]
    public LayerMask pieceLayer;

    // Cached reference to the BoardManager.
    private BoardManager boardManager;

    void Awake()
    {
        boardManager = BoardManager.Instance;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Combine the board and piece layers.
            int combinedLayerMask = boardLayer | pieceLayer;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, combinedLayerMask))
            {
                // Try to get the PieceController from the hit collider.
                PieceController clickedPiece = hit.collider.GetComponent<PieceController>();

                // If a piece is clicked...
                if (clickedPiece != null)
                {
                    // If no piece is selected...
                    if (PieceController.currentlySelectedPiece == null)
                    {
                        if (!GameManager.Instance.IsCurrentPlayerTurn(clickedPiece.isWhite))
                        {
                            Debug.Log("Not your turn!");
                            return;
                        }
                        clickedPiece.OnPieceClicked();
                    }
                    // If a piece is already selected:
                    else
                    {
                        // If clicking an enemy piece:
                        if (clickedPiece.isWhite != PieceController.currentlySelectedPiece.isWhite)
                        {
                            Vector3 target = ChessUtilities.BoardPosition(clickedPiece.transform.position);
                            boardManager.pieceMovement.OnTargetTileClicked(target);
                        }
                        else
                        {
                            // Switch selection to the friendly piece.
                            clickedPiece.OnPieceClicked();
                        }
                    }
                }
                else
                {
                    // No piece was clicked; if a board tile was clicked, handle movement.
                    PieceMovement clickedTile = hit.collider.GetComponentInParent<PieceMovement>();
                    if (clickedTile != null && PieceController.currentlySelectedPiece != null)
                    {
                        Vector3 target = ChessUtilities.BoardPosition(hit.collider.transform.position);
                        clickedTile.OnTargetTileClicked(target);
                    }
                }
            }
        }
    }
}