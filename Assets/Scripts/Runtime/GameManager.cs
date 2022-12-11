using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        int score = 0;
        public TimeSpan survivalTime;
        private float timer;
        public GameState gameState {get; private set;}
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
        public GameObject controlsUI;

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
            if (gameState != GameState.Ended)
            {
                score += points * comboMultiplier;
                popup.text = (points * comboMultiplier).ToString();
                comboTimer = timeUntilComboReset;
                OnComboUpdate?.Invoke(comboMultiplier);
            }
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
            SceneLoader.Instance?.TryLoadScene(Scene.ClassicMode);
            if (gameState == GameState.Paused) ResumeGame();
        }

        public void ReturnToMenu() 
        {
            SceneLoader.Instance?.TryLoadScene(Scene.MainMenu);
            if (gameState == GameState.Paused) ResumeGame();
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
            controlsUI.SetActive(false);
        }

        public void ResumeGame()
        {
            gameState = GameState.Playing;
            Time.timeScale = 1;
            pauseDisplay.SetActive(false);
            // gameUI.SetActive(true);
            controlsUI.SetActive(true);
        }

        public void WinGame()
        {
            // gameUI.SetActive(false);
            controlsUI.SetActive(false);
            youWinDisplay.SetActive(true);

            int highscore;
            if (TryUpdateHighscore(out highscore))
                youWinDisplay.GetComponentsInChildren<Image>()[2].gameObject.SetActive(true);
            else
                youWinDisplay.GetComponentsInChildren<Image>()[2].gameObject.SetActive(false);

            youWinDisplay.GetComponentsInChildren<TextMeshProUGUI>()[1].text = 
                $"<size=80%>Highscore:</size> {highscore}";
            gameState = GameState.Ended;
        
        }

        public void LoseGame()
        {
            // gameUI.SetActive(false);
            controlsUI.SetActive(false);
            gameOverDisplay.SetActive(true);

            int highscore;
            if (TryUpdateHighscore(out highscore))
                gameOverDisplay.GetComponentsInChildren<Image>()[2].gameObject.SetActive(true);
            else
                gameOverDisplay.GetComponentsInChildren<Image>()[2].gameObject.SetActive(false);

            gameOverDisplay.GetComponentsInChildren<TextMeshProUGUI>()[1].text = 
                $"<size=80%>Highscore:</size> {highscore}";
            gameState = GameState.Ended;

        }

        bool TryUpdateHighscore(out int highscore)
        {
            // Get current highscore
            highscore = PlayerPrefs.GetInt("highscore", 0);
            
            // Compare with current score
            if (highscore < score) 
            {
                // Save the new highscore
                PlayerPrefs.SetInt("highscore", highscore);
                highscore = score;

                return true;
            }

            return false;

        }
    }
}
