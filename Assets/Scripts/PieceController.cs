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
    private void OnMouseDown()
{
    // Ensure it's the correct player's turn
    if (!GameManager.Instance.IsCurrentPlayerTurn(isWhite))
    {
        Debug.Log("Not your turn!");
        return;
    }

    // Deselect the previously selected piece
    if (currentlySelectedPiece != null && currentlySelectedPiece != this)
    {
        currentlySelectedPiece.Deselect();
    }

    // Select the new piece if it's not already selected
    isSelected = !isSelected;
    currentlySelectedPiece = isSelected ? this : null;

    if (isSelected)
    {
        // Highlight only the selected piece
        if (goHighlight != null) Destroy(goHighlight);
        goHighlight = Instantiate(highlightSelectedPiece, transform.position, Quaternion.identity);
        goHighlight.transform.SetParent(transform);
        
        // Highlight valid moves
        HighlightValidMoves();
    }
    else
    {
        Deselect();
    }
}
    public void Deselect()
    {
        //foreach (GameObject indicator in validMoveIndicators){
         //   Destroy(indicator);
        //}
        GameObject[] outlines = GameObject.FindGameObjectsWithTag("ValidMove");
             foreach (GameObject outline in outlines){
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
            Debug.Log(move);
            if (IsValidMove(move))
            {
                GameObject validMove = Instantiate(highlightValidMoves, move, Quaternion.identity);
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
        if (!IsWithinBounds(move)) return false;
        if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
            return piece.isWhite != this.isWhite;
        return true;
    }

    public bool IsWithinBounds(Vector3 position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }
    public Vector3[] GetValidMoves( )
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
                if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
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

        if (!piecePlacement.occupiedPositions.ContainsKey(forwardMove))
        {
            moves.Add(forwardMove);
            Vector3 doubleForwardMove = transform.position + new Vector3(0, direction * 2, 0);
            if ((transform.position.y == (isWhite ? 1 : 6)) && !piecePlacement.occupiedPositions.ContainsKey(doubleForwardMove))
                moves.Add(doubleForwardMove);
        }
        
        Vector3 leftDiagonal = transform.position + new Vector3(-1, direction, 0);
        Vector3 rightDiagonal = transform.position + new Vector3(1, direction, 0);
        if (piecePlacement.occupiedPositions.TryGetValue(leftDiagonal, out PieceController leftPiece) && leftPiece.isWhite != isWhite)
            moves.Add(leftDiagonal);
        if (piecePlacement.occupiedPositions.TryGetValue(rightDiagonal, out PieceController rightPiece) && rightPiece.isWhite != isWhite)
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
                if (piecePlacement.occupiedPositions.TryGetValue(move, out PieceController piece))
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
