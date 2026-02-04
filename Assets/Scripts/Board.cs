using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetronimoData[] tetronimos;
    public Piece activePiece;
    public Tilemap tilemap;
    public Vector2Int boardSize;
    public Vector2Int startPosition;

    private void Start()
    {
        SpawnPiece();
    }

    void SpawnPiece()
    {
        activePiece.Initialize(this, Tetronimo.T);
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
            tilemap.SetTile(cellPosition, piece.data.tile);
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
            if (cellPosition.x  < left || cellPosition.x >= right || cellPosition.y < bottom || cellPosition.y >= right) return false;

            // this will check if this position is occupied in the tilemap
            if (tilemap.HasTile(cellPosition)) return false;
        }
            return true;
    }
}