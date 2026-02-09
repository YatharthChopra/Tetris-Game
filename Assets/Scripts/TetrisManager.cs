using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    public int score { get; private set; }
    public bool gameOver { get; private set; }

    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;

    private void Start()
    {
        SetGameOver(false);
    }

    public int CalculateScore(int linesCleared)
    {
        switch (linesCleared)
        {
            case 0: return 0;
            case 1: return 100;
            case 2: return 300;
            case 3: return 500;
            case 4: return 800;
            default: return 0;
        }
    }

    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool _gameOver)
    {
        if (!_gameOver)
        {
            //when the game over event is FALSE, reset the score 
            score = 0;
            ChangeScore(0);
        }
        gameOver  = _gameOver;
        OnGameOver.Invoke();
    }
}