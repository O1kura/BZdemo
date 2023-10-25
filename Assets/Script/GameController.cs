using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public TextMeshProUGUI highscoreText;
    public TextMeshProUGUI scoreText;
    public Button resumeBtn;
    public ScoreTextSpawner scoreTextSpawner;
    public GameObject pauseMenuPanel;

    [HideInInspector]
    public bool isGameOver;
    [HideInInspector]
    public bool paused;

    private int score = 0;
    private int highScore;
    private string key = "highscore";
    public int CalculateScore(int lines,Block block)
    {
        return lines*(lines+1)*5 + block.structure.Length;
    }
    // for block v2 test
    public int CalculateScore(int lines, BlockV2 block)
    {
        return lines * (lines + 1) * 5 + block.structure.Length;
    }

    public void UpdateScore(int lines, Block block)
    {
        scoreTextSpawner.Spawn(CalculateScore(lines,block), block.transform.position);

        score += CalculateScore(lines, block);
        scoreText.SetText(score.ToString());
        if(score >= highScore)
        {
            highScore = score;
            highscoreText.SetText(highScore.ToString());
        }
    }

    // for block v2 test
    public void UpdateScore(int lines, BlockV2 block)
    {
        scoreTextSpawner.Spawn(CalculateScore(lines, block), block.transform.position);

        score += CalculateScore(lines, block);
        scoreText.SetText(score.ToString());
        if (score >= highScore)
        {
            highScore = score;
            highscoreText.SetText(highScore.ToString());
        }
    }

    /*public void PauseButton()
    {
        if (paused) UnpauseGame();
        else PauseGame();
    }*/

    public void PauseGame()
    {
        paused = true;
        Time.timeScale = 0.0f;
        pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
    }

    public void UnpauseGame()
    {
        paused = false;
        Time.timeScale = 1.0f;
        pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
    }

    public void SetGameOver()
    {
        isGameOver = true;
        PauseGame();
        resumeBtn.interactable = false;
        SaveHighscore();
    }

    public void NewGame()
    {
        SaveHighscore();
        isGameOver = false;
        UnpauseGame();
        LoadHighscore();
        score = 0;
        scoreText.SetText("0");
        resumeBtn.interactable = true;
        ScoreText st = scoreTextSpawner.GetComponentInChildren<ScoreText>();
        if (st != null) st.DestroyText();
    }
    private void LoadHighscore()
    {
        if(PlayerPrefs.HasKey(key))
        {
            highScore = PlayerPrefs.GetInt(key);
            highscoreText.SetText(highScore.ToString());
        }
        else { highScore = 0; }
    }

    private void SaveHighscore()
    {
        PlayerPrefs.SetInt(key, highScore);
        PlayerPrefs.Save();
    }
    private void Awake()
    {
        if(!instance)
        {
            instance = this;
        }
        pauseMenuPanel.SetActive(false);
        LoadHighscore();
        Screen.autorotateToPortrait = true;
    }
}
