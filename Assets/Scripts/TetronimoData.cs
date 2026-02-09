using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// F being the new custom piece for the games
public enum Tetronimo { I, O, T, J, L, S, Z, F }

[Serializable]

public struct TetronimoData
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;
}