using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    public int score {  get; private set; }

    public UnityEvent OnScoreChanged;

    private void Start()
    {
        score = 0;
        ChangeScore(0);
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
}