using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : StaticInstance<GameManager>
{
    enum GameState {None, Started, Ended}


    [SerializeField] int score = 0;
    GameState gameState = GameState.None;

    public TextMeshProUGUI scoreUI;
    
    public GameObject gameOverDisplay;

    // Start is called before the first frame update
    void Start()
    {
        scoreUI.text = $"Score: 0";
    }

    // Update is called once per frame
    void Update()
    {
        scoreUI.text = $"Score: {score}";
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void Reset() 
    {
        SceneManager.LoadScene(0);
    }

    public void LoseGame()
    {
        gameOverDisplay.SetActive(true);
    }
}
