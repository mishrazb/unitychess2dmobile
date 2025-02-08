using System.Linq;
using UnityEngine;

public class PieceMovement : MonoBehaviour
{
  //  [SerializeField]
   // private PieceController pieceController;  // Reference to the PieceController
   
    // Reference to the selected piece
    private void Start()
    {
       // if(pieceController==null)
       // pieceController = FindObjectOfType<PieceController>();  // Alternatively, set it via inspector
    }

 /*   void OnMouseDown()
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
*/
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
                Debug.Log("Clicked Piece");
                clickedPiece.OnMouseDown(); // Let PieceController handle selection/capture
            }
            else if (clickedTile != null)
            {
                clickedTile.OnMouseDown(); // Let PieceMovement handle movement
            }
        }
    }
}
void OnMouseDown()
    {
        if (PieceController.currentlySelectedPiece != null)
        {
            Vector3 targetPosition = transform.position;
            targetPosition.z = PieceController.currentlySelectedPiece.transform.position.z;

            if (PieceController.currentlySelectedPiece.IsValidMove(targetPosition))
            {
                TryMovePiece(targetPosition);
            }
            else
            {
                Debug.Log("Invalid move!");
            }
        }
        else
        {
            Debug.Log("Piece not selected");
        }
    }

  public void TryMovePiece(Vector3 target)
    {
        if (PieceController.currentlySelectedPiece == null)
            return;

        PieceController selectedPiece = PieceController.currentlySelectedPiece;

        if (!GameManager.Instance.IsCurrentPlayerTurn(selectedPiece.isWhite))
        {
            Debug.Log("Not your turn!");
            return;
        }

        Vector3[] validMoves = selectedPiece.GetValidMoves();
        bool isValidMove = validMoves.Any(move => Vector3.Distance(move, target) < 0.1f);
        bool isEnPassant = false;

        if (isValidMove && selectedPiece.selectedPieceType == PieceController.pieceType.Pawn)
        {
            if (GameManager.Instance.lastMovedPiece != null &&
                GameManager.Instance.lastMovedPiece.selectedPieceType == PieceController.pieceType.Pawn)
            {
                Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos;
                Vector3 lastMoveEnd = GameManager.Instance.lastMovedPiece.transform.position;

                if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                    lastMoveEnd.y == selectedPiece.transform.position.y)
                {
                    bool isLeftEnPassant = (selectedPiece.transform.position.x == lastMoveEnd.x + 1);
                    bool isRightEnPassant = (selectedPiece.transform.position.x == lastMoveEnd.x - 1);

                    if ((isLeftEnPassant && lastMoveEnd.x > 0) || (isRightEnPassant && lastMoveEnd.x < 7))
                    {
                        Vector3 enPassantTarget = new Vector3(lastMoveEnd.x, selectedPiece.transform.position.y + (selectedPiece.isWhite ? 1 : -1), -1);
                        if (target == enPassantTarget)
                        {
                            isValidMove = true;
                            isEnPassant = true;
                        }
                    }
                }
            }
        }

        if (!isValidMove)
        {
            Debug.Log("Invalid Move Attempt! The move is not in the valid moves list.");
            return;
        }

        if (isEnPassant)
        {
            Vector3 capturedPawnPosition = new Vector3(target.x, target.y - (selectedPiece.isWhite ? 1 : -1), 0);
            if (selectedPiece.piecePlacement.occupiedPositions.TryGetValue(capturedPawnPosition, out PieceController capturedPawn))
            {
                if (capturedPawn.selectedPieceType == PieceController.pieceType.Pawn)
                {
                    Debug.Log($"Captured via En Passant: {capturedPawn.name}");
                    selectedPiece.piecePlacement.occupiedPositions.Remove(capturedPawnPosition);
                    Destroy(capturedPawn.gameObject);
                }
            }
        }
        else if (selectedPiece.piecePlacement.occupiedPositions.TryGetValue(target, out PieceController targetPiece))
        {
            if (targetPiece.isWhite != selectedPiece.isWhite)
            {
                Debug.Log("Captured: " + targetPiece.name);
                Destroy(targetPiece.gameObject);
                selectedPiece.piecePlacement.occupiedPositions.Remove(target);
            }
            else
            {
                Debug.Log("Cannot capture your own piece!");
                return;
            }
        }

        Vector3 previousPosition = selectedPiece.transform.position;
        selectedPiece.piecePlacement.occupiedPositions.Remove(previousPosition);
        selectedPiece.piecePlacement.occupiedPositions[target] = selectedPiece;

        target.z = -1;
        selectedPiece.transform.position = target;

        GameManager.Instance.lastMovedPiece = selectedPiece;
        GameManager.Instance.lastMoveStartPos = previousPosition;

        selectedPiece.isSelected = false;
        selectedPiece.Deselect();
        GameManager.Instance.EndTurn();
    }
}