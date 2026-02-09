using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public TextMeshProUGUI scoreText;
    public GameObject endGamePanel;

    public void UIUpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score}";
    }

    public void UpdateGameOver()
    {
        //when the game over event is broadcast the end game panel will show when the game is over. It will hide once the game resets.
        endGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        //setting the game over as false will fall reset the game
        tetrisManager.SetGameOver(false);
    }
}