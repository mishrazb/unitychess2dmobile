using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class AIUtility
{
    /// <summary>
    /// Iterates over all pieces on the board for the given color and returns all legal moves
    /// as a list of MoveCandidate objects. This uses the GetValidMovesForCheck() method on each piece
    /// (which computes the full set of moves regardless of selection) and filters them with
    /// IsMoveLegalConsideringCheck.
    /// </summary>
   public static List<MoveCandidate> GetAllLegalMoves(bool forWhite)
{
    List<MoveCandidate> legalMoves = new List<MoveCandidate>();
    
    // Make a copy of the occupied positions to avoid modification during enumeration.
    List<KeyValuePair<Vector3, PieceController>> occupiedCopy = 
        new List<KeyValuePair<Vector3, PieceController>>(BoardManager.Instance.GetOccupiedPositions());
    
    foreach (KeyValuePair<Vector3, PieceController> kvp in occupiedCopy)
    {
        PieceController piece = kvp.Value;
        if (piece.isWhite == forWhite)
        {
            Vector3[] moves = piece.GetValidMovesForCheck();
            foreach (Vector3 move in moves)
            {
                if (GameManager.Instance.IsMoveLegalConsideringCheck(piece, move))
                {
                    legalMoves.Add(new MoveCandidate(piece, move));
                }
            }
        }
    }
    return legalMoves;
}

    /// <summary>
    /// Evaluates the board state with a simple material count. Positive values favor White; negative favor Black.
    /// </summary>
    public static float EvaluateBoard()
    {
        float score = 0f;
        foreach (KeyValuePair<Vector3, PieceController> kvp in BoardManager.Instance.GetOccupiedPositions())
        {
            PieceController piece = kvp.Value;
            float pieceValue = GetPieceValue(piece);
            score += piece.isWhite ? pieceValue : -pieceValue;
        }
        return score;
    }

    private static float GetPieceValue(PieceController piece)
    {
        switch (piece.selectedPieceType)
        {
            case PieceController.pieceType.Pawn:   return 1f;
            case PieceController.pieceType.Knight: return 3f;
            case PieceController.pieceType.Bishop: return 3f;
            case PieceController.pieceType.Rook:   return 5f;
            case PieceController.pieceType.Queen:  return 9f;
            case PieceController.pieceType.King:   return 1000f;
            default: return 0f;
        }
    }

    /// <summary>
    /// Simulates making a move on the board.
    /// It moves the given piece from its current position to target, handles capturing if needed,
    /// and returns a MoveRecord describing the move (to later be used for undo).
    /// </summary>
    public static MoveRecord MakeMove(PieceController piece, Vector3 target)
    {
        target = ChessUtilities.BoardPosition(target);
        Vector3 originalPos = ChessUtilities.BoardPosition(piece.transform.position);
        PieceController capturedPiece = null;

        // Check if there's an enemy piece at the target.
        if (BoardManager.Instance.IsOccupied(target))
        {
            capturedPiece = BoardManager.Instance.GetPieceAt(target);
        }
        
        // Simulate the move.
        BoardManager.Instance.RemovePieceAt(originalPos);
        if (capturedPiece != null)
        {
            BoardManager.Instance.RemovePieceAt(target);
            capturedPiece.gameObject.SetActive(false);
        }
        BoardManager.Instance.AddPiece(target, piece);
        piece.transform.position = target;
        
        MoveRecord record = new MoveRecord()
        {
            MovedPiece = piece,
            StartPosition = originalPos,
            EndPosition = target,
            CapturedPiece = capturedPiece,
            CapturedPieceOriginalPosition = capturedPiece != null ? ChessUtilities.BoardPosition(capturedPiece.transform.position) : Vector3.zero,
            IsCastling = false,
            CastlingRook = null,
            capturedPieceGo = null
        };
        return record;
    }

    /// <summary>
    /// Undoes a move based on the provided MoveRecord.
    /// Moves the moved piece back to its original position and restores any captured piece.
    /// </summary>
    public static void UndoMove(MoveRecord record)
    {
        // Revert the moved piece.
        BoardManager.Instance.RemovePieceAt(record.EndPosition);
        BoardManager.Instance.AddPiece(record.StartPosition, record.MovedPiece);
        record.MovedPiece.transform.position = record.StartPosition;

        // Restore captured piece, if any.
        if (record.CapturedPiece != null)
        {
            BoardManager.Instance.AddPiece(record.CapturedPieceOriginalPosition, record.CapturedPiece);
            record.CapturedPiece.transform.position = record.CapturedPieceOriginalPosition;
            record.CapturedPiece.gameObject.SetActive(true);
        }

        // Handle castling moves here if needed.
    }

    /// <summary>
    /// Checks if the game is over based on a simple condition: if either king is missing.
    /// You might expand this to include checkmate or stalemate conditions.
    /// </summary>
    public static bool IsGameOver()
    {
        PieceController whiteKing = CheckController.GetKingFor(true);
        PieceController blackKing = CheckController.GetKingFor(false);
        return whiteKing == null || blackKing == null;
    }
}
