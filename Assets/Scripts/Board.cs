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
    private float dropTime = 0.0f;

    private Piece activePiece;

    int left => -boardSize.x / 2;
    int right => boardSize.x / 2;
    int bottom => -boardSize.y / 2;
    int top => boardSize.y / 2;

    private void Start()
    {
        CreateSpecialBoardState();
        SpawnPiece();
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;
        if (activePiece == null) return;

        dropTime += Time.deltaTime;
        if (dropTime >= dropInterval)
        {
            dropTime = 0f;

            Clear(activePiece);
            bool moved = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moved)
            {
                activePiece.freeze = true;

                // Score based on lines cleared (and optional bonus)
                CheckBoard(activePiece.data.tetronimo);

                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        TetronimoData randomData = tetronimos[Random.Range(0, tetronimos.Length)];
        activePiece.Initialize(this, randomData.tetronimo);

        // ✅ Spawn safety: push down until valid (prevents instant game over with tall custom pieces)
        if (!TryFitSpawn(activePiece))
        {
            tetrisManager.SetGameOver(true);
            return;
        }

        Set(activePiece);
    }

    bool TryFitSpawn(Piece piece)
    {
        piece.position = startPosition;

        if (IsPositionValid(piece, piece.position))
            return true;

        Vector2Int test = piece.position;

        // push down up to board height to try to find a legal spawn
        for (int i = 0; i < boardSize.y; i++)
        {
            test += Vector2Int.down;
            if (IsPositionValid(piece, test))
            {
                piece.position = test;
                return true;
            }
        }

        return false;
    }

    // Hook this to TetrisManager.OnGameOver in Inspector
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
        foreach (Piece p in foundPieces) Destroy(p.gameObject);

        activePiece = null;
        tilemap.ClearAllTiles();

        CreateSpecialBoardState();
        SpawnPiece();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int pos = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(pos, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int pos = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(pos, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPos = (Vector3Int)(piece.cells[i] + position);

            if (cellPos.x < left || cellPos.x >= right ||
                cellPos.y < bottom || cellPos.y >= top)
                return false;

            if (tilemap.HasTile(cellPos))
                return false;
        }
        return true;
    }

    // Overload for old calls that do CheckBoard() with no params
    public void CheckBoard()
    {
        Tetronimo placed = (activePiece != null) ? activePiece.data.tetronimo : Tetronimo.T;
        CheckBoard(placed);
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            if (!tilemap.HasTile(new Vector3Int(x, y, 0))) return false;
        }
        return true;
    }

    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), null);
        }
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);

                tilemap.SetTile(pos, null);
                pos.y -= 1;
                tilemap.SetTile(pos, tile);
            }
        }
    }

    public void CheckBoard(Tetronimo placedType)
    {
        List<int> cleared = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                cleared.Add(y);
            }
        }

        int shifted = 0;
        foreach (int y in cleared)
        {
            ShiftRowsDown(y - shifted);
            shifted++;
        }

        int score = tetrisManager.CalculateScore(cleared.Count);

        // Optional bonus if your enum includes F
        if (placedType.ToString() == "F" && cleared.Count > 0)
        {
            score += 200;
        }

        tetrisManager.ChangeScore(score);
    }

    void CreateSpecialBoardState()
    {
        tilemap.ClearAllTiles();

        // Use any tile to visually fill the board
        TileBase fillTile = (tetronimos != null && tetronimos.Length > 0) ? tetronimos[0].tile : null;
        if (fillTile == null) return;

        int width = boardSize.x;    // 10
        int height = boardSize.y;   // 20

        int ColToX(int col) => left + (col - 1);
        int RowToY(int row) => bottom + (row - 1);

        // 10 chars wide per row (10 columns). Bottom -> Top.
        string[] rowsBottomToTop = new string[]
        {
            "##########", // row 1 (bottom)
            "##########", // row 2
            "####..####", // row 3
            "####..####", // row 4
            "###...####", // row 5
            "###...####", // row 6
            "##....####", // row 7
            "##....####", // row 8
            "##...#####", // row 9
            "#....#####", // row 10
            "#...######", // row 11
            "....######", // row 12
            "...#######", // row 13
            "..########", // row 14
            ".#########", // row 15
            "##########", // row 16
            "..........", // row 17
            "..........", // row 18
            "..........", // row 19
            ".........."  // row 20 (top)
        };

        // Paint filled cells
        for (int row = 1; row <= height; row++)
        {
            string line = rowsBottomToTop[row - 1];
            for (int col = 1; col <= width; col++)
            {
                if (line[col - 1] == '#')
                {
                    tilemap.SetTile(new Vector3Int(ColToX(col), RowToY(row), 0), fillTile);
                }
            }
        }

        // Carve a 2-wide vertical well (similar to your example image)
        int wellColA = 5;
        int wellColB = 6;

        for (int r = 6; r <= 16; r++)
        {
            tilemap.SetTile(new Vector3Int(ColToX(wellColA), RowToY(r), 0), null);
            tilemap.SetTile(new Vector3Int(ColToX(wellColB), RowToY(r), 0), null);
        }

        // Carve a pocket for your custom F piece
        // Your F cells: (0,1) (1,1) (0,2) (1,2) (1,3) (1,0)
        int pocketBaseRow = 4; // adjust up/down
        int pocketBaseCol = 5; // adjust left/right

        List<Vector2Int> pocketCells = new List<Vector2Int>()
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
            new Vector2Int(1, 3),
            new Vector2Int(1, 0),
        };

        foreach (Vector2Int cell in pocketCells)
        {
            int col = pocketBaseCol + cell.x;
            int row = pocketBaseRow + cell.y;

            if (col >= 1 && col <= width && row >= 1 && row <= height)
            {
                tilemap.SetTile(new Vector3Int(ColToX(col), RowToY(row), 0), null);
            }
        }
    }
}
