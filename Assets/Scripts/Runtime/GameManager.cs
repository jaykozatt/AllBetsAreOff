using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AllBets
{
    public enum GameState {Initialized, Playing, Paused, Ended}
    public enum GameMode {Classic, Endless}

    public class GameManager : StaticInstance<GameManager>
    {
        #region Data Structures
            [Serializable]
            public struct TimeSpan {
                public int minutes;
                public int seconds;
                public TimeSpan(int minutes, int seconds) {
                    this.minutes = minutes; this.seconds = seconds;
                }
                public int totalSeconds {get=>60*minutes+seconds;}
            }
        #endregion

        #region Settings
        [Header("Settings")]
            public TimeSpan survivalTime;
            public float timeUntilComboReset = 5;
            [SerializeField] int maxMultiplier = 6;
            public float comboTimer {get; private set;}
        #endregion

        #region Variables & Switches
            public GameState gameState {get; private set;}
            public static GameMode gameMode {get; set;}
            int score = 0;
            private float timer;
            private int comboMultiplier = 1;
        #endregion

        #region References
        [Header("References")]
            public TextMeshProUGUI scoreText;
            public TextMeshProUGUI timerText;
            public GameObject timerUI;
            public GameObject gameOverDisplay;
            public GameObject youWinDisplay;
            public GameObject pauseDisplay;
            public GameObject gameUI;
            public GameObject controlsUI;
        #endregion

        #region Events & Delegates
            public Action<int> OnComboUpdate;
        #endregion

        #region Functions & Methods

            #region Monobehaviour Functions
                // Start is called before the first frame update
                void Start()
                {
                    scoreText.text = $"0";
                    timerText.text = "00:00";
                    gameState = GameState.Initialized;
                    timer = survivalTime.totalSeconds;
                    comboTimer = 0;

                    if (gameMode == GameMode.Endless)
                        timerUI.SetActive(false);
                }

                // Update is called once per frame
                void Update()
                {
                    scoreText.text = $"{score}";
                    if(gameMode == GameMode.Classic)
                        timerText.text = string.Format("{00}:{1:00}", (int)timer / 60, (int)timer % 60);
                
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Game State", (float) gameState);

                    if (gameState == GameState.Playing)
                    {
                        if (gameMode == GameMode.Classic) 
                            timer = Mathf.Max(0, timer - Time.deltaTime);
                        comboTimer = Mathf.Max(0, comboTimer - Time.deltaTime);
                    }
                    
                    if (comboTimer <= 0) ResetCombo();
                    if (gameMode == GameMode.Classic && timer <= 0) WinGame();
                }
            #endregion
            
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

            public void ResetGame() 
            {
                SceneLoader.Instance?.TryLoadScene(Scene.Game);
                if (gameState == GameState.Paused) ResumeGame();
            }

            public void ReturnToMenu() 
            {
                SceneLoader.Instance?.TryLoadScene(Scene.MainMenu);
                if (gameState == GameState.Paused) ResumeGame();
            }

            public void BeginGame()
            {
                gameState = GameState.Playing;
            }

            public void EnableGUI() => gameUI.SetActive(true);

            public void PauseGame()
            {
                gameState = GameState.Paused;
                Time.timeScale = 0;
                pauseDisplay.SetActive(true);
                controlsUI.SetActive(false);
            }

            public void ResumeGame()
            {
                gameState = GameState.Playing;
                Time.timeScale = 1;
                pauseDisplay.SetActive(false);
                controlsUI.SetActive(true);
            }

            public void WinGame()
            {
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
        #endregion
    }
}
