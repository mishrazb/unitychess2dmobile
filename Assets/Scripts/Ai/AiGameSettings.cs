using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AiGameSettings : MonoBehaviour
{

     public GameObject GamePanel;
    public GameObject AiSettingsPanel;
    public Button PlayAsWhiteBtn;
     public Button PlayAsBlackBtn;
    // Start is called before the first frame update
    public int DefaultDifficulty =   1;
    public int PlayAsWhite = 1;

    public Text DifficultyText; 
   
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
       DefaultDifficulty = PlayerPrefs.GetInt("DifficultyLevel", 1);
       DifficultyText.text = PlayerPrefs.GetString("DifficultyLevelText", "Easy");
       
       int playAsWhite = PlayerPrefs.GetInt("PlayAsWhite",1);

       Image whiteImage = PlayAsWhiteBtn.GetComponent<Image>();
       Image blackImage = PlayAsWhiteBtn.GetComponent<Image>();

       Color c = new Color(25,173,0,0);
      


       if(playAsWhite==1){
            c.a = 1f;
             whiteImage.color = c;
          c.a = 0;
         blackImage.color  = c;

       }else{
         c.a = 1f;
         blackImage.color  = c;
           c.a = 0;
         whiteImage.color  = c;
       }

        PlayAsWhiteBtn.onClick.AddListener(() => SetPlayAs(1));
        PlayAsBlackBtn.onClick.AddListener(() => SetPlayAs(0));
       
    
    }
    public void PlayGame(){
        GamePanel.SetActive(true);
        gameObject.SetActive(false);
    }
    public void SetPlayAs(int playAsWhite){
        PlayAsWhite = playAsWhite;
        PlayerPrefs.SetInt("PlayAsWhite",PlayAsWhite);

    }
    
    public void SetDifficulty(bool add){
        if(add){
            if(DefaultDifficulty<10)
             DefaultDifficulty++;
        }else{
            if(DefaultDifficulty>1)
            DefaultDifficulty--;
        }
        switch(DefaultDifficulty){
            case 1:DifficultyText.text = "Extremely Easy"; break;
            case 2:DifficultyText.text = "Very Easy"; break;
            case 3:DifficultyText.text = "Easy"; break;
            case 4:DifficultyText.text = "Easy"; break;
            case 5:DifficultyText.text = "Normal"; break;
            case 6:DifficultyText.text = "Hard"; break;
            case 7:DifficultyText.text = "Very Hard"; break;
            case 8:DifficultyText.text = "Extremly Hard"; break;
            case 9:DifficultyText.text = "Master"; break;
            case 10:DifficultyText.text = "Chamption"; break;
        }
        PlayerPrefs.SetInt("DifficultyLevel", DefaultDifficulty);
        PlayerPrefs.SetString("DifficultyLevelText", DifficultyText.text.ToString());
    }


    public void GoToMainMenu(){
       // SceneMangment
       SceneManager.LoadScene("GameStart");
    }


}
