using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChessController : MonoBehaviour
{
    public GameObject tilePrefab;
    public Color blackTileColor;
    public Color whiteTileColor;
    private GameObject[,] boardSquares = new GameObject[8, 8];  // 8x8 grid
    
    void Start()
    {
        //piecePlacement = GetComponent<PiecePlacement>();
        CreateBoard();
        //piecePlacement.PlacePieces();  // Delegate piece placement to PiecePlacement
    }

    void CreateBoard()
    {
        string tileColor;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square;
                if ((x + y) % 2 == 0)
                {
                    square = Instantiate(tilePrefab);
                    square.GetComponent<SpriteRenderer>().color=whiteTileColor;
                    tileColor = "white_tile_";
                }
                else
                {
                    square = Instantiate(tilePrefab);
                    square.GetComponent<SpriteRenderer>().color=blackTileColor;
                     tileColor = "black_tile_";
                }
                square.gameObject.name=tileColor+x+"_"+y;
                square.transform.position = new Vector3(x, y, 0);
                square.transform.SetParent(this.transform);
                boardSquares[x, y] = square;
            }
        }
    }
    public GameObject GetTileAtPosition(int x, int y){
       return boardSquares[x, y];
    }

   
}