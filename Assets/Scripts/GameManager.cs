using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState {Paused, Started, Ended}
public class GameManager : StaticInstance<GameManager>
{

    
    [SerializeField] int score = 0;
    public float survivalTime;
    private TimeSpan timespan;
    public GameState gameState = GameState.Started;

    public TextMeshProUGUI scoreUI;
    public TextMeshProUGUI timerUI;
    
    public GameObject gameOverDisplay;

    // Start is called before the first frame update
    void Start()
    {
        scoreUI.text = $"Score: 0";
        timerUI.text = "00:00";
    }

    // Update is called once per frame
    void Update()
    {
        scoreUI.text = $"Score: {score}";
        timerUI.text = string.Format("{00}:{1:00}", (int)survivalTime / 60, (int)survivalTime % 60);
    
        if (!MissionText.Instance.gameObject.activeInHierarchy && gameState != GameState.Ended)
            survivalTime = Mathf.Max(0, survivalTime - Time.deltaTime);
        
        if (survivalTime <= 0) EndGame();
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void Reset() 
    {
        SceneManager.LoadScene(0);
    }

    public void EndGame()
    {
        gameOverDisplay.SetActive(true);
        gameState = GameState.Ended;
    }
}
