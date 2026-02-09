using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    public bool freeze = false;

    public void Initialize(Board board, Tetronimo tetronimo)
    {
        // set a reference to the board object
        this.board = board;

        bool found = false;

        // search for the tetronimo data and assign
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogError($"No TetronimoData found for {tetronimo}. Check Board.tetronimos inspector entries.");
            return;
        }

        // copy of the tetronimo cell location
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++) cells[i] = data.cells[i];

        // set the start position of the piece
        position = board.startPosition;

        freeze = false;
    }

    private void Update()
    {
        if (board.tetrisManager.gameOver) return;
        if (freeze) return;

        board.Clear(this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }
            // TBD else if (Input.GetKeyDown(KeyCode.W))
            //{
            //    Move(Vector2Int.up);
            //}

            //rotation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1); //clockwise
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1); //counter-clockwise
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            board.CheckBoard(data.tetronimo);
        }


        board.Set(this);

        if (freeze)
        {
            board.CheckBoard(data.tetronimo);
            board.SpawnPiece();
        }
    }

    void Rotate(int direction)
    {
        Vector2Int[] temporarayCells = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++) temporarayCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks())
            {
                RevertRotation(temporarayCells);
            }
            else
            {
                Debug.Log("Wall kick succeeded");
            }
        }
        else
        {
            Debug.Log("Valid rotation");
        }
    }

    bool TryWallKicks()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>()
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
        };

        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);
        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    void RevertRotation(Vector2Int[] temporaryCells)
    {
        for (int i = 0; i < cells.Length; i++) cells[i] = temporaryCells[i];
    }

    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90.0f * direction);

        bool isSpeacial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cellsPosition = new Vector3(cells[i].x, cells[i].y);

            if (isSpeacial)
            {
                cellsPosition.x -= 0.5f;
                cellsPosition.y -= 0.5f;
            }

            Vector3 result = rotation * cellsPosition;

            if (isSpeacial)
            {
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }
        }
    }

    void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            //Do nothing
        }

        freeze = true;
    }

    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool isValid = board.IsPositionValid(this, newPosition);
        if (isValid) position = newPosition;

        return isValid;
    }
}