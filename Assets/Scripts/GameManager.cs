using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState {Started, Paused, Ended}
public class GameManager : StaticInstance<GameManager>
{

    
    [SerializeField] int score = 0;
    public float survivalTime;
    private TimeSpan timespan;
    public GameState gameState = GameState.Started;

    public TextMeshProUGUI scoreUI;
    public TextMeshProUGUI timerUI;
    
    public GameObject gameOverDisplay;
    public GameObject youWinDisplay;
    public GameObject gameUI;

    // Start is called before the first frame update
    void Start()
    {
        scoreUI.text = $"0";
        timerUI.text = "00:00";
    }

    // Update is called once per frame
    void Update()
    {
        scoreUI.text = $"{score}";
        timerUI.text = string.Format("{00}:{1:00}", (int)survivalTime / 60, (int)survivalTime % 60);
    
        if (gameState < GameState.Paused)
            survivalTime = Mathf.Max(0, survivalTime - Time.deltaTime);
        
        if (survivalTime <= 0) WinGame();
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void Reset() 
    {
        SceneManager.LoadScene(0);
    }

    public void BeginGame()
    {
        gameUI.SetActive(true);
        gameState = GameState.Started;
    }

    public void WinGame()
    {
        gameUI.SetActive(false);
        youWinDisplay.SetActive(true);
        youWinDisplay.GetComponentsInChildren<TextMeshProUGUI>()[2].text = $"Score: {score}";
        youWinDisplay.GetComponentsInChildren<TextMeshProUGUI>()[3].text = 
            string.Format("{00}:{1:00}", (int)survivalTime / 60, (int)survivalTime % 60);
        gameState = GameState.Ended;
    }

    public void LoseGame()
    {
        gameUI.SetActive(false);
        gameOverDisplay.SetActive(true);
        gameOverDisplay.GetComponentsInChildren<TextMeshProUGUI>()[2].text = $"Score: {score}";
        gameOverDisplay.GetComponentsInChildren<TextMeshProUGUI>()[3].text = 
            string.Format("{00}:{1:00}", (int)survivalTime / 60, (int)survivalTime % 60);
        gameState = GameState.Ended;
    }
}
