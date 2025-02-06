using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    [SerializeField]
    private PieceController pieceController;  // Reference to the PieceController
   
    // Reference to the selected piece
    private void Start()
    {
        //if(pieceController==null)
        //pieceController = FindObjectOfType<PieceController>();  // Alternatively, set it via inspector
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
public void TryMovePiece(Vector3 target)
{
    if (!GameManager.Instance.IsCurrentPlayerTurn(PieceController.currentlySelectedPiece.isWhite))
    {
        Debug.Log("Not your turn!");
        return;
    }

    GameObject selectedPiece = PieceController.currentlySelectedPiece.gameObject;
    Vector3[] validMoves = PieceController.currentlySelectedPiece.GetValidMoves();
    Debug.Log("Selected Piece " + PieceController.currentlySelectedPiece.gameObject.name);
    Debug.Log("Target " + target);
    Debug.Log( "Valid " + validMoves.ToString());
    // Classical loop to check if the target is a valid move
    bool isValidMove = false;
    foreach (Vector3 move in validMoves)
    {
        if (move == target)
        {
            isValidMove = true;
            break;
        }
    }

    if (isValidMove)
    {
        Vector3 previousPosition = selectedPiece.transform.position;
        target.z = -1; // Ensure correct z position
        selectedPiece.transform.position = target;
        PieceController.currentlySelectedPiece.isSelected = false;
        PieceController.currentlySelectedPiece.Deselect();

        // Update occupiedPositions in PiecePlacement
        PieceController.currentlySelectedPiece.piecePlacement.occupiedPositions.Remove(previousPosition);
        PieceController.currentlySelectedPiece.piecePlacement.occupiedPositions[target] = PieceController.currentlySelectedPiece;

        GameManager.Instance.EndTurn();
    }
    else
    {
        Debug.Log("Invalid Move Attempt!");
    }
}
}