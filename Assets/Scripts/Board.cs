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

                // Check lines + apply custom-piece bonus (uses the piece type that just locked)
                CheckBoard(activePiece.data.tetronimo);

                SpawnPiece();
            }
        }
    }

    // computes bounds of the piece's cells (relative coords)
    int GetMaxCellY(Piece p)
    {
        int maxY = int.MinValue;
        for (int i = 0; i < p.cells.Length; i++)
            maxY = Mathf.Max(maxY, p.cells[i].y);
        return maxY;
    }

    int GetMinCellY(Piece p)
    {
        int minY = int.MaxValue;
        for (int i = 0; i < p.cells.Length; i++)
            minY = Mathf.Min(minY, p.cells[i].y);
        return minY;
    }

    int GetMinCellX(Piece p)
    {
        int minX = int.MaxValue;
        for (int i = 0; i < p.cells.Length; i++)
            minX = Mathf.Min(minX, p.cells[i].x);
        return minX;
    }

    int GetMaxCellX(Piece p)
    {
        int maxX = int.MinValue;
        for (int i = 0; i < p.cells.Length; i++)
            maxX = Mathf.Max(maxX, p.cells[i].x);
        return maxX;
    }

    // adjust spawn so the whole piece fits within the board bounds
    void FitSpawnInsideBoard(Piece p)
    {
        int minX = GetMinCellX(p);
        int maxX = GetMaxCellX(p);
        int minY = GetMinCellY(p);
        int maxY = GetMaxCellY(p);

        // Allowed ranges for the piece pivot position so all cells remain inside bounds
        int minAllowedX = left - minX;        
        int maxAllowedX = (right - 1) - maxX; 

        int minAllowedY = bottom - minY;      
        int maxAllowedY = (top - 1) - maxY;   

        int clampedX = Mathf.Clamp(p.position.x, minAllowedX, maxAllowedX);
        int clampedY = Mathf.Clamp(p.position.y, minAllowedY, maxAllowedY);

        p.position = new Vector2Int(clampedX, clampedY);
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        // Spawns random TetronimoData from inspector list (includes custom piece too)
        TetronimoData randomData = tetronimos[Random.Range(0, tetronimos.Length)];
        activePiece.Initialize(this, randomData.tetronimo);

        // auto-fit spawn location so tall/wide custom pieces don't instantly game over
        activePiece.position = startPosition;
        FitSpawnInsideBoard(activePiece);

        // If even after fitting there isn't a valid spot (board is actually blocked), game over.
        CheckEndGame();

        Set(activePiece);
    }

    void CheckEndGame()
    {
        if (activePiece == null) return;

        if (!IsPositionValid(activePiece, activePiece.position))
        {
            // If there is not a valid position for the newly placed piece, the game is over.
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;
        tilemap.ClearAllTiles();

        SpawnPiece();
    }

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
                cellPosition.y < bottom || cellPosition.y >= top) return false;

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

    // Updated: accepts the placed piece type so game can award custom-piece bonuses
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

        if (placedType == Tetronimo.F && destroyedLines.Count >= 2)
        {
            score += 200;
        }

        tetrisManager.ChangeScore(score);
    }
}
