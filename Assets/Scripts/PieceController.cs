using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public bool isWhite;
    public bool kingHasMoved = false;
    public bool rookHasmoved = false;
    public enum pieceType { Pawn, Rook, Knight, Bishop, Queen, King }
    public pieceType selectedPieceType;

    [Header("Selection and Validation")]
    public GameObject highlightSelectedPiece;
    public GameObject highlightValidMoves;
    private GameObject goHighlight;
    public static PieceController currentlySelectedPiece = null;
    public List<GameObject> validMoveIndicators = new List<GameObject>();
        public bool isSelected = false;

    [SerializeField]    
    private Vector3[] cachedValidMoves;
    private bool movesCached = false;
    // We no longer need to reference PiecePlacement for board occupancy,
    // since BoardManager is now the central authority.
    // private PiecePlacement piecePlacement;


    /// <summary>
    /// Called when this piece is clicked.
    /// Handles selection (and deselection) and turn validation.
    /// </summary>
    public void OnPieceClicked()
    {
        // Ensure it is the correct turn.
        if (!GameManager.Instance.IsCurrentPlayerTurn(isWhite))
        {
            Debug.Log("Not your turn!");
            return;
        }

        // Deselect any previously selected piece that is different.
        if (currentlySelectedPiece != null && currentlySelectedPiece != this)
        {
            currentlySelectedPiece.Deselect();
        }

        // Toggle selection.
        isSelected = !isSelected;
        currentlySelectedPiece = isSelected ? this : null;

        if (isSelected)
        {
            if (goHighlight != null)
                Destroy(goHighlight);
            goHighlight = Instantiate(highlightSelectedPiece, transform.position, Quaternion.identity);
            goHighlight.transform.SetParent(transform);
            goHighlight.tag = "ValidMove";
            HighlightValidMoves();
        }
        else
        {
            Deselect();
        }
    }

    /// <summary>
    /// Deselects this piece and removes any valid move indicators.
    /// </summary>
    public void Deselect()
    {
        // Remove all valid move indicators by tag.
        GameObject[] outlines = GameObject.FindGameObjectsWithTag("ValidMove");
        foreach (GameObject outline in outlines)
        {
            Destroy(outline);
        }
        validMoveIndicators.Clear();

        // Clear the cached valid moves.
        cachedValidMoves = null;
        movesCached = false;

    }

    /// <summary>
    /// Highlights valid moves by instantiating indicators on the board.
    /// </summary>
    private void HighlightValidMoves()
    {
        Vector3[] validMoves = GetValidMoves();
        validMoveIndicators.Clear();
        foreach (Vector3 move in validMoves)
        {
            // Ensure the z coordinate matches the board standard.
            Vector3 mmove = move;
            mmove.z = ChessUtilities.BoardPosition(transform.position).z;

            if (IsValidMove(mmove) && GameManager.Instance.IsMoveLegalConsideringCheck(this, mmove)  )
            {
                GameObject validMove = Instantiate(highlightValidMoves, mmove, Quaternion.identity);
                validMove.transform.position = mmove;
                validMove.transform.SetParent(transform);
                validMove.tag = "ValidMove";
                validMoveIndicators.Add(validMove);
            }
        }
    }

    /// <summary>
    /// Determines if a move is valid for this piece.
    /// Uses the BoardManager’s state to check for occupancy.
    /// </summary>
    public bool IsValidMove(Vector3 move)
    {
        return IsValidMove(move, BoardManager.Instance);
    }

    private bool IsValidMove(Vector3 move, BoardManager boardManager)
    {
        if (!ChessUtilities.IsWithinBounds(move))
            return false;
        // Get any piece at the target square.
        PieceController other = boardManager.GetPieceAt(move);
        // If the square is occupied, it must be by an opponent.
        if (other != null)
            return other.isWhite != this.isWhite;
        return true;
    }

    /// <summary>
    /// Returns an array of board positions representing valid moves.
    /// </summary>
    public Vector3[] GetValidMoves()
    {
       // Only compute valid moves if this piece is the currently selected piece.
            if (PieceController.currentlySelectedPiece != this)
            {
                return new Vector3[0];
            }

            if (movesCached)
            {
                return cachedValidMoves;
            }


         cachedValidMoves = selectedPieceType switch
        {
            pieceType.Pawn => GetPawnMoves(),
            pieceType.Rook => GetRookMoves(),
            pieceType.Knight => GetKnightMoves(),
            pieceType.Bishop => GetBishopMoves(),
            pieceType.Queen => GetQueenMoves(),
            pieceType.King => GetKingMoves(),
            _ => new Vector3[0],
        };

         movesCached = true;
        return cachedValidMoves;
    }
