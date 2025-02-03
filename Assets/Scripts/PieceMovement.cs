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
        }
    }

   public  void TryMovePiece(Vector3 target)
    {

       if (!GameManager.Instance.IsCurrentPlayerTurn(PieceController.currentlySelectedPiece.isWhite))
        {
            Debug.Log("Not your turn!");
            return;
        }


        GameObject selectedPiece = PieceController.currentlySelectedPiece.gameObject;
        // Check if the move is valid before moving
        if (PieceController.currentlySelectedPiece.IsValidMove(target))
        {
                //change z position to keep the piece above the tile
              target.z = -1;
            // Move the piece to the new position
           selectedPiece.transform.position = target;
            PieceController.currentlySelectedPiece.isSelected = false;  // Deselect after moving
            PieceController.currentlySelectedPiece.Deselect();  // Clear previous highlights
             GameManager.Instance.EndTurn(); // End turn after a successful move
        }
        else
        {
            Debug.Log("Invalid Move Attempt!");
        }
    }
    



}