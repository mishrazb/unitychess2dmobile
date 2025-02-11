using UnityEngine;

public class GameUI : MonoBehaviour
{

    // Start is called before the first frame update
    public void OnPauseGame(){
        Debug.Log("Pause Game");
    }
    public void OnResign(){
        Debug.Log("Resign");
    }
    public void OnUndoMove(){
        Debug.Log("Undo Move");
    }
    public void OnRestartGame(){
        Debug.Log("Restart Game");
    } 
}
