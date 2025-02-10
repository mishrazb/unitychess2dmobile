using System.Collections.Generic;
using UnityEngine;

public static class SpecialMoves
{
     /// <summary>
    /// Validates whether castling is allowed for the given king.
    /// This method checks that the king is in its starting position, that the corresponding rook is in its starting position,
    /// and that all squares between the king and rook are unoccupied.
    /// It does not check for check conditions—those should be integrated as needed.
    /// </summary>
    /// <param name="king">The king piece attempting to castle.</param>
    /// <param name="boardManager">The central BoardManager instance.</param>
    /// <returns>True if castling is valid (on at least one side), false otherwise.</returns>
    public static bool IsCastlingValid(PieceController king, BoardManager boardManager)
    {
        // Standardize the king's current position.
        Vector3 kingPos = ChessUtilities.BoardPosition(king.transform.position);

        // Determine starting positions based on color.
        // Adjust these values as needed to match your board's coordinate system.
        Vector3 kingStartPos = king.isWhite ? new Vector3(4, 0, -1) : new Vector3(4, 7, -1);
        if (kingPos != kingStartPos)
        {
            Debug.Log("King is not in its starting position; castling not allowed.");
            return false;
        }

        // (Optional) If you track whether a piece has moved, check that the king has not moved.
         if (king.kingHasMoved) return false;
        

        // Check kingside castling.
        Vector3 kingsideRookPos = king.isWhite ? new Vector3(7, 0, -1) : new Vector3(7, 7, -1);
        PieceController kingsideRook = boardManager.GetPieceAt(kingsideRookPos);
        
        
        
        bool kingsideValid = false;
        if (kingsideRook != null && kingsideRook.selectedPieceType == PieceController.pieceType.Rook)
        {
            // (Optional) Check that the rook has not moved.
             if (kingsideRook.rookHasmoved) { return false; }

            // For kingside, the squares between the king and rook must be empty.
            Vector3 square1 = king.isWhite ? new Vector3(5, 0, -1) : new Vector3(5, 7, -1);
            Vector3 square2 = king.isWhite ? new Vector3(6, 0, -1) : new Vector3(6, 7, -1);
            if (!boardManager.IsOccupied(square1) && !boardManager.IsOccupied(square2))
            {
                kingsideValid = true;
                Debug.Log("Castling on kingside is valid, spawn valid highlights");
            }
        }

        // Check queenside castling.
        Vector3 queensideRookPos = king.isWhite ? new Vector3(0, 0, -1) : new Vector3(0, 7, -1);
        PieceController queensideRook = boardManager.GetPieceAt(queensideRookPos);
        bool queensideValid = false;
        if (queensideRook != null && queensideRook.selectedPieceType == PieceController.pieceType.Rook)
        {
            if (queensideRook.rookHasmoved) { return false; }

            // For queenside, the squares between the king and rook must be empty.
            // For white: check squares (3,0), (2,0), and (1,0); for black: (3,7), (2,7), and (1,7).
            Vector3 sq1 = king.isWhite ? new Vector3(3, 0, -1) : new Vector3(3, 7, -1);
            Vector3 sq2 = king.isWhite ? new Vector3(2, 0, -1) : new Vector3(2, 7, -1);
            Vector3 sq3 = king.isWhite ? new Vector3(1, 0, -1) : new Vector3(1, 7, -1);
            if (!boardManager.IsOccupied(sq1) && !boardManager.IsOccupied(sq2) && !boardManager.IsOccupied(sq3))
            {
                queensideValid = true;
                Debug.Log("Castling on queenside is valid, spawn valid highlights");
            }
        }

        // You can choose to return true if at least one side is valid.
        if (kingsideValid || queensideValid)
        {
            Debug.Log($"Castling valid: Kingside = {kingsideValid}, Queenside = {queensideValid}");
            return true;
        }

        Debug.Log("Castling conditions not met.");
        return false;
    }

