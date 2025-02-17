using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChessController : MonoBehaviour
{
    public GameObject tilePrefabWhite;
        public GameObject tilePrefabBlack;
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
        string tileName;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square;
                if ((x + y) % 2 == 0)
                {
                     square = Instantiate(tilePrefabBlack);
                  //  square.GetComponent<SpriteRenderer>().color=blackTileColor;
                     tileName = "black_tile_";
                }
                else
                {
                    square = Instantiate(tilePrefabWhite);
                   // square.GetComponent<SpriteRenderer>().color=whiteTileColor;
                    tileName = "white_tile_";
                }
                square.gameObject.name=tileName+x+"_"+y;
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