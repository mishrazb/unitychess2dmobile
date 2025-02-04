using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isWhiteTurn = true; // Track whose turn it is (true = White, false = Black)
    public UnityEngine.UI.Text turnText; // UI Text element to show the current player's turn


    public ChessController chessController;
    public PiecePlacement pp;
   public UnityEngine.UI.Button resetBtn;


    // Update is called once per frame
    void Start(){
        UpdateTurnUI();
        resetBtn.onClick.AddListener(ResetPositions);
    }
    public void  ResetPositions(){
        Debug.Log("Reset board positions");
        pp.PlacePieces();
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
  

    public bool IsCurrentPlayerTurn(bool isWhite)
    {
        return isWhite == isWhiteTurn;
    }
    public void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        UpdateTurnUI();
    }
    private void UpdateTurnUI()
    {
        turnText.text = isWhiteTurn ? "White's Turn" : "Black's Turn";
    }
}


