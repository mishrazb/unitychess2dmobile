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
  public void OnMouseDown()
{
  
    // 🔥 Otherwise, this is just a selection attempt
    //this is called 66 times potential performance hazard. Perhaps failt it directly on first call from game manager
    if (!GameManager.Instance.IsCurrentPlayerTurn(isWhite))
    {
        Debug.Log("Not your turn!");
        return;
    }

    // 🔥 Deselect previous piece if a different piece is selected
    if (currentlySelectedPiece != null && currentlySelectedPiece != this)
    {
        currentlySelectedPiece.Deselect();
    }

    // 🔥 Toggle selection
    isSelected = !isSelected;
    currentlySelectedPiece = isSelected ? this : null;

    if (isSelected)
    {
        if (goHighlight != null) Destroy(goHighlight);
        goHighlight = Instantiate(highlightSelectedPiece, transform.position, Quaternion.identity);
        goHighlight.transform.SetParent(transform);

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

    // 🔥 Standard forward move
    if (!piecePlacement.occupiedPositions.ContainsKey(forwardMove))
    {
        moves.Add(forwardMove);

        // 🔥 First double-step move
        Vector3 doubleForwardMove = transform.position + new Vector3(0, direction * 2, 0);
        if ((transform.position.y == (isWhite ? 1 : 6)) &&
            !piecePlacement.occupiedPositions.ContainsKey(doubleForwardMove))
        {
            moves.Add(doubleForwardMove);
        }
    }

    // 🔥 Normal diagonal captures
    Vector3 leftDiagonal = transform.position + new Vector3(-1, direction, 0);
    Vector3 rightDiagonal = transform.position + new Vector3(1, direction, 0);
    
    if (piecePlacement.occupiedPositions.TryGetValue(leftDiagonal, out PieceController leftPiece) &&
        leftPiece.isWhite != isWhite)
    {
        moves.Add(leftDiagonal);
    }
    
    if (piecePlacement.occupiedPositions.TryGetValue(rightDiagonal, out PieceController rightPiece) &&
        rightPiece.isWhite != isWhite)
    {
        moves.Add(rightDiagonal);
    }

   // 🔥 En Passant Check
    if (GameManager.Instance.lastMovedPiece != null && GameManager.Instance.lastMovedPiece.selectedPieceType == pieceType.Pawn)
    {
        Vector3 lastMoveStart = GameManager.Instance.lastMoveStartPos;
        Vector3 lastMoveEnd = GameManager.Instance.lastMovedPiece.transform.position;

        // 🔥 Ensure last move was a double-step move and on the same row
        if (Mathf.Abs(lastMoveEnd.y - lastMoveStart.y) == 2 && lastMoveEnd.y == transform.position.y)
        {
            // 🔥 Check for left-side en passant
            if (transform.position.x == lastMoveEnd.x + 1)
            {
                moves.Add(new Vector3(lastMoveEnd.x, transform.position.y + direction, 0)); // Move diagonally forward-left
            }

            // 🔥 Check for right-side en passant
            if (transform.position.x == lastMoveEnd.x - 1)
            {
                moves.Add(new Vector3(lastMoveEnd.x, transform.position.y + direction, 0)); // Move diagonally forward-right
            }
        }
    }

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
