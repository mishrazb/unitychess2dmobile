using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessController : MonoBehaviour
{
    public GameObject whiteSquarePrefab;  // Reference to white square prefab
    public GameObject blackSquarePrefab;  // Reference to black square prefab
    public PiecePlacement piecePlacement; // Reference to PiecePlacement for handling piece placement
 public GameObject tilePrefab;
 public Color blackTileColor;
 public Color whiteTileColor;
    private GameObject[,] boardSquares = new GameObject[8, 8];  // 8x8 grid
    
    void Start()
    {
        CreateBoard();
        piecePlacement.PlacePieces();  // Delegate piece placement to PiecePlacement
    }

    void CreateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square;
                if ((x + y) % 2 == 0)
                {
                    square = Instantiate(tilePrefab);
                    square.GetComponent<SpriteRenderer>().color=whiteTileColor;
                }
                else
                {
                    square = Instantiate(tilePrefab);
                    square.GetComponent<SpriteRenderer>().color=blackTileColor;
                }
                square.transform.position = new Vector3(x, y, 0);
                square.transform.SetParent(this.transform);
                boardSquares[x, y] = square;
            }
        }
    }

    // You can add other game logic here such as moves, checks, etc.
}