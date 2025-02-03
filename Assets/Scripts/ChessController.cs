using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessController : MonoBehaviour
{
    public GameObject whiteSquarePrefab;  // Reference to white square prefab
    public GameObject blackSquarePrefab;  // Reference to black square prefab
    private GameObject[,] boardSquares = new GameObject[8, 8];  // 8x8 grid
    
    void Start()
    {
        CreateBoard();
       
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
                    square = Instantiate(whiteSquarePrefab);
                }
                else
                {
                    square = Instantiate(blackSquarePrefab);
                }
                square.transform.position = new Vector3(x, y, 0);
                square.transform.SetParent(this.transform);
                boardSquares[x, y] = square;
            }
        }
    }



}
