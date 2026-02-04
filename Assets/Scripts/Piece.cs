using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    bool freeze = false;

    public void Initialize(Board board, Tetronimo tetronimo)
    {
        // set a reference to the board object
        this.board = board;

        // search for the tetronimo data and assign
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // copy of the tetronimo cell location
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++) cells[i] = data.cells[i];

        // set the start position of the piece
        position = board.startPosition;
    }

    private void Update()
    {
        if (freeze) return;
        
        board.Clear(this);

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        //rotation
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Rotate(1); //clockwise
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Rotate(-1); //counter-clockwise
        }

            board.Set(this);
    }

    void Rotate(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90.0f * direction);
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cellPosition = cells[i];

            Vector3 cellsPositionV3 = new Vector3(cellPosition.x, cellPosition.y);

            Vector3 result = rotation * cellsPositionV3;

            cells[i] = new Vector2Int(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.y));
        }
    }

    void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            //Do nothing
        }

        freeze = true;
        board.SpawnPiece();
    }

    bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool isValid = board.IsPositionValid(this, newPosition);
        if (isValid) position = newPosition;

        return isValid;
    }
}