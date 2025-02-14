using UnityEngine;
using UnityEngine.UI;
using System;

public class GameOver : MonoBehaviour
{
    // (Optional) UI text to show the game over message.
    public Text gameOverText;

    void OnEnable()
    {
        GameManager.OnMoveCompleted += CheckGameOver;
    }

    void OnDisable()
    {
        GameManager.OnMoveCompleted -= CheckGameOver;
    }

    /// <summary>
    /// Called after each move to check for checkmate.
    /// </summary>
    private void CheckGameOver()
    {
        Debug.Log("Check Game Over");
        // It is now the turn of the player whose turn is indicated in GameManager.
        // Check if that player's king is checkmated.
        bool currentTurnIsWhite = GameManager.Instance.isWhiteTurn;
        PieceController king = CheckController.GetKingFor(currentTurnIsWhite);
        if (king != null)
        {
            Debug.Log("KING IS FOUND");
            CheckController cc = king.GetComponent<CheckController>();
            if (cc != null && cc.IsCheckMate())
            {
                // Activate the GameOver Panel.
                gameObject.SetActive(true);
                if (gameOverText != null)
                {
                    string winner = currentTurnIsWhite ? "Black" : "White";
                    gameOverText.text = winner + " wins by checkmate!";
                }
            }
        }
    }

    /// <summary>
    /// Checks for checkmate by querying the appropriate king's CheckController.
    /// Assumes that the kings are stored in BoardManager via their PieceController.
    /// </summary>
    public bool CheckForCheckmate()
{
    // activeColorIsWhite is the player who is about to move.
    bool activeColorIsWhite = GameManager.Instance.isWhiteTurn;
    PieceController king = CheckController.GetKingFor(activeColorIsWhite);
    if (king != null)
    {
        CheckController cc = king.GetComponent<CheckController>();
        if (cc != null)
        {
            Debug.Log("Check CheckMate " + cc.IsCheckMate());
            return cc.IsCheckMate();
        }
    }
    return false;
}

    /// <summary>
    /// Triggers game over by activating the GameOver panel and displaying the message.
    /// </summary>
    public void TriggerGameOver(string message)
    {
        // Activate this panel (which is assumed to be inactive until game over).
        gameObject.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.text = message;
        }
    }
}