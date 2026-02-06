using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public TextMeshProUGUI scoreText;

    public void UIUpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score}";
    }
}
