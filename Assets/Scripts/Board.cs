using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece piecePrefab;
    public Tilemap tilemap;

    public TetronimoData[] tetronimos;

    public Vector2Int boardSize;        // should be 10x20
    public Vector2Int startPosition;

    [Header("Drop Settings")]
    public float dropInterval = 0.5f;
    private float dropTime = 0f;

    [Header("Preset Board")]
    public TileBase presetFillTile;     // drag a tile here (ex: Cyan)

    private Piece activePiece;

    int left => -boardSize.x / 2;
    int right => boardSize.x / 2;
    int bottom => -boardSize.y / 2;
    int top => boardSize.y / 2;

    [Header("Spawn Order (fixed)")]
    public bool useFixedSpawnOrder = true;
    public Tetronimo[] fixedSpawnOrder =
    {
    Tetronimo.F,
    Tetronimo.L,
    Tetronimo.T,
    Tetronimo.F,
    Tetronimo.T,
    Tetronimo.I
    };

    private int fixedSpawnIndex = 0;


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

                CheckBoard(activePiece.data.tetronimo);
                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        Tetronimo nextType;

        if (useFixedSpawnOrder && fixedSpawnOrder != null && fixedSpawnOrder.Length > 0)
        {
            nextType = fixedSpawnOrder[fixedSpawnIndex];
            fixedSpawnIndex = (fixedSpawnIndex + 1) % fixedSpawnOrder.Length; // loops
        }
        else
        {
            TetronimoData randomData = tetronimos[Random.Range(0, tetronimos.Length)];
            nextType = randomData.tetronimo;
        }

        activePiece.Initialize(this, nextType);


        // Spawn safety for tall/custom pieces
        if (!TryFitSpawn(activePiece))
        {
            tetrisManager.SetGameOver(true);
            return;
        }

        Set(activePiece);
    }

    private bool TryFitSpawn(Piece piece)
    {
        piece.position = startPosition;

        if (IsPositionValid(piece, piece.position))
            return true;

        Vector2Int test = piece.position;

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

    // Hook to TetrisManager.OnGameOver if you want reset button flow
    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    private void ResetBoard()
    {
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece p in foundPieces) Destroy(p.gameObject);

        activePiece = null;
        tilemap.ClearAllTiles();

        CreateSpecialBoardState();
        fixedSpawnIndex = 0;
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

            if (tilemap.HasTile(cellPos)) return false;
        }

        return true;
    }

    // ---------- Line Clear / Score ----------

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

        // Optional bonus for custom piece named "F"
        if (placedType.ToString() == "F" && cleared.Count > 0)
        {
            score += 200;
        }

        tetrisManager.ChangeScore(score);
    }

    private bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            if (!tilemap.HasTile(new Vector3Int(x, y, 0))) return false;
        }
        return true;
    }

    private void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), null);
        }
    }

    private void ShiftRowsDown(int clearedRow)
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

    // ---------- Preset Board ----------

    private void CreateSpecialBoardState()
    {
        tilemap.ClearAllTiles();

        TileBase fill = presetFillTile != null
            ? presetFillTile
            : (tetronimos != null && tetronimos.Length > 0 ? tetronimos[0].tile : null);

        if (fill == null)
        {
            Debug.LogWarning("No presetFillTile assigned (and no tetronimos[0].tile). Preset board can't draw.");
            return;
        }

        // YOUR EXACT 10x10 (top -> bottom)
        // × = filled, - = empty
        string[] patternTopToBottom = new string[]
        {
            "X----XXXXX",
            "XX--XXXXXX",
            "XXX--XXXXX",
            "XXXX-XXXXX",
            "XXXX-XXXXX",
            "XXXXX---XX",
            "XXXXXX-XXX",
            "----XX----",
            "XXXXXXX---",
            "XXXXXXXX--",
            "XXXXXXXXX-",
        };

        // Validate width strictly (prevents silent failure)
        for (int i = 0; i < patternTopToBottom.Length; i++)
        {
            if (patternTopToBottom[i].Length != boardSize.x)
            {
                Debug.LogError($"Pattern row {i} is {patternTopToBottom[i].Length} chars but board width is {boardSize.x}.");
                return;
            }
        }

        // Place this 10-row pattern at the bottom of the 20-high board
        int startY = bottom;
        int patternRows = patternTopToBottom.Length;

        // bottom-most pattern row is last string
        for (int row = 0; row < patternRows; row++)
        {
            string line = patternTopToBottom[patternRows - 1 - row];
            int y = startY + row;

            for (int col = 0; col < boardSize.x; col++)
            {
                if (line[col] == 'X')
                {
                    int x = left + col;
                    tilemap.SetTile(new Vector3Int(x, y, 0), fill);
                }
            }
        }
    }
}
