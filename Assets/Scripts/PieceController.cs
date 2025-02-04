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
    public static PieceController currentlySelectedPiece = null;
    public List<GameObject> validMoveIndicators = new List<GameObject>();
    public bool isSelected = false;

    private void OnMouseDown()
    {
        if (currentlySelectedPiece != null)
            currentlySelectedPiece.Deselect();

        isSelected = !isSelected;
        currentlySelectedPiece = isSelected ? this : null;

        if (isSelected)
        {
            Instantiate(highlightSelectedPiece, transform.position, Quaternion.identity);
            HighlightValidMoves();
        }
        else
        {
            Deselect();
        }
    }

    public void Deselect()
    {
        foreach (GameObject indicator in validMoveIndicators)
            Destroy(indicator);
        validMoveIndicators.Clear();
    }

    private void HighlightValidMoves()
    {
        Vector3[] validMoves = GetValidMoves();
        validMoveIndicators.Clear();
        foreach (Vector3 move in validMoves)
        {
            if (IsValidMove(move))
            {
                GameObject validMove = Instantiate(highlightValidMoves, move, Quaternion.identity);
                validMove.tag = "ValidMove";
                validMoveIndicators.Add(validMove);
            }
        }
    }

    public bool IsValidMove(Vector3 move)
    {
        if (!IsWithinBounds(move)) return false;
        if (PiecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
            return piece.isWhite != this.isWhite;
        return true;
    }

    public bool IsWithinBounds(Vector3 position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }

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
        Vector3[] directions = { 
            Vector3.up, 
            Vector3.down, 
            Vector3.left, 
            Vector3.right 
            };

        foreach (Vector3 direction in directions)
        {
            Vector3 move = transform.position + direction;
            while (IsWithinBounds(move))
            {
                if (PiecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
                {
                    if (piece.isWhite != isWhite) moves.Add(move);
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
        Vector3 forwardMove = transform.position + new Vector3(0, direction, 0);

        if (!PiecePlacement.occupiedPositions.ContainsKey(forwardMove))
        {
            moves.Add(forwardMove);
            Vector3 doubleForwardMove = transform.position + new Vector3(0, direction * 2, 0);
            if ((transform.position.y == (isWhite ? 1 : 6)) && !PiecePlacement.occupiedPositions.ContainsKey(doubleForwardMove))
                moves.Add(doubleForwardMove);
        }
        
        Vector3 leftDiagonal = transform.position + new Vector3(-1, direction, 0);
        Vector3 rightDiagonal = transform.position + new Vector3(1, direction, 0);
        if (PiecePlacement.occupiedPositions.TryGetValue(leftDiagonal, out PieceController leftPiece) && leftPiece.isWhite != isWhite)
            moves.Add(leftDiagonal);
        if (PiecePlacement.occupiedPositions.TryGetValue(rightDiagonal, out PieceController rightPiece) && rightPiece.isWhite != isWhite)
            moves.Add(rightDiagonal);
        
        return moves.ToArray();
    }

    private Vector3[] GetBishopMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        Vector3[] directions = { new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };

        foreach (Vector3 direction in directions)
        {
            Vector3 move = transform.position + direction;
            while (IsWithinBounds(move))
            {
                if (PiecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
                {
                    if (piece.isWhite != isWhite) moves.Add(move);
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
        Vector3[] knightMoves = { new Vector3(1, 2, 0), new Vector3(1, -2, 0), new Vector3(-1, 2, 0), new Vector3(-1, -2, 0),
                                  new Vector3(2, 1, 0), new Vector3(2, -1, 0), new Vector3(-2, 1, 0), new Vector3(-2, -1, 0) };
        List<Vector3> validMoves = new List<Vector3>();
        
        foreach (Vector3 move in knightMoves)
        {
            Vector3 newPosition = transform.position + move;
            if (IsValidMove(newPosition)) validMoves.Add(newPosition);
        }
        return validMoves.ToArray();
    }

    private Vector3[] GetQueenMoves() => GetRookMoves().Concat(GetBishopMoves()).ToArray();

    private Vector3[] GetKingMoves()
    {
        Vector3[] kingMoves = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0),
                                 new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };
        return kingMoves.Select(move => transform.position + move).Where(IsValidMove).ToArray();
    }
}