    /// <summary>
    /// Executes castling for the given king, moving both the king and the corresponding rook.
    /// Assumes that castling conditions have already been validated and that castlingTarget
    /// is one of the valid castling moves (i.e. two squares horizontally from the king's start).
    /// </summary>
    /// <param name="king">The king piece that is castling.</param>
    /// <param name="boardManager">The board manager for updating board state.</param>
    /// <param name="castlingTarget">The target square for the king (e.g., (6,0,-1) for white kingside castling).</param>
    public static bool ExecuteCastling(PieceController king, BoardManager boardManager, Vector3 castlingTarget)
    {
        // Standardize the castling target.
        castlingTarget = ChessUtilities.BoardPosition(castlingTarget);
        // Define the king's starting position.
        Vector3 kingStartPos = king.isWhite ? new Vector3(4, 0, -1) : new Vector3(4, 7, -1);
        
        // Determine castling side based on the target:
        // If the target square's x-coordinate is greater than the king's starting x,
        // assume kingside castling; otherwise, queenside.
        bool isKingside = castlingTarget.x > kingStartPos.x;

        // Define the rook's starting and target positions based on side.
        Vector3 rookStartPos, rookTargetPos;
        if (isKingside)
        {
           
            // Kingside:
            // King moves from (4,?) to (6,?); rook moves from (7,?) to (5,?).
            rookStartPos = king.isWhite ? new Vector3(7, 0, -1) : new Vector3(7, 7, -1);
            rookTargetPos = king.isWhite ? new Vector3(5, 0, -1) : new Vector3(5, 7, -1);
            
      
        }
        else
        {
            // Queenside:
            // King moves from (4,?) to (2,?); rook moves from (0,?) to (3,?).
            rookStartPos = king.isWhite ? new Vector3(0, 0, -1) : new Vector3(0, 7, -1);
            rookTargetPos = king.isWhite ? new Vector3(3, 0, -1) : new Vector3(3, 7, -1);
        }

        // Retrieve the corresponding rook from the board state.
        PieceController rook = boardManager.GetPieceAt(rookStartPos);
        if (rook == null)
        {
            Debug.LogError("Castling failed: Rook not found at " + rookStartPos);
            return false;
        }

        // Remove the king and rook from their old positions.
        boardManager.RemovePieceAt(kingStartPos);
        boardManager.RemovePieceAt(rookStartPos);

        // Update the transforms.
        king.transform.position = castlingTarget;
        rook.transform.position = rookTargetPos;

        // Add them back to the board state at their new positions.
        boardManager.AddPiece(ChessUtilities.BoardPosition(king.transform.position), king);
        boardManager.AddPiece(ChessUtilities.BoardPosition(rook.transform.position), rook);

        // (Optional) Mark the king and rook as having moved if you maintain such flags.
         king.kingHasMoved = true;
         rook.rookHasmoved = true;

        Debug.Log("Castling executed: " + (isKingside ? "Kingside" : "Queenside"));
        return true;
    }
/// <summary>
    /// Returns an array of valid castling target squares for the given king.
    /// Assumes that a castling move is only allowed if the king is on its starting square.
    /// The method checks kingside and queenside castling separately and returns the target
    /// square for each valid castling move.
    /// </summary>
    public static Vector3[] GetCastlingMoves(PieceController king, BoardManager boardManager)
    {
        List<Vector3> castlingMoves = new List<Vector3>();
        // Standardize the king's position.
        Vector3 kingPos = ChessUtilities.BoardPosition(king.transform.position);
        // Define starting positions (adjust these as per your board's coordinates)
        Vector3 kingStartPos = king.isWhite ? new Vector3(4, 0, -1) : new Vector3(4, 7, -1);
        if (kingPos != kingStartPos)
            return castlingMoves.ToArray();

        // Check kingside castling.
        Vector3 kingsideRookPos = king.isWhite ? new Vector3(7, 0, -1) : new Vector3(7, 7, -1);
        PieceController kingsideRook = boardManager.GetPieceAt(kingsideRookPos);
        if (kingsideRook != null && kingsideRook.selectedPieceType == PieceController.pieceType.Rook)
        {
            // (Optional) If you track if a piece has moved, check that neither has moved.
            // For kingside, the squares between the king and rook must be empty.
            Vector3 square1 = king.isWhite ? new Vector3(5, 0, -1) : new Vector3(5, 7, -1);
            Vector3 square2 = king.isWhite ? new Vector3(6, 0, -1) : new Vector3(6, 7, -1);
            if (!boardManager.IsOccupied(square1) && !boardManager.IsOccupied(square2))
            {
                // The king normally moves two squares toward the rook.
                Vector3 kingsideTarget = king.isWhite ? new Vector3(2, 0, -1) : new Vector3(2, 0, -1);
               Debug.Log("Additional castling move " + kingsideTarget);
                castlingMoves.Add(kingsideTarget);
            }
        }
        // Check queenside castling.
        Vector3 queensideRookPos = king.isWhite ? new Vector3(0, 0, -1) : new Vector3(0, 7, -1);
        PieceController queensideRook = boardManager.GetPieceAt(queensideRookPos);
        if (queensideRook != null && queensideRook.selectedPieceType == PieceController.pieceType.Rook)
        {
            // For queenside, the squares between the king and rook must be empty.
            // For white, check squares at (3,0), (2,0), and (1,0); for black: (3,7), (2,7), and (1,7).
            Vector3 sq1 = king.isWhite ? new Vector3(3, 0, -1) : new Vector3(3, 7, -1);
            Vector3 sq2 = king.isWhite ? new Vector3(2, 0, -1) : new Vector3(2, 7, -1);
            Vector3 sq3 = king.isWhite ? new Vector3(1, 0, -1) : new Vector3(1, 7, -1);
            if (!boardManager.IsOccupied(sq1) && !boardManager.IsOccupied(sq2) && !boardManager.IsOccupied(sq3))
            {
                // The king typically moves two squares toward the queenside rook.
              
                Vector3 queensideTarget = king.isWhite ? new Vector3(-2, 0, -1) : new Vector3(-2, 0, -1);
                  Debug.Log("Additional castling move " + queensideTarget);
                castlingMoves.Add(queensideTarget);
            }
        }
        return castlingMoves.ToArray();
    }
    // Methods for en passant or other special moves could be added here.

    public static bool isCastlingMoveAttempt(PieceController king, BoardManager boardManager, Vector3 target){

        Vector3 kingsideTarget = king.isWhite ? new Vector3(2, 0, -1) : new Vector3(2, 7, -1);
        Vector3 queensideTarget = king.isWhite ? new Vector3(-2, 0, -1) : new Vector3(-2, 7, -1);


        if(target!=null){
            if(target == kingsideTarget || target == queensideTarget){
                return true;
            }
        }
        return false;
    }


}