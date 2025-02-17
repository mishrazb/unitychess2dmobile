using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUi : MonoBehaviour
{
    // Start is called before the first frame update

    public void OnHumanVsAI(){
        SceneManager.LoadScene("HumanVrsAI");

    }
    public void OnHumanVsHuman(){
        SceneManager.LoadScene("HumanVrsHuman");
    }
    public void OnPuzzlePlay(){
     Debug.Log("Puzzles");
    }
     public void OnPlayOnline(){
        Debug.Log("Play online");
    }
    public void OnGameSettings(){
             Debug.Log("Game Settings");
    }


}
