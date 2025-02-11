using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class MoveRecord
{
    public PieceController MovedPiece;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public PieceController CapturedPiece;  // null if no capture
    // Optional: any flags for castling, en passant, promotion, etc.
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isWhiteTurn = true;
    public PieceController lastMovedPiece; // 🔥 Track last moved piece
    public Vector3 lastMoveStartPos; // 🔥 Track its starting position
    public Text turnText;

public List<MoveRecord> moveHistory = new List<MoveRecord>();

public  void AddMove(PieceController lastMovedPiece,Vector3 lastMoveStartPos, Vector3 endPos, PieceController capturedPiece ){
    // After a move is executed:
    MoveRecord record = new MoveRecord()
    {
        MovedPiece = lastMovedPiece,
        StartPosition = lastMoveStartPos,
        EndPosition = endPos,
        CapturedPiece = capturedPiece,  // if any, otherwise null
        // Set special move flags if needed.
    };
    moveHistory.Add(record);
}
    private void Awake()
    {
         turnText.text = "White's Turn";
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void EndTurn()
    {   

        isWhiteTurn = !isWhiteTurn;

        if(isWhiteTurn){
            turnText.text = "White's Turn";
        }else{
            turnText.text = "Black's Turn";
        }
    }
    public bool IsCurrentPlayerTurn(bool isWhite)
    {

        return isWhite == isWhiteTurn;
    }

   

}


