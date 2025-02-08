using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public bool isWhite;
    public enum pieceType { Pawn, Rook, Knight, Bishop, Queen, King }
    public pieceType selectedPieceType;

    [Header("Selection and Validation")]
    public GameObject highlightSelectedPiece;
    public GameObject highlightValidMoves;
    private GameObject goHighlight;
    public static PieceController currentlySelectedPiece = null;
    public List<GameObject> validMoveIndicators = new List<GameObject>();
    public bool isSelected = false;
    public PiecePlacement piecePlacement;

    private void Start()
    {
        piecePlacement = FindObjectOfType<PiecePlacement>();
        if (piecePlacement == null)
        {
            Debug.LogError("PiecePlacement not found! Ensure there is a PiecePlacement object in the scene.");
        }
    }

    public void OnPieceClicked()
    {
        // Ensure it is the correct turn
        if (!GameManager.Instance.IsCurrentPlayerTurn(isWhite))
        {
            Debug.Log("Not your turn!");
            return;
        }

        // Deselect previously selected piece if different from current one
        if (currentlySelectedPiece != null && currentlySelectedPiece != this)
        {
            currentlySelectedPiece.Deselect();
        }

        // Toggle selection
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

    public void Deselect()
    {
        // Remove all valid move indicators by tag
        GameObject[] outlines = GameObject.FindGameObjectsWithTag("ValidMove");
        foreach (GameObject outline in outlines)
        {
            Destroy(outline);
        }
        validMoveIndicators.Clear();
    }

    private void HighlightValidMoves()
    {
        Vector3[] validMoves = GetValidMoves();
        validMoveIndicators.Clear();
        foreach (Vector3 move in validMoves)
        {
            // Ensure that the z coordinate matches our board standard
            Vector3 mmove = move;
            mmove.z = ChessUtilities.BoardPosition(transform.position).z;

            if (IsValidMove(mmove))
            {
                GameObject validMove = Instantiate(highlightValidMoves, mmove, Quaternion.identity);
                validMove.transform.position = mmove;
                validMove.transform.SetParent(transform);
                validMove.tag = "ValidMove";
                validMoveIndicators.Add(validMove);
            }
        }
    }

    public bool IsValidMove(Vector3 move)
    {
        return IsValidMove(move, piecePlacement);
    }

    public bool IsValidMove(Vector3 move, PiecePlacement piecePlacement)
    {
        if (!ChessUtilities.IsWithinBounds(move))
            return false;
        if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
            return piece.isWhite != this.isWhite;
        return true;
    }
/*
    public bool IsWithinBounds(Vector3 position)
    {
        return position.x >= 0 && position.x < 8 &&
               position.y >= 0 && position.y < 8;
    }
*/
    public Vector3[] GetValidMoves()
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
                if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
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

        if (!piecePlacement.occupiedPositions.ContainsKey(forwardMove))
        {
            Debug.Log($"{name}: Adding Single Forward Move {forwardMove}");
            moves.Add(forwardMove);

            Vector3 doubleForwardMove = currentPos + new Vector3(0, direction * 2, 0);
            bool isAtStartingRank = (isWhite && currentPos.y == 1) || (!isWhite && currentPos.y == 6);
            if (isAtStartingRank && !piecePlacement.occupiedPositions.ContainsKey(doubleForwardMove))
            {
                Debug.Log($"{name}: Adding Double Forward Move {doubleForwardMove}");
                moves.Add(doubleForwardMove);
            }
        }
        else
        {
            Debug.Log($"{name}: Forward Move Blocked at {forwardMove}");
        }
    }

    private void AddDiagonalCaptures(List<Vector3> moves, int direction)
    {
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);
        Vector3 leftDiagonal = currentPos + new Vector3(-1, direction, 0);
        Vector3 rightDiagonal = currentPos + new Vector3(1, direction, 0);

        // Left diagonal capture
        if (currentPos.x > 0 && piecePlacement.IsPositionOccupied(leftDiagonal))
        {
            PieceController leftPiece = piecePlacement.GetPieceAtPosition(leftDiagonal);
            if (leftPiece != null && leftPiece.isWhite != isWhite)
            {
                moves.Add(leftDiagonal);
            }
        }

        // Right diagonal capture
        if (currentPos.x < 7 && piecePlacement.IsPositionOccupied(rightDiagonal))
        {
            PieceController rightPiece = piecePlacement.GetPieceAtPosition(rightDiagonal);
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
            Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos;
            Vector3 lastMoveEnd = ChessUtilities.BoardPosition(GameManager.Instance.lastMovedPiece.transform.position);
            Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);

            // Ensure the enemy pawn moved two squares and is adjacent horizontally
            if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 &&
                lastMoveEnd.y == currentPos.y &&
                Mathf.Abs(lastMoveEnd.x - currentPos.x) == 1)
            {
                Vector3 enPassantTarget = new Vector3(lastMoveEnd.x, currentPos.y + direction, currentPos.z);
                Debug.Log($"En Passant Move Detected for {name} at {enPassantTarget}");
                if (!piecePlacement.occupiedPositions.ContainsKey(enPassantTarget))
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
                if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
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
        Vector3 currentPos = ChessUtilities.BoardPosition(transform.position);
        return kingMoves.Select(move => currentPos + move)
                        .Where(IsValidMove)
                        .ToArray();
    }

 
}