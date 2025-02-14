using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    // Start is called before the first frame update
    public void OnPauseGame(){
        Debug.Log("Pause Game");
    }
    public void OnResign(){
        Debug.Log("Resign");
    }
    public void OnUndoMove()
    {
        // Check with the GameManager if there are moves to undo.
        if (GameManager.Instance.HasMoveRecords() && GameManager.Instance.MaxUndoMoves>0)
        {
            bool success = GameManager.Instance.UndoLastMove();
            GameManager.Instance.MaxUndoMoves--;
            GameManager.Instance.UndoMoveBtnText.text = GameManager.Instance.MaxUndoMoves.ToString();
            if (success)
            {
                Debug.Log("Undo successful.");
            }
            else
            {
                Debug.LogWarning("Undo failed: unable to revert the last move.");
            }
        }
        else
        {
            Debug.Log("No moves to undo.");
        }
    }
    public void OnRestartGame(){
       SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    } 
    public void OnGameReview(){
       Debug.Log("game review board");
    } 
     public void PreviusMove(){
       Debug.Log("Previous move");
    } 
    public void NextMove(){
       Debug.Log("Next move");
    } 
}
