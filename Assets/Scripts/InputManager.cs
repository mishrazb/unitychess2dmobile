using System;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Tooltip("Layer mask for board tiles")]
    public LayerMask boardLayer;
    [Tooltip("Layer mask for pieces")]
    public LayerMask pieceLayer;

    // Cached reference to the BoardManager.
    private BoardManager boardManager;
   // private PieceMovement clickedTile;
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

            if (clickedPiece != null)
            {
                // No piece is currently selected.
                if (PieceController.currentlySelectedPiece == null)
                {
                    if (!GameManager.Instance.IsCurrentPlayerTurn(clickedPiece.isWhite))
                    {
                        Debug.Log("Not your turn!");
                        return;
                    }
                    clickedPiece.OnPieceClicked();
                }
                // A piece is already selected.
                else
                {
                    // If the clicked piece belongs to the opponent, attempt capture.
                   
                   if (clickedPiece.isWhite != PieceController.currentlySelectedPiece.isWhite)
                    {
                        Vector3 target = ChessUtilities.BoardPosition(clickedPiece.transform.position);
                        if (PieceController.currentlySelectedPiece.IsValidMove(target))
                        {
                            Debug.Log("Attempting capture move to " + target);
                            // Use the cached pieceMovement from BoardManager instead of GetComponent.
                            OnTargetTileClicked(target);
                        }
                        else
                        {
                            Debug.Log("Enemy piece is not on a valid move square for capture.");
                        }
                    }
                    // If a friendly piece is clicked, change selection.
                    else
                    {
                        clickedPiece.OnPieceClicked();
                    }
                }
            }
            else
            {
                // No piece was clicked; check if a board tile was hit.
         
                Vector3 target = ChessUtilities.BoardPosition(hit.collider.transform.position);
                if (target != null && PieceController.currentlySelectedPiece != null)
                {
                    OnTargetTileClicked(target);
                }
            }
        }
    }
}


 public void OnTargetTileClicked(Vector3 targetPosition)
    {
        if (PieceController.currentlySelectedPiece != null)
        {
            // Standardize the target board position.
            targetPosition = ChessUtilities.BoardPosition(targetPosition);
            if (PieceController.currentlySelectedPiece.IsValidMove(targetPosition))
            {
                TryMovePiece(targetPosition);
            }
            else
            {
                Debug.Log("PieceMovement: Invalid move!");
            }
        }
        else
        {
            Debug.Log("Piece not selected");
        }
    }

    /// <summary>
    /// Attempts to move the currently selected piece to the given target.
    /// Records the starting position (for en passant) before moving.
    /// </summary>
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

        // Save the starting (pre-move) board position.
        Vector3 startingPos = ChessUtilities.BoardPosition(selectedPiece.transform.position);

        Debug.Log("Is valid move target " + target + " for " + selectedPiece.gameObject.name);
        
        bool castlingAttempt = HandleCastling(target, selectedPiece);



        if (!castlingAttempt){
        if (!IsValidMove(target, selectedPiece))
            {
                Debug.Log("Invalid move attempt.");
                return;
            }
        }
        // Process en passant before moving the piece.
        bool isEnPassant = HandleEnPassant(target, selectedPiece);
        
        //capture piece if valid.
       PieceController capturedPiece = HandleCapture(target, selectedPiece);
        
        
        if (!castlingAttempt){
                // Move the piece using the BoardManager's API.
                if (selectedPiece.selectedPieceType == PieceController.pieceType.King){
                    selectedPiece.kingHasMoved = true;
                }
                if (selectedPiece.selectedPieceType == PieceController.pieceType.Rook){
                    selectedPiece.rookHasmoved = true;
                }
                MovePiece(target, selectedPiece);
        }
        // Store last move details for future en passant checks.
        GameManager.Instance.lastMovedPiece = selectedPiece;
        GameManager.Instance.lastMoveStartPos = startingPos;
        GameManager.Instance.AddMove(selectedPiece,startingPos, target,capturedPiece); 

        selectedPiece.isSelected = false;
        selectedPiece.Deselect();

        
        PieceController.currentlySelectedPiece = null;
        GameManager.Instance.EndTurn();
    }

    private bool HandleCastling(Vector3 target, PieceController piece)
    {
        target = ChessUtilities.BoardPosition(target);

       
        //PieceController targetPiece = BoardManager.Instance.GetPieceAt(target);
        if (piece.selectedPieceType == PieceController.pieceType.King)
            {
                PieceController selectedKing = PieceController.currentlySelectedPiece;
                
                if (SpecialMoves.IsCastlingValid(selectedKing, BoardManager.Instance))
                {
                 if( SpecialMoves.isCastlingMoveAttempt(selectedKing, BoardManager.Instance,target) ){
                    SpecialMoves.ExecuteCastling(selectedKing, BoardManager.Instance, target);
                    return true;
                }
               }
            }
        return false;
    }

    /// <summary>
    /// Validates the target move against the piece’s list of valid moves.
    /// </summary>
    private bool IsValidMove(Vector3 target, PieceController piece)
    {
        // Ensure the target is in board coordinates.
        target = ChessUtilities.BoardPosition(target);
        Vector3[] validMoves = piece.GetValidMoves();
        return validMoves.Any(move => Vector3.Distance(move, target) < 0.1f);
    }

    /// <summary>
    /// Checks if the move results in a capture.
    /// If so, removes the captured piece from the board.
    /// </summary>
    private PieceController HandleCapture(Vector3 target, PieceController piece)
    {
        // Standardize the target coordinate.
        target = ChessUtilities.BoardPosition(target);

        // Get the piece at the target position via BoardManager.
        PieceController targetPiece = BoardManager.Instance.GetPieceAt(target);
        if (targetPiece != null && targetPiece.isWhite != piece.isWhite)
        {
            Debug.Log("Captured: " + targetPiece.name);
            Transform parent;
            if(piece.isWhite){
             parent = GameObject.Find("WhiteCapturedPieces").transform;
            }else{
             parent = GameObject.Find("BackCapturedPieces").transform;
            }
            int totalCapturedPieces = parent.childCount;
            
            float posX = totalCapturedPieces > 0 ? totalCapturedPieces*0.5f : 0;
            GameObject capturedP = Instantiate(targetPiece.gameObject, new Vector3(posX,0,-1), Quaternion.identity);
           
          
            capturedP.transform.localScale = new Vector3(0.5f, 0.5f,0.5f);
            capturedP.transform.SetParent( parent , false);
            

            BoardManager.Instance.RemovePieceAt(target);
            Destroy(targetPiece.gameObject);
            return capturedP.GetComponent<PieceController>();
        }
        else
        {
            Debug.Log("Capture attempt failed: No enemy piece found at target or position mismatch.");
        }
        return null;
    }

    /// <summary>
    /// Processes an en passant move if the conditions are met.
    /// Uses the stored last move data to determine if an adjacent enemy pawn moved two squares.
    /// </summary>
    private bool HandleEnPassant(Vector3 target, PieceController piece)
    {
        target = ChessUtilities.BoardPosition(target);
        if (piece.selectedPieceType != PieceController.pieceType.Pawn)
            return false;

        if (GameManager.Instance.lastMovedPiece != null &&
            GameManager.Instance.lastMovedPiece.selectedPieceType == PieceController.pieceType.Pawn)
        {
            Vector3 lastMoveStart = ChessUtilities.BoardPosition(GameManager.Instance.lastMoveStartPos);
            Vector3 lastMoveEnd = ChessUtilities.BoardPosition(GameManager.Instance.lastMovedPiece.transform.position);
            Vector3 currentPos = ChessUtilities.BoardPosition(piece.transform.position);

            // Validate that the enemy pawn moved two squares, is on the same rank as our pawn,
            // and is horizontally adjacent.
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == currentPos.y &&
                Mathf.Abs(lastMoveEnd.x - currentPos.x) == 1)
            {
                // Determine the captured pawn's board position.
                Vector3 capturedPawnPosition = new Vector3(lastMoveEnd.x, lastMoveEnd.y, -1);

                PieceController capturedPawn = BoardManager.Instance.GetPieceAt(capturedPawnPosition);
                if (capturedPawn != null && capturedPawn.selectedPieceType == PieceController.pieceType.Pawn)
                {
                    Debug.Log($"Captured via En Passant: {capturedPawn.name} at {capturedPawnPosition}");
                    BoardManager.Instance.RemovePieceAt(capturedPawnPosition);
                    Destroy(capturedPawn.gameObject);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Moves the piece to the target position using the BoardManager's API.
    /// </summary>
    private void MovePiece(Vector3 target, PieceController piece)
    {
        target = ChessUtilities.BoardPosition(target);
        // Let the BoardManager handle updating the board state and moving the piece.
        BoardManager.Instance.MovePiece(piece, target);
    }



}
