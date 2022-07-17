using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : StaticInstance<GameManager>
{

    [SerializeField] int score = 0;

    public TextMeshProUGUI scoreUI;

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
}
