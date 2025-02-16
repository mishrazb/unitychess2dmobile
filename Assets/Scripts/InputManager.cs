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
                            // Use the cached pieceMovement from BoardManager instead of GetComponent.
                            OnTargetTileClicked(target);
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
       
       
        if (!GameManager.Instance.IsMoveLegalConsideringCheck(selectedPiece, target))
            {
                Debug.Log("Move is illegal: It would leave the king in check.");
                return;
            }


       

        if (!GameManager.Instance.IsCurrentPlayerTurn(selectedPiece.isWhite))
        {
            Debug.Log("Not your turn!");
            return;
        }


        // Save the starting (pre-move) board position.
        Vector3 startingPos = ChessUtilities.BoardPosition(selectedPiece.transform.position);
        bool castlingAttempt = false;
        if( !CheckController.Instance.IsInCheck() ){
           castlingAttempt = HandleCastling(target, selectedPiece);
        }
        
        if (!castlingAttempt){
            if (!IsValidMove(target, selectedPiece))
                {
                    Debug.Log("Invalid move attempt.");
                    return;
                }
        }
       
        #region capture pieces
        PieceController capturedPiece = null;
        //capture piece if valid capture.
        capturedPiece = HandleCapture(target, selectedPiece);

         // Process en passant before moving the piece.
         if(capturedPiece == null){
           capturedPiece = EnPassantHandler.HandleEnPassantCapture(PieceController.currentlySelectedPiece, target);
        }
        #endregion

        if (!castlingAttempt){
                // Move the piece using the BoardManager's API.
                if (selectedPiece.selectedPieceType == PieceController.pieceType.King){
                    selectedPiece.kingHasMoved = true;
                }
                if (selectedPiece.selectedPieceType == PieceController.pieceType.Rook){
                    selectedPiece.rookHasmoved = true;
                }
                MovePiece(target, selectedPiece);
                
                
                 // if a piece was captured then also instantiate it in the WhiteOrBlack captured pieces prefab
                //this will visually indicate that a piece is captured. 
                GameObject capturedP = null;
                if(capturedPiece!=null){
                // lets try to find the gameObject, using this approach for now to manage the UI this object change its location // once the UI is ready perhaps creating a chached reference in inspector will be better.
                    Transform parent = capturedPiece.isWhite ? GameObject.Find("BackCapturedPieces").transform : GameObject.Find("WhiteCapturedPieces").transform;
                    int totalCapturedPieces = parent.childCount;
                    float posX = totalCapturedPieces > 0 ? totalCapturedPieces*0.5f : 0;
                    capturedP = Instantiate(capturedPiece.gameObject, new Vector3(posX,0,-1), Quaternion.identity);
                    capturedP.transform.localScale = new Vector3(0.5f, 0.5f,0.5f);
                    capturedP.transform.SetParent( parent , false);
                    Destroy(capturedP.GetComponent<PieceController>());
                    Destroy(capturedP.GetComponent<BoxCollider>());
                    capturedP.SetActive(true);
                    //capturedP.transform.Find("").SetActive(true);
                    GameObject outlinePreviousMove = ChessUtilities.FindGameObjectInChildWithTag(capturedP,"Outline");
                    if(outlinePreviousMove != null){
                        Destroy(outlinePreviousMove);
                    }
                }
                // only create move record for regular movement for castling, the move will be recorded by castlingHandler.
                MoveRecord record = new MoveRecord()
                {
                    MovedPiece = selectedPiece,
                    StartPosition = startingPos,
                    EndPosition = target,
                    CapturedPiece = capturedPiece,
                    CapturedPieceOriginalPosition = capturedPiece != null 
                                                    ? ChessUtilities.BoardPosition(capturedPiece.transform.position) 
                                                    : Vector3.zero,
                    IsCastling = false,       // This is a normal move.
                    CastlingRook = null,      // Not applicable for normal moves.
                    capturedPieceGo=capturedP
                };
                // Add the move record to the move history in the GameManager.
                GameManager.Instance.AddMoveRecord(record);
        }
        // Store last move details for future en passant checks.
        GameManager.Instance.lastMovedPiece = selectedPiece;
        GameManager.Instance.lastMoveStartPos = startingPos;
     

        selectedPiece.isSelected = false;
       selectedPiece.Deselect();

        
        // (Optional) If the moved piece is a pawn, check for promotion.
            if (selectedPiece.selectedPieceType == PieceController.pieceType.Pawn)
            {
                CheckPromotionForSelectedPiece();
            }

        PieceController.currentlySelectedPiece = null;
        GameManager.Instance.EndTurn();
        GameManager.Instance.HighlightLastMove(startingPos, selectedPiece);
    }

   /// <summary>
/// Checks if the currently moved piece is a pawn and, if so, triggers its promotion check.
/// </summary>
public void CheckPromotionForSelectedPiece()
{
    if (PieceController.currentlySelectedPiece != null &&
        PieceController.currentlySelectedPiece.selectedPieceType == PieceController.pieceType.Pawn)
    {
        PromotionHandler promotionHandler = PieceController.currentlySelectedPiece.GetComponent<PromotionHandler>();
        if (promotionHandler != null)
        {
            promotionHandler.CheckPromotion();
        }
    }
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
            BoardManager.Instance.RemovePieceAt(target);
            //Destroy(targetPiece.gameObject);
            targetPiece.gameObject.SetActive(false);
            return targetPiece.GetComponent<PieceController>();
        }
        return null;
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
