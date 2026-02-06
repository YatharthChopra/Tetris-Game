using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece piecePrefab;
    public Tilemap tilemap;

    public TetronimoData[] tetronimos;

    public Vector2Int boardSize;
    public Vector2Int startPosition;

    Piece activePiece;

    int left
    {
        get
        {
            return -boardSize.x / 2;
        }
    }

    int right
    {
        get
        {
            return boardSize.x / 2;
        }
    }

    int bottom
    {
        get
        {
            return -boardSize.y / 2;
        }
    }

    int top
    {
        get
        {
            return boardSize.y / 2;
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        // Spawns random Tetronimo at start of every turn
        TetronimoData randomData = tetronimos[Random.Range(0, tetronimos.Length)];

        activePiece.Initialize(this, randomData.tetronimo);
        Set(activePiece);
    }

    // Set color's the tiles for the piece
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        int left = -boardSize.x / 2;
        int right = boardSize.x / 2;
        int bottom = -boardSize.y / 2;
        int top = boardSize.y / 2;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // bounce check
            if (cellPosition.x < left || cellPosition.x >= right || cellPosition.y < bottom || cellPosition.y >= top) return false;

            // this will check if this position is occupied in the tilemap
            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            if (!tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    void DestrotyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            tilemap.SetTile(cellPosition, null);
        }
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                TileBase currentTile = tilemap.GetTile(cellPosition);

                tilemap.SetTile(cellPosition, null);

                cellPosition.y -= 1;
                tilemap.SetTile(cellPosition, currentTile);
            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestrotyLine(y);
                destroyedLines.Add(y);
            }
        }

        //Debug.Log($"Lines Destroyed: {destroyedLines.Count}");

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        int score = tetrisManager.CalculateScore(destroyedLines.Count);

        tetrisManager.ChangeScore(score);
    }
}