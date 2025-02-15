using System.Collections.Generic;
using UnityEngine;

public class EnPassantHandler : MonoBehaviour
{
    /// <summary>
    /// Returns an array of en passant target squares available for the given pawn.
    /// Only returns a diagonal capture move if an enemy pawn has just advanced two squares.
    /// </summary>
    /// <param name="pawn">The pawn for which to check en passant moves.</param>
    /// <returns>An array of valid en passant move positions.</returns>
    public static Vector3[] GetEnPassantMoves(PieceController pawn)
    {
        List<Vector3> enPassantMoves = new List<Vector3>();

        // This method only applies to pawns.
        if (pawn.selectedPieceType != PieceController.pieceType.Pawn)
            return enPassantMoves.ToArray();

        if (GameManager.Instance.lastMovedPiece != null &&
            GameManager.Instance.lastMovedPiece.selectedPieceType == PieceController.pieceType.Pawn)
        {
            // Standardize positions.
            Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos; // assumed standardized
            Vector3 lastMoveEnd = ChessUtilities.BoardPosition(GameManager.Instance.lastMovedPiece.transform.position);
            Vector3 currentPos = ChessUtilities.BoardPosition(pawn.transform.position);

            // Check that the enemy pawn moved two squares, is on the same rank as our pawn,
            // and is horizontally adjacent.
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == currentPos.y &&
                Mathf.Abs(lastMoveEnd.x - currentPos.x) == 1)
            {
                int direction = pawn.isWhite ? 1 : -1;
                Vector3 enPassantTarget = new Vector3(lastMoveEnd.x, currentPos.y + direction, currentPos.z);
                // Only allow en passant if the target square is empty.
                if (!BoardManager.Instance.IsOccupied(enPassantTarget))
                {
                    enPassantMoves.Add(enPassantTarget);
                }
            }
        }

        return enPassantMoves.ToArray();
    }

    /// <summary>
    /// Checks the en passant capture conditions for the given pawn and target square.
    /// If conditions are met, removes the enemy pawn from the board and returns its PieceController.
    /// </summary>
    /// <param name="pawn">The pawn attempting the en passant capture.</param>
    /// <param name="target">The intended target square for the capture (already standardized).</param>
    /// <returns>The captured pawn's PieceController if en passant capture is executed; otherwise, null.</returns>
    public static PieceController HandleEnPassantCapture(PieceController pawn, Vector3 target)
    {
        target = ChessUtilities.BoardPosition(target);

        // Only apply for pawns.
        if (pawn.selectedPieceType != PieceController.pieceType.Pawn)
            return null;

        // Get the list of valid en passant moves.
        Vector3[] enPassantMoves = GetEnPassantMoves(pawn);
        // If the target is not one of the valid en passant moves, then do nothing.
        bool validTarget = false;
        foreach (Vector3 move in enPassantMoves)
        {
            if (Vector3.Distance(move, target) < 0.1f)
            {
                validTarget = true;
                break;
            }
        }
        if (!validTarget)
            return null;

        // Conditions are met; now identify the enemy pawn that is captured.
        if (GameManager.Instance.lastMovedPiece != null &&
            GameManager.Instance.lastMovedPiece.selectedPieceType == PieceController.pieceType.Pawn)
        {
            Vector3 lastMoveStart = ChessUtilities.BoardPosition(GameManager.Instance.lastMoveStartPos);
            Vector3 lastMoveEnd = ChessUtilities.BoardPosition(GameManager.Instance.lastMovedPiece.transform.position);
            Vector3 currentPos = ChessUtilities.BoardPosition(pawn.transform.position);

            // Confirm conditions (these should already be met via GetEnPassantMoves, but we double-check).
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == currentPos.y &&
                Mathf.Abs(lastMoveEnd.x - currentPos.x) == 1)
            {
                // The captured pawn's board position is determined by lastMoveEnd.
                Vector3 capturedPawnPosition = new Vector3(lastMoveEnd.x, lastMoveEnd.y, target.z);
                PieceController capturedPawn = BoardManager.Instance.GetPieceAt(capturedPawnPosition);
                if (capturedPawn != null && capturedPawn.selectedPieceType == PieceController.pieceType.Pawn)
                {
                    Debug.Log($"Captured via En Passant: {capturedPawn.name} at {capturedPawnPosition}");
                    BoardManager.Instance.RemovePieceAt(capturedPawnPosition);
                    capturedPawn.gameObject.SetActive(false);
                    return capturedPawn;
                }
            }
        }
        return null;
    }
}
