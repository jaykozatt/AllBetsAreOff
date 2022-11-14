using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace AllBets
{
    public enum GameState {Initialized, Playing, Paused, Ended}
    public class GameManager : StaticInstance<GameManager>
    {
        [Serializable]
        public struct TimeSpan {
            public int minutes;
            public int seconds;
            public TimeSpan(int minutes, int seconds) {
                this.minutes = minutes; this.seconds = seconds;
            }
            public int totalSeconds {get=>60*minutes+seconds;}
        }

        [SerializeField] int score = 0;
        public TimeSpan survivalTime;
        private float timer;
        public GameState gameState = GameState.Initialized;
        public float timeUntilComboReset = 5;
        public float comboTimer {get; private set;}

        [SerializeField] int maxMultiplier = 6;
        private int comboMultiplier = 1;

        public TextMeshProUGUI scoreUI;
        public TextMeshProUGUI timerUI;

        public GameObject gameOverDisplay;
        public GameObject youWinDisplay;
        public GameObject pauseDisplay;
        public GameObject gameUI;

        public Action<int> OnComboUpdate;

        // Start is called before the first frame update
        void Start()
        {
            scoreUI.text = $"0";
            timerUI.text = "00:00";
            gameState = GameState.Initialized;
            timer = survivalTime.totalSeconds;
            comboTimer = 0;
        }

        // Update is called once per frame
        void Update()
        {
            scoreUI.text = $"{score}";
            timerUI.text = string.Format("{00}:{1:00}", (int)timer / 60, (int)timer % 60);
        
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Game State", (float) gameState);

            if (gameState == GameState.Playing)
            {
                timer = Mathf.Max(0, timer - Time.deltaTime);
                comboTimer = Mathf.Max(0, comboTimer - Time.deltaTime);
            }
            
            if (comboTimer <= 0) ResetCombo();
            if (timer <= 0) WinGame();
        }

        public void AddScore(int points, TextPopup popup)
        {
            score += points * comboMultiplier;
            popup.text = (points * comboMultiplier).ToString();
            comboTimer = timeUntilComboReset;
            OnComboUpdate?.Invoke(comboMultiplier);
        }

        public void IncreaseCombo()
        {
            comboMultiplier = Mathf.Clamp(comboMultiplier + 1, 1, maxMultiplier);
            comboTimer = timeUntilComboReset;
            OnComboUpdate?.Invoke(comboMultiplier);
        } 

        private void ResetCombo() => comboMultiplier = 1;

        public void Reset() 
        {
            SceneManager.LoadScene(0);
        }

        public void BeginGame()
        {
            // gameUI.SetActive(true);
            gameState = GameState.Playing;
        }

        public void EnableGUI() => gameUI.SetActive(true);

        public void PauseGame()
        {
            gameState = GameState.Paused;
            Time.timeScale = 0;
            pauseDisplay.SetActive(true);
            // gameUI.SetActive(false);
        }

        public void ResumeGame()
        {
            gameState = GameState.Playing;
            Time.timeScale = 1;
            pauseDisplay.SetActive(false);
            // gameUI.SetActive(true);
        }

        public void WinGame()
        {
            gameUI.SetActive(false);
            youWinDisplay.SetActive(true);
            youWinDisplay.GetComponentsInChildren<TextMeshProUGUI>()[0].text = 
                string.Format("{00}:{1:00}", (int)timer / 60, (int)timer % 60);
            youWinDisplay.GetComponentsInChildren<TextMeshProUGUI>()[2].text = $"Score: {score}";
            gameState = GameState.Ended;
        
            UpdateHighScore();
        }

        public void LoseGame()
        {
            gameUI.SetActive(false);
            gameOverDisplay.SetActive(true);
            gameOverDisplay.GetComponentsInChildren<TextMeshProUGUI>()[0].text = 
                string.Format("{00}:{1:00}", (int)timer / 60, (int)timer % 60);
            gameOverDisplay.GetComponentsInChildren<TextMeshProUGUI>()[2].text = $"Score: {score}";
            gameState = GameState.Ended;

            UpdateHighScore();
        }

        void UpdateHighScore()
        {
            int highscore = PlayerPrefs.GetInt("highscore", 0);
            if (highscore < score)
                PlayerPrefs.SetInt("highscore", score);
        }
    }
}
