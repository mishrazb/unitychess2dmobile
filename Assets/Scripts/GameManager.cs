using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isWhiteTurn = true;
    public PieceController lastMovedPiece; // 🔥 Track last moved piece
    public Vector3 lastMoveStartPos; // 🔥 Track its starting position
    public Text turnText;
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


