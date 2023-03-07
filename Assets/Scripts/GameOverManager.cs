using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    
    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public void SubmitScore()
    {
        var playerName = nameInput.text;
        AppManager.Instance.SaveScore((int) GameManager.Instance.score, playerName);
        AppManager.Instance.ToMainMenu();
    }
}
