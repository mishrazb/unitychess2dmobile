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

    // 🔥 Step 1: Validate Move - Special Case for En Passant
    Vector3[] validMoves = selectedPiece.GetValidMoves();
  
    bool isValidMove = false;
    bool isEnPassant = false;

    foreach (Vector3 move in validMoves)
    {
        Vector3 mmove = move;
        mmove.z = -1;
        if (Vector3.Distance(mmove, target) < 0.1f) // Handle floating-point precision
        {
            isValidMove = true;
            break;
        }
    }

    // 🔥 Step 2: En Passant Validation (Even if target is empty)
    if (!isValidMove && selectedPiece.selectedPieceType == PieceController.pieceType.Pawn)
    {
        Debug.Log("EntPassnant Identified");

        if (GameManager.Instance.lastMovedPiece != null &&
            GameManager.Instance.lastMovedPiece.selectedPieceType == PieceController.pieceType.Pawn)
        {
            Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos;
            Vector3 lastMoveEnd = GameManager.Instance.lastMovedPiece.transform.position;

            // Check if last move was a double-step pawn move
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == selectedPiece.transform.position.y &&
                Mathf.Abs(lastMoveEnd.x - selectedPiece.transform.position.x) == 1)
            {
               
                if (target == new Vector3(lastMoveEnd.x, selectedPiece.transform.position.y + (selectedPiece.isWhite ? 1 : -1), -1))
                {
                    isValidMove = true;
                    isEnPassant = true;
                }
            }
        }
    }

    if (!isValidMove)
    {
        Debug.Log("Invalid Move Attempt! The move is not in the valid moves list.");
        return;
    }

    // 🔥 Step 3: Capture Logic (Including En Passant)
    if (isEnPassant)
    {
        Vector3 capturedPawnPosition = new Vector3(target.x, selectedPiece.transform.position.y, -1);
        if (selectedPiece.piecePlacement.occupiedPositions.TryGetValue(capturedPawnPosition, out PieceController capturedPawn))
        {
            if (capturedPawn.selectedPieceType == PieceController.pieceType.Pawn)
            {
                Debug.Log("Captured via En Passant: " + capturedPawn.name);
                Destroy(capturedPawn.gameObject);
                selectedPiece.piecePlacement.occupiedPositions.Remove(capturedPawnPosition);
            }
        }
    }
    else if (selectedPiece.piecePlacement.occupiedPositions.TryGetValue(target, out PieceController targetPiece))
    {
        if (targetPiece.isWhite != selectedPiece.isWhite) // Standard capture
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

    // 🔥 Step 4: Update Board State
    Vector3 previousPosition = selectedPiece.transform.position;
    selectedPiece.piecePlacement.occupiedPositions.Remove(previousPosition);
    selectedPiece.piecePlacement.occupiedPositions[target] = selectedPiece;

    // Move piece
    target.z = -1;
    selectedPiece.transform.position = target;

    // 🔥 Store last moved piece for future en passant checks
    GameManager.Instance.lastMovedPiece = selectedPiece;
    GameManager.Instance.lastMoveStartPos = previousPosition;

    // 🔥 Step 5: Deselect & End Turn
    selectedPiece.isSelected = false;
    selectedPiece.Deselect();
    GameManager.Instance.EndTurn();
}
}