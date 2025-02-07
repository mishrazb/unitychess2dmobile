using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    [SerializeField]
    private PieceController pieceController;  // Reference to the PieceController
   
    // Reference to the selected piece
    private void Start()
    {
        if(pieceController==null)
        pieceController = FindObjectOfType<PieceController>();  // Alternatively, set it via inspector
    }

    void OnMouseDown()
    {
        // Ensure there's a selected piece
        if (PieceController.currentlySelectedPiece != null)
        {
            Vector3 targetPosition = transform.position;
            //ensure that the target position z and selected piece z are same otherwise validation will fail and pieces will not move.
            targetPosition.z = PieceController.currentlySelectedPiece.transform.position.z;
            // Check if the target position is a valid move for the currently selected piece
            if (PieceController.currentlySelectedPiece.IsValidMove(targetPosition))
            {
                // Call TryMovePiece() to move the piece
               TryMovePiece(targetPosition);
           }
            else
            {
                // Optionally handle invalid move (e.g., show feedback or sound)
               Debug.Log("Invalid move!");
            }
        }else{
            Debug.Log("Piece not selected");
        }
    }

void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PieceController clickedPiece = hit.collider.GetComponent<PieceController>();
            PieceMovement clickedTile = hit.collider.GetComponent<PieceMovement>();

            if (clickedPiece != null)
            {
                clickedPiece.OnMouseDown(); // Let PieceController handle selection/capture
            }
            else if (clickedTile != null)
            {
                clickedTile.OnMouseDown(); // Let PieceMovement handle movement
            }
        }
    }
}
public void TryMovePiece(Vector3 target)
{
    if (PieceController.currentlySelectedPiece == null)
        return;

    PieceController selectedPiece = PieceController.currentlySelectedPiece;

    // 🔥 Ensure it's the correct player's turn
    if (!GameManager.Instance.IsCurrentPlayerTurn(selectedPiece.isWhite))
    {
        Debug.Log("Not your turn!");
        return;
    }

    // 🔥 Step 1: Check if the move is valid
    Vector3[] validMoves = selectedPiece.GetValidMoves();
    bool isValidMove = false;

    foreach (Vector3 move in validMoves)
    {
        if (Vector3.Distance(move, target) < 0.1f) // Handling floating-point precision
        {
            isValidMove = true;
            break;
        }
    }

    if (!isValidMove)
    {
        Debug.Log("Invalid Move Attempt! The move is not in the valid moves list.");
        return;
    }

    // 🔥 Step 2: Save previous position and remove from occupied positions
    Vector3 previousPosition = selectedPiece.transform.position;
    selectedPiece.piecePlacement.occupiedPositions.Remove(previousPosition);

    // 🔥 Step 3: Move the piece to the target position first
    target.z = -1;  // Keep piece above the tile
    selectedPiece.transform.position = target;

    // 🔥 Step 4: Check if an enemy piece is at the new position after moving
    if (selectedPiece.piecePlacement.occupiedPositions.TryGetValue(target, out PieceController targetPiece))
    {
        if (targetPiece.isWhite != selectedPiece.isWhite) // Ensure it's an enemy piece
        {
            Debug.Log("Captured: " + targetPiece.name);
            Destroy(targetPiece.gameObject); // Remove enemy piece
        }
        else
        {
            Debug.Log("Cannot capture your own piece!");
            return; // Stop the move attempt
        }
    }

    // 🔥 Step 5: Update occupiedPositions dictionary with the new position
    selectedPiece.piecePlacement.occupiedPositions[target] = selectedPiece;

    // 🔥 Step 6: Deselect & End Turn
    selectedPiece.isSelected = false;
    selectedPiece.Deselect();
    GameManager.Instance.EndTurn();
}
}