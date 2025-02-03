using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public bool isWhite;  // Flag to determine if the piece is white or black
    public enum pieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    [Header("Selection and validation")]
    public GameObject highlightSelectedPiece;  // Currently selected piece indicator
    public GameObject highlightValidMoves;    // Visually indicates valid moves for a piece
    public static PieceController currentlySelectedPiece = null;

    public List<GameObject> validMoveIndicators = new List<GameObject>();  // To store valid move indicators
    public pieceType selectedPieceType;
    private Vector3 originalPosition; // To store original position for undo if needed
    public bool isSelected = false;

    void Start()
    {
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        // Deselect previously selected piece if any
        if (currentlySelectedPiece != null)
        {
            currentlySelectedPiece.Deselect();
        }

        // Toggle the selection of the current piece
        if (!isSelected)
        {
            isSelected = true;
            currentlySelectedPiece = this;  // Set this piece as the selected piece 
            Instantiate(highlightSelectedPiece, transform.position, Quaternion.identity);
            HighlightValidMoves();
        }
        else
        {
            // Deselect if the piece is already selected
            isSelected = false;
            currentlySelectedPiece = null;  // No piece selected now
            Deselect();  // Deselect this piece
        }
    }

    public void Deselect()
    {
        // Destroy any previously instantiated outline or valid move indicators
        foreach (GameObject indicator in validMoveIndicators)
        {
            Destroy(indicator);
        }
        validMoveIndicators.Clear();
        GameObject[] selectedPieceOutlines = GameObject.FindGameObjectsWithTag("Outline");
        foreach (GameObject selectedPieceOutline in selectedPieceOutlines)
        {
            Destroy(selectedPieceOutline);
        }
    }

    void HighlightValidMoves()
    {
        Vector3[] validMoves = GetValidMoves();
        validMoveIndicators.Clear();
        foreach (Vector3 move in validMoves)
        {
           if(IsValidMove(move)){
            GameObject validMove = Instantiate(highlightValidMoves, move, Quaternion.identity);
            validMove.tag = "ValidMove"; // Tag valid moves for easy identification
            validMoveIndicators.Add(validMove);

           }
        }
    }

#region moves validation
 public bool IsValidMove(Vector3 move)
{
    // Ensure the move is within bounds (0 <= x < 8 and 0 <= y < 8)
    if (!IsWithinBounds(move)) return false;

    GameObject pieceAtTarget = GetPieceAtPosition(move);
    if (pieceAtTarget != null)
    {
        PieceController pieceController = pieceAtTarget.GetComponent<PieceController>();
        if (pieceController != null && pieceController.isWhite == this.isWhite)
        {
            return false; // Can't move to a square occupied by the same color
        }
    }

    Vector3 moveDelta = move - transform.position;
    int moveX = Mathf.RoundToInt(moveDelta.x);
    int moveY = Mathf.RoundToInt(moveDelta.y);

    switch (selectedPieceType)
    {
        case pieceType.Rook:
            if (moveX == 0 || moveY == 0) return CheckPathForObstacles(move, new Vector3(Mathf.Sign(moveDelta.x), Mathf.Sign(moveDelta.y), 0));
            return false;

        case pieceType.Bishop:
            if (Mathf.Abs(moveX) == Mathf.Abs(moveY)) return CheckPathForObstacles(move, new Vector3(Mathf.Sign(moveDelta.x), Mathf.Sign(moveDelta.y), 0));
            return false;

        case pieceType.Queen:
            if (moveX == 0 || moveY == 0 || Mathf.Abs(moveX) == Mathf.Abs(moveY))
                return CheckPathForObstacles(move, new Vector3(Mathf.Sign(moveDelta.x), Mathf.Sign(moveDelta.y), 0));
            return false;

        case pieceType.Knight:
            return (Mathf.Abs(moveX) == 2 && Mathf.Abs(moveY) == 1) || (Mathf.Abs(moveX) == 1 && Mathf.Abs(moveY) == 2);

        case pieceType.Pawn:
            return ValidatePawnMove(move, pieceAtTarget);
    }

    return true;
}
private bool IsWithinBounds(Vector3 position)
{
    return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
}
private bool CheckPathForObstacles(Vector3 target, Vector3 direction)
{
    Vector3 currentPosition = transform.position + direction;

    while (Vector3.Distance(currentPosition, target) > 0.1f)
    {
        if (GetPieceAtPosition(currentPosition) != null) return false; // Obstacle found
        currentPosition += direction;
    }
    return true;
}

private bool ValidatePawnMove(Vector3 move, GameObject pieceAtTarget)
{
    int direction = isWhite ? 1 : -1; // White moves up, Black moves down
    Vector3 moveDelta = move - transform.position;
    int moveX = Mathf.RoundToInt(moveDelta.x);
    int moveY = Mathf.RoundToInt(moveDelta.y);

    // Standard forward move (1 step)
    if (moveX == 0 && moveY == direction && pieceAtTarget == null)
    {
        return true;
    }

    // First move can be two steps forward if no piece is in front
    if (moveX == 0 && moveY == 2 * direction && transform.position.y == (isWhite ? 1 : 6) && pieceAtTarget == null)
    {
        // Ensure there's no piece blocking the first step
        Vector3 intermediatePosition = transform.position + new Vector3(0, direction, 0);
        if (GetPieceAtPosition(intermediatePosition) == null)
        {
            return true;
        }
    }

    // Diagonal capture move
    if (Mathf.Abs(moveX) == 1 && moveY == direction && pieceAtTarget != null)
    {
        return true;
    }

    return false;
}

GameObject GetPieceAtPosition(Vector3 position)
    {
       // Debug.Log(position.ToString());
        // Return the piece at the given position, or null if there isn't one
        foreach (Transform piece in transform.parent)
        {
            if (piece.position == position)
            {
                 //  Debug.Log("returning " + piece.gameObject.name.ToString());
                return piece.gameObject;
            }
        }
        return null;
    }

#endregion

#region Get moves 
    Vector3[] GetValidMoves()
    {
        // Return an array of valid move positions for the piece
        // This should be unique to each piece type (Pawn, Rook, Knight, etc.)
        if (selectedPieceType == pieceType.Pawn)
            return GetPawnMoves();
        if (selectedPieceType == pieceType.Rook)
            return GetRookMoves();
        if (selectedPieceType == pieceType.Knight)
            return GetKnightMoves();
        if (selectedPieceType == pieceType.Bishop)
            return GetBishopMoves();
        if (selectedPieceType == pieceType.Queen)
            return GetQueenMoves();
        if (selectedPieceType == pieceType.King)
            return GetKingMoves();

        return new Vector3[0];  // Default to empty if no piece type matches
    }
Vector3[] GetRookMoves()
{
    List<Vector3> moves = new List<Vector3>();
    
    // Define movement directions (up, down, left, right)
    Vector3[] directions = { 
        new Vector3(0, 1, 0),  // Up
        new Vector3(0, -1, 0), // Down
        new Vector3(-1, 0, 0), // Left
        new Vector3(1, 0, 0)   // Right
    };

    foreach (Vector3 direction in directions)
    {
        Vector3 move = transform.position + direction;
        
        while (IsWithinBounds(move) && GetPieceAtPosition(move) == null)
        {
            moves.Add(move);
            move += direction; // Move further in the same direction
        }

        // If there's an opponent's piece, we can capture it
        if (IsWithinBounds(move) && GetPieceAtPosition(move) != null)
        {
            PieceController piece = GetPieceAtPosition(move).GetComponent<PieceController>();
            if (piece != null && piece.isWhite != this.isWhite)
            {
                moves.Add(move);
            }
        }
    }

    return moves.ToArray();
}
    Vector3[] GetPawnMoves()
{
    List<Vector3> moves = new List<Vector3>();
    int forward = isWhite ? 1 : -1;
    Vector3 forwardMove = transform.position + new Vector3(0, forward, 0);
    
    // Normal forward move
    if (GetPieceAtPosition(forwardMove) == null)
        moves.Add(forwardMove);
    
    // First double step move
    Vector3 doubleForwardMove = transform.position + new Vector3(0, forward * 2, 0);
    if (transform.position.y == (isWhite ? 1 : 6) && GetPieceAtPosition(forwardMove) == null && GetPieceAtPosition(doubleForwardMove) == null)
        moves.Add(doubleForwardMove);

    // Capture moves
    Vector3 leftDiagonal = transform.position + new Vector3(-1, forward, 0);
    Vector3 rightDiagonal = transform.position + new Vector3(1, forward, 0);
    GameObject leftPiece = GetPieceAtPosition(leftDiagonal);
    GameObject rightPiece = GetPieceAtPosition(rightDiagonal);
    
    if (leftPiece != null && leftPiece.GetComponent<PieceController>().isWhite != isWhite)
        moves.Add(leftDiagonal);
    if (rightPiece != null && rightPiece.GetComponent<PieceController>().isWhite != isWhite)
        moves.Add(rightDiagonal);

    return moves.ToArray();
}
    Vector3[] GetBishopMoves()
{
    List<Vector3> moves = new List<Vector3>();
    for (int i = 1; i < 8; i++)
    {
        Vector3[] directions = {
            new Vector3(i, i, 0), new Vector3(-i, i, 0),
            new Vector3(i, -i, 0), new Vector3(-i, -i, 0)
        };

        foreach (Vector3 dir in directions)
        {
            Vector3 move = transform.position + dir;
            if (IsValidMove(move)) moves.Add(move);
        }
    }
    return moves.ToArray();
}
    Vector3[] GetKnightMoves()
    {
        Vector3[] moves = new Vector3[8];
        if (IsValidMove(transform.position + new Vector3(1, 2, 0))){
            moves[0] = transform.position + new Vector3(1, 2, 0);
        }
         if (IsValidMove(transform.position + new Vector3(1, -2, 0))){
                 moves[1] = transform.position + new Vector3(1, -2, 0);
        }
        if (IsValidMove(transform.position + new Vector3(-1, 2, 0))){
            moves[2] = transform.position + new Vector3(-1, 2, 0);
        }
       if (IsValidMove( transform.position + new Vector3(-1, -2, 0) ) ){
              moves[3] = transform.position + new Vector3(-1, -2, 0);
        }
      if (IsValidMove(  transform.position + new Vector3(2, 1, 0) ) ){
             moves[4] = transform.position + new Vector3(2, 1, 0);
        }
        if (IsValidMove(  transform.position + new Vector3(2, -1, 0) ) ){
             moves[5] = transform.position + new Vector3(2, -1, 0);
        }
       if (IsValidMove(  transform.position + new Vector3(-2, 1, 0 ) )){
             moves[6] = transform.position + new Vector3(-2, 1, 0);
        }
        if (IsValidMove(  transform.position + new Vector3(-2, -1, 0) )){
             moves[7] = transform.position + new Vector3(-2, -1, 0);
        }
       
        return moves;
    }
    Vector3[] GetQueenMoves()
    {
        Vector3[] rookMoves = GetRookMoves();
        Vector3[] bishopMoves = GetBishopMoves();

        Vector3[] queenMoves = new Vector3[rookMoves.Length + bishopMoves.Length];
        rookMoves.CopyTo(queenMoves, 0);
        bishopMoves.CopyTo(queenMoves, rookMoves.Length);

        return queenMoves;
    }
    Vector3[] GetKingMoves()
    {
        Vector3[] moves = new Vector3[8];
        if (IsValidMove(  transform.position + new Vector3(1, 0, 0) )){
        moves[0] = transform.position + new Vector3(1, 0, 0);
        }
         if (IsValidMove(  transform.position + new Vector3(-1, 0, 0) )){
        moves[1] = transform.position + new Vector3(-1, 0, 0);
         }
          if (IsValidMove(  transform.position + new Vector3(0, 1, 0) )){
        moves[2] = transform.position + new Vector3(0, 1, 0);
          }
           if (IsValidMove(  transform.position + new Vector3(0, -1, 0) )){
        moves[3] = transform.position + new Vector3(0, -1, 0);
           }
            if (IsValidMove(  transform.position + new Vector3(1, 1, 0) )){
        moves[4] = transform.position + new Vector3(1, 1, 0);
            }
             if (IsValidMove(  transform.position + new Vector3(-1, 1, 0) )){
        moves[5] = transform.position + new Vector3(-1, 1, 0);
             }
              if (IsValidMove(  transform.position + new Vector3(1, -1, 0) )){

        moves[6] = transform.position + new Vector3(1, -1, 0);
 
              }
               if (IsValidMove(   transform.position + new Vector3(-1, -1, 0) )){
                    moves[7] = transform.position + new Vector3(-1, -1, 0);
               }
        return moves;
    }
#endregion

}