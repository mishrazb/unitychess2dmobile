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
        // Copy to avoid modification during enumeration.
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
    /// Returns only capture moves from all legal moves.
    /// </summary>
    public static List<MoveCandidate> GetCaptureMoves(bool forWhite)
    {
        List<MoveCandidate> captures = new List<MoveCandidate>();
        List<MoveCandidate> allMoves = GetAllLegalMoves(forWhite);
        foreach (MoveCandidate candidate in allMoves)
        {
            if (BoardManager.Instance.IsOccupied(candidate.Target))
            {
                PieceController targetPiece = BoardManager.Instance.GetPieceAt(candidate.Target);
                if (targetPiece != null && targetPiece.isWhite != forWhite)
                    captures.Add(candidate);
            }
        }
        return captures;
    }

    /// <summary>
    /// Evaluates the board state using a simple material count.
    /// Positive values favor White; negative values favor Black.
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

        if (BoardManager.Instance.IsOccupied(target))
        {
            capturedPiece = BoardManager.Instance.GetPieceAt(target);
        }
        
        // Simulate move.
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
    /// </summary>
    public static void UndoMove(MoveRecord record)
    {
        BoardManager.Instance.RemovePieceAt(record.EndPosition);
        BoardManager.Instance.AddPiece(record.StartPosition, record.MovedPiece);
        record.MovedPiece.transform.position = record.StartPosition;

        if (record.CapturedPiece != null)
        {
            BoardManager.Instance.AddPiece(record.CapturedPieceOriginalPosition, record.CapturedPiece);
            record.CapturedPiece.transform.position = record.CapturedPieceOriginalPosition;
            record.CapturedPiece.gameObject.SetActive(true);
        }
        // Add castling undo if needed.
    }

    /// <summary>
    /// Checks if the game is over (e.g., if one king is missing).
    /// </summary>
    public static bool IsGameOver()
    {
        PieceController whiteKing = CheckController.GetKingFor(true);
        PieceController blackKing = CheckController.GetKingFor(false);
        return whiteKing == null || blackKing == null;
    }

    /// <summary>
    /// MINIMAX WITH ALPHA-BETA PRUNING.
    /// Searches for the best move given a depth, returning a numeric evaluation.
    /// </summary>
    public static float Minimax(int depth, float alpha, float beta, bool maximizingPlayer, bool forWhite)
    {
        if (depth == 0 || IsGameOver())
        {
            return QuiescenceSearch(alpha, beta, forWhite);
        }

        List<MoveCandidate> moves = GetAllLegalMoves(forWhite);
        // Optionally sort moves for better pruning (e.g., captures first) – not implemented here.

        if (maximizingPlayer)
        {
            float maxEval = float.NegativeInfinity;
            foreach (MoveCandidate move in moves)
            {
                MoveRecord record = MakeMove(move.Piece, move.Target);
                float eval = Minimax(depth - 1, alpha, beta, false, forWhite);
                UndoMove(record);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }
        else
        {
            float minEval = float.PositiveInfinity;
            foreach (MoveCandidate move in moves)
            {
                MoveRecord record = MakeMove(move.Piece, move.Target);
                float eval = Minimax(depth - 1, alpha, beta, true, forWhite);
                UndoMove(record);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

    /// <summary>
    /// A simple quiescence search that extends capture moves until the position is quiet.
    /// </summary>
    public static float QuiescenceSearch(float alpha, float beta, bool forWhite)
    {
        float standPat = EvaluateBoard();
        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;

        List<MoveCandidate> captureMoves = GetCaptureMoves(forWhite);
        foreach (MoveCandidate move in captureMoves)
        {
            MoveRecord record = MakeMove(move.Piece, move.Target);
            float score = -QuiescenceSearch(-beta, -alpha, forWhite);
            UndoMove(record);
            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }
        return alpha;
    }
}
