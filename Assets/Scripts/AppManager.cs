using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [SerializeField] private GameObject _leaderboardContainer;
    [SerializeField] private GameObject _leaderboardItem;
    
    [HideInInspector] public bool interactable = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);

        Time.timeScale = 0;
        
        UpdateLeaderboard();
    }

    public void BeginGame()
    {
        SceneManager.LoadScene(1);
        interactable = true;
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
        interactable = false;
        Time.timeScale = 0;
        Debug.Log("pause");
    }

    public void ResumeGame()
    {
        SceneManager.UnloadSceneAsync(2);
        interactable = true;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
        interactable = false;
        Time.timeScale = 0;
        Debug.Log("over");
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
        interactable = false;
        Time.timeScale = 0;
        Debug.Log("main");
        
        UpdateLeaderboard();
    }

    private void UpdateLeaderboard()
    {
        if (PlayerPrefs.HasKey(Constants.PLAYER_PREFS_SCORE_KEY) == false)
        {
            _leaderboardContainer.transform.GetChild(0).gameObject.SetActive(true);
        }
        
        var scores = GetScores().ToList();
        scores.Sort((s1, s2) =>
        {
            if (s1.Item2 < s2.Item2) return 1;
            if (s1.Item2 > s2.Item2) return -1;
            else return 0;
        });
        
        if (scores.Count == 0)
        {
            _leaderboardContainer.transform.GetChild(0).gameObject.SetActive(true);
        }

        for (int i = 0; i < 5 && i < scores.Count; i++)
        {
            var item = Instantiate(_leaderboardItem, _leaderboardContainer.transform);
            Debug.Log(item.transform.childCount);
            item.transform.GetChild(0).GetComponent<TMP_Text>().text = scores[i].Item1;
            item.transform.GetChild(1).GetComponent<TMP_Text>().text = (scores[i].Item2).ToString().PadLeft(6, '0');
        }
    }

    public void SaveScore(int score, string playerName)
    {
        var scoresString = PlayerPrefs.GetString(Constants.PLAYER_PREFS_SCORE_KEY);
        if (string.IsNullOrEmpty(scoresString) == false)
            scoresString += Constants.PLAYER_PREFS_SEPERATOR;
        scoresString += $"{playerName},{score}";
        PlayerPrefs.SetString(Constants.PLAYER_PREFS_SCORE_KEY, scoresString);
    }

    public (string, int)[] GetScores()
    {
        if (PlayerPrefs.HasKey(Constants.PLAYER_PREFS_SCORE_KEY) == false)
        {
            return Array.Empty<(string, int)>();
        }

        var scoresString = PlayerPrefs.GetString(Constants.PLAYER_PREFS_SCORE_KEY);
        var scores = scoresString.Split(Constants.PLAYER_PREFS_SEPERATOR);
        return scores.Select(s =>
        {
            int.TryParse(s.Split(',')[1], out var score);
            return (s.Split(',')[0], score);
        }).ToArray();
    }
}
