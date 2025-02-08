using System.Linq;
using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    /// <summary>
    /// Called when a board tile is clicked.
    /// Converts the target position into a standardized board coordinate.
    /// </summary>
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
        if (!IsValidMove(target, selectedPiece))
        {
            Debug.Log("Invalid move attempt.");
            return;
        }

        // Process en passant before moving the piece.
        bool isEnPassant = HandleEnPassant(target, selectedPiece);
        bool isCapture = HandleCapture(target, selectedPiece);

        // Move the piece using the BoardManager's API.
        MovePiece(target, selectedPiece);

        // Store last move details for future en passant checks.
        GameManager.Instance.lastMovedPiece = selectedPiece;
        GameManager.Instance.lastMoveStartPos = startingPos;

        selectedPiece.isSelected = false;
        selectedPiece.Deselect();
        PieceController.currentlySelectedPiece = null;

        GameManager.Instance.EndTurn();
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
    private bool HandleCapture(Vector3 target, PieceController piece)
    {
        // Standardize the target coordinate.
        target = ChessUtilities.BoardPosition(target);

        // Get the piece at the target position via BoardManager.
        PieceController targetPiece = BoardManager.Instance.GetPieceAt(target);
        if (targetPiece != null && targetPiece.isWhite != piece.isWhite)
        {
            Debug.Log("Captured: " + targetPiece.name);
            BoardManager.Instance.RemovePieceAt(target);
            Destroy(targetPiece.gameObject);
            return true;
        }
        else
        {
            Debug.Log("Capture attempt failed: No enemy piece found at target or position mismatch.");
        }
        return false;
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
