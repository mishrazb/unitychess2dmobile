using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class MoveRecord
{
    public PieceController MovedPiece;         // The piece that was moved (e.g. the king).
    public Vector3 StartPosition;                // Its position before the move.
    public Vector3 EndPosition;                  // Its position after the move.
    
    public PieceController CapturedPiece;        // If a capture occurred; otherwise, null.
    public Vector3 CapturedPieceOriginalPosition;
    
    // Special move fields for castling:
    public bool IsCastling;                      // True if this record represents a castling move.
    public PieceController CastlingRook;         // The rook involved in castling.
    public Vector3 RookStartPosition;            // The rook's starting position.
    public Vector3 RookEndPosition;              // The rook's new position after castling.
    public GameObject capturedPieceGo; // this is for the undo move record so we can also remove the visual indicastor for the captured piece
    // (You can add fields for en passant, promotion, etc., as needed.)
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isWhiteTurn = true;
    public PieceController lastMovedPiece; // 🔥 Track last moved piece
    public Vector3 lastMoveStartPos; // 🔥 Track its starting position

        // Reference to the Game Over UI panel (assign via Inspector).
    public Text turnText;

    public int MaxUndoMoves = 10;
    public Text UndoMoveBtnText;
    public static event Action OnMoveCompleted;
    public GameOver gameOver;

public List<MoveRecord> moveHistory = new List<MoveRecord>();

 
    private void Awake()
    {
         turnText.text = "White's Turn";
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

      gameOver = gameOver.gameObject.GetComponent<GameOver>();
      //  gameOver.gameObject.SetActive(false);

    }
    public void UpdateTurnUI(){
// Update turn UI.
    if (isWhiteTurn)
    {
        turnText.text = "White's Turn";
    }
    else
    {
        turnText.text = "Black's Turn";
    }
    }
    public void EndTurn()
    {   

       // Toggle the turn.
    isWhiteTurn = !isWhiteTurn;

    

    // Fire the move completed event so that other systems (e.g., CheckControllers) update.
    OnMoveCompleted?.Invoke();

    // Check for checkmate using the GameOver script.
    if (gameOver.CheckForCheckmate())
    {   
        // If checkmate, determine the winner.
        string winner = isWhiteTurn ? "Black" : "White";
        gameOver.TriggerGameOver($"{winner} wins by Checkmate!");
    }

    }
   
    public bool IsCurrentPlayerTurn(bool isWhite)
    {

        return isWhite == isWhiteTurn;
    }
 /// <summary>
    /// Returns true if there are any move records.
    /// </summary>
    public bool HasMoveRecords()
    {
        return moveHistory.Count > 0;
    }
    /// <summary>
    /// Call this method after a move is executed.
    /// </summary>
    public void AddMoveRecord(MoveRecord record)
    {
        moveHistory.Add(record);
    }
    /// <summary>
    /// Undoes the last move.
    /// Returns true if the undo was successful, false otherwise.
    /// </summary>
    public bool UndoLastMove()
    {
        if (moveHistory.Count == 0)
        {
            return false;
        }

        // Retrieve the last move record.
        MoveRecord lastMove = moveHistory[moveHistory.Count - 1];

        // Reverse the move of the piece.
        PieceController movedPiece = lastMove.MovedPiece;
        Vector3 startPos = lastMove.StartPosition;
        Vector3 endPos = lastMove.EndPosition;

        // Update the board state: remove the piece from its current (end) position,
        // and place it back at its start.
        BoardManager.Instance.UpdatePiecePosition(endPos, startPos, movedPiece);
        movedPiece.transform.position = startPos;

        // If a piece was captured, restore it.
        if (lastMove.CapturedPiece != null)
        {
            PieceController capturedPiece = lastMove.CapturedPiece;
            Vector3 capturedOriginalPos = lastMove.CapturedPieceOriginalPosition;
            BoardManager.Instance.AddPiece(ChessUtilities.BoardPosition(capturedOriginalPos), capturedPiece);
            // Optionally, re-enable its collider or any components if they were disabled.
            capturedPiece.gameObject.SetActive(true);
            Destroy(lastMove.capturedPieceGo);
           


        }

        // If this move was a castling move, revert the rook's movement as well.
        if (lastMove.IsCastling)
        {
            PieceController rook = lastMove.CastlingRook;
            Vector3 rookStart = lastMove.RookStartPosition;
            Vector3 rookEnd = lastMove.RookEndPosition;

            // Update the board state for the rook.
            BoardManager.Instance.UpdatePiecePosition(rookEnd, rookStart, rook);
            rook.transform.position = rookStart;
        }

        // Remove the move record from history.
        moveHistory.RemoveAt(moveHistory.Count - 1);

        // Switch the turn back to the previous player.
        isWhiteTurn = !isWhiteTurn;

        // Optionally, update your UI (captured pieces, move history list, etc.) here.
        PieceController king = CheckController.GetKingFor(!movedPiece.isWhite);
        if (king != null)
            {
               
                if (king.GetComponent<CheckController>() != null)
                {
                     king.GetComponent<CheckController>().UpdateCheckIndicator();
                }
            }

    // Optionally update UI here.
    UpdateTurnUI();
    Debug.Log("Undo successful.");
    return true;
    }
   

    public bool IsMoveLegalConsideringCheck(PieceController movingPiece, Vector3 target)
{
    // Save original board position of the moving piece.
    Vector3 originalPos = ChessUtilities.BoardPosition(movingPiece.transform.position);
    target = ChessUtilities.BoardPosition(target);
    
    // Save a reference to any piece currently at the target square.
    PieceController capturedPiece = null;
    if (BoardManager.Instance.IsOccupied(target))
    {
        capturedPiece = BoardManager.Instance.GetPieceAt(target);
    }
    
    // --- Simulate the move ---
    // 1. Remove the moving piece from its original position.
    BoardManager.Instance.RemovePieceAt(originalPos);
    
    // 2. If there's a captured piece, remove it from the board.
    if (capturedPiece != null)
    {
        BoardManager.Instance.RemovePieceAt(target);
        // Temporarily disable its GameObject.
        capturedPiece.gameObject.SetActive(false);
    }
    
    // 3. Place the moving piece at the target.
    movingPiece.transform.position = target;
    BoardManager.Instance.AddPiece(target, movingPiece);
    
    // --- Check for check ---
    // Find your king (you can have a helper method that returns the king for a given color).
    PieceController king = CheckController.GetKingFor(movingPiece.isWhite);
    bool moveLeavesKingInCheck = false;
    if (king != null)
    {
        // Assume CheckScript has been modified to provide a public method IsInCheck().
         CheckController cc = king.GetComponent<CheckController>();
        if (cc != null)
        {
            moveLeavesKingInCheck = cc.IsInCheck();
        }
    }
    
    // --- Revert the simulated move ---
    // Remove the moving piece from the target.
    BoardManager.Instance.RemovePieceAt(target);
    movingPiece.transform.position = originalPos;
    BoardManager.Instance.AddPiece(originalPos, movingPiece);
    
    // If a piece was captured, restore it.
    if (capturedPiece != null)
    {
        capturedPiece.gameObject.SetActive(true);
        BoardManager.Instance.AddPiece(target, capturedPiece);
    }
    
    // If the simulated move left the king in check, then the move is illegal.
    return !moveLeavesKingInCheck;
}






}