// This method is used for check detection. It computes the valid moves regardless of selection.
public Vector3[] GetValidMovesForCheck()
{
    return selectedPieceType switch
    {
        pieceType.Pawn => GetPawnMoves(),
        pieceType.Rook => GetRookMoves(),
        pieceType.Knight => GetKnightMoves(),
        pieceType.Bishop => GetBishopMoves(),
        pieceType.Queen => GetQueenMoves(),
        pieceType.King => GetKingMoves(),
        _ => new Vector3[0],
    };
}
    private Vector3[] GetRookMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);

        foreach (Vector3 direction in directions)
        {
            Vector3 move = currentPos + direction;
            while (ChessUtilities.IsWithinBounds(move))
            {
                PieceController piece = BoardManager.Instance.GetPieceAt(move);
                if (piece != null)
                {
                    if (piece.isWhite != isWhite)
                        moves.Add(move);
                    break;
                }
                moves.Add(move);
                move += direction;
            }
        }
        return moves.ToArray();
    }

    private Vector3[] GetPawnMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        int direction = isWhite ? 1 : -1;

        AddForwardMoves(moves, direction);
        AddDiagonalCaptures(moves, direction);
        AddEnPassantMoves(moves, direction);

        return moves.ToArray();
    }

    private void AddForwardMoves(List<Vector3> moves, int direction)
    {
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);
        Vector3 forwardMove = currentPos + new Vector3(0, direction, 0);

        if (!BoardManager.Instance.IsOccupied(forwardMove))
        {
            moves.Add(forwardMove);

            Vector3 doubleForwardMove = currentPos + new Vector3(0, direction * 2, 0);
            bool isAtStartingRank = (isWhite && currentPos.y == 1) || (!isWhite && currentPos.y == 6);
            if (isAtStartingRank && !BoardManager.Instance.IsOccupied(doubleForwardMove))
            {
                moves.Add(doubleForwardMove);
            }
        }
        /*else
        {

        }*/
    }

    private void AddDiagonalCaptures(List<Vector3> moves, int direction)
    {
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);
        Vector3 leftDiagonal = currentPos + new Vector3(-1, direction, 0);
        Vector3 rightDiagonal = currentPos + new Vector3(1, direction, 0);

        // Left diagonal capture.
        if (currentPos.x > 0 && BoardManager.Instance.IsOccupied(leftDiagonal))
        {
            PieceController leftPiece = BoardManager.Instance.GetPieceAt(leftDiagonal);
            if (leftPiece != null && leftPiece.isWhite != isWhite)
            {
                moves.Add(leftDiagonal);
            }
        }

        // Right diagonal capture.
        if (currentPos.x < 7 && BoardManager.Instance.IsOccupied(rightDiagonal))
        {
            PieceController rightPiece = BoardManager.Instance.GetPieceAt(rightDiagonal);
            if (rightPiece != null && rightPiece.isWhite != isWhite)
            {
                moves.Add(rightDiagonal);
            }
        }
    }

    private void AddEnPassantMoves(List<Vector3> moves, int direction)
    {
        if (GameManager.Instance.lastMovedPiece != null &&
            GameManager.Instance.lastMovedPiece.selectedPieceType == pieceType.Pawn)
        {
            // Assume lastMoveStartPos is already standardized.
            Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos;
            Vector3 lastMoveEnd = ChessUtilities.BoardPosition(GameManager.Instance.lastMovedPiece.transform.position);
            Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);

            // Check that the enemy pawn moved two squares, is on the same rank as this pawn,
            // and is horizontally adjacent.
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == currentPos.y &&
                Mathf.Abs(lastMoveEnd.x - currentPos.x) == 1)
            {
                Vector3 enPassantTarget = new Vector3(lastMoveEnd.x, currentPos.y + direction, currentPos.z);
               // Debug.Log($"En Passant Move Detected for {name} at {enPassantTarget}");
                if (!BoardManager.Instance.IsOccupied(enPassantTarget))
                {
                    moves.Add(enPassantTarget);
                }
            }
        }
    }

    private Vector3[] GetBishopMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        Vector3[] directions = {
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0)
        };
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);

        foreach (Vector3 direction in directions)
        {
            Vector3 move = currentPos + direction;
            while (ChessUtilities.IsWithinBounds(move))
            {
                PieceController piece = BoardManager.Instance.GetPieceAt(move);
                if (piece != null)
                {
                    if (piece.isWhite != isWhite)
                        moves.Add(move);
                    break;
                }
                moves.Add(move);
                move += direction;
            }
        }
        return moves.ToArray();
    }

    private Vector3[] GetKnightMoves()
    {
        Vector3[] knightMoves = {
            new Vector3(1, 2, 0), new Vector3(1, -2, 0),
            new Vector3(-1, 2, 0), new Vector3(-1, -2, 0),
            new Vector3(2, 1, 0), new Vector3(2, -1, 0),
            new Vector3(-2, 1, 0), new Vector3(-2, -1, 0)
        };
        List<Vector3> validMoves = new List<Vector3>();
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);

        foreach (Vector3 move in knightMoves)
        {
            Vector3 newPosition = currentPos + move;
            if (IsValidMove(newPosition))
                validMoves.Add(newPosition);
        }


        return validMoves.ToArray();
    }

    private Vector3[] GetQueenMoves() => GetRookMoves().Concat(GetBishopMoves()).ToArray();

    private Vector3[] GetKingMoves()
    {
        
        Vector3[] kingMoves = {
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(1, 1, 0), new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0), new Vector3(-1, -1, 0)
        };

        if (SpecialMoves.IsCastlingValid(this, BoardManager.Instance))
        {
           // Debug.Log("Getting Kind Castling moves since castling is valid");
            kingMoves = kingMoves.Concat(SpecialMoves.GetCastlingMoves(this, BoardManager.Instance)).ToArray();
        }

        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);
        return kingMoves.Select(move => currentPos + move)
                        .Where(IsValidMove)
                        .ToArray();
        }


        
}
