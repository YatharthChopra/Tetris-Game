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

    public float dropInterval = 0.5f;
    float dropTime = 0.0f;

    Piece activePiece;

    int left => -boardSize.x / 2;
    int right => boardSize.x / 2;
    int bottom => -boardSize.y / 2;
    int top => boardSize.y / 2;

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;
        if (activePiece == null) return;

        dropTime += Time.deltaTime;
        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            // If the move fails, the piece has locked
            if (!moveResult)
            {
                activePiece.freeze = true;

                // ? Check lines + apply custom-piece bonus (uses the piece type that just locked)
                CheckBoard(activePiece.data.tetronimo);

                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        // Spawns random TetronimoData from your inspector list (includes your custom piece if you add it)
        TetronimoData randomData = tetronimos[Random.Range(0, tetronimos.Length)];

        activePiece.Initialize(this, randomData.tetronimo);

        CheckEndGame();

        Set(activePiece);
    }

    void CheckEndGame()
    {
        if (activePiece == null) return;

        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        // gameOver == false means we started a new game
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        // clear the spawned Piece objects
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;
        tilemap.ClearAllTiles();

        SpawnPiece();
    }

    // Set colors the tiles for the piece
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
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // bounds check
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top)
            {
                return false;
            }

            // occupied check
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

    void DestroyLine(int y)
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

    // ? Updated: accepts the placed piece type so we can award custom-piece bonuses
    public void CheckBoard(Tetronimo placedType)
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        int score = tetrisManager.CalculateScore(destroyedLines.Count);

        // Custom-piece bonus: if  custom piece clears 2+ lines, award a bonus
        // Change Tetronimo.HEX to whatever you named your custom piece in the enum.
        if (placedType == Tetronimo.F && destroyedLines.Count >= 2)
        {
            score += 200;
        }

        tetrisManager.ChangeScore(score);
    }
}
