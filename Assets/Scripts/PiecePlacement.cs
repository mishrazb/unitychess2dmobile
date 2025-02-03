using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlacement : MonoBehaviour
{
    
    [Header("White")]
    public GameObject whitePawn;
    public GameObject whiteRook;
    public GameObject whiteKnight;
    
    public GameObject whiteBishop;
    
    public GameObject whiteQueen;
    
    public GameObject whiteKing;

 [Header("Black")]
public GameObject blackPawn;
public GameObject blackRook;
public GameObject blackKnight;
    
    public GameObject blackBishop;
    
    public GameObject blackQueen;
    
    public GameObject blackKing;
    // Start is called before the first frame update
 /*
 [Header("Piece Positions")]
    public struct PieceInfo 
{
    public string pieceName;
    public string color;
    public Vector3 position;
    public PieceController pieceController;
}
 
    List<PieceInfo> pieceInfo = new List<PieceInfo>();  // Stores all piece info
    */

    public List<Vector3> piecePositions = new List<Vector3>(); 
    
    void Start()
    {
         SetupPieces();
    }

   #region piece placement


// Add all other piece prefabs here...

void SetupPieces()
{
    int z = -1;
    // Set up pawns
    for (int x = 0; x < 8; x++)
    {
        Vector3 pp = new Vector3(x, 1, z);
       Instantiate(whitePawn, pp, Quaternion.identity).transform.SetParent(this.transform);
       //piecePositions.Add(pp);
        /* PieceInfo p = new PieceInfo();
        p.color="white";
        p.pieceName="Pawn";
        p.position = pp;
        pieceInfo.Add(p);
        */
        Instantiate(blackPawn, new Vector3(x, 6, z), Quaternion.identity).transform.SetParent(this.transform);
        //piecePositions.Add(new Vector3(x, 6, z));
    }

    // Set up rooks
    Instantiate(whiteRook, new Vector3(0, 0, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(whiteRook, new Vector3(7, 0, z), Quaternion.identity).transform.SetParent(this.transform);
    //setup knights
    Instantiate(whiteKnight, new Vector3(1, 0, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(whiteKnight, new Vector3(6, 0, z), Quaternion.identity).transform.SetParent(this.transform);
     //setup bishops
    Instantiate(whiteBishop, new Vector3(2, 0, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(whiteBishop, new Vector3(5, 0,z), Quaternion.identity).transform.SetParent(this.transform);
     //setup queen
    Instantiate(whiteQueen, new Vector3(3, 0, z), Quaternion.identity).transform.SetParent(this.transform);
     //setup king
    Instantiate(whiteKing, new Vector3(4, 0, z), Quaternion.identity).transform.SetParent(this.transform);

    
    Instantiate(blackRook, new Vector3(0, 7, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(blackRook, new Vector3(7, 7, z), Quaternion.identity).transform.SetParent(this.transform);

     //setup knights
    Instantiate(blackKnight, new Vector3(1, 7, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(blackKnight, new Vector3(6, 7, z), Quaternion.identity).transform.SetParent(this.transform);
     //setup bishops
    Instantiate(blackBishop, new Vector3(2, 7, z), Quaternion.identity).transform.SetParent(this.transform);
    Instantiate(blackBishop, new Vector3(5, 7,z), Quaternion.identity).transform.SetParent(this.transform);
     //setup queen
    Instantiate(blackQueen, new Vector3(3, 7, z), Quaternion.identity).transform.SetParent(this.transform);
     //setup king
    Instantiate(blackKing, new Vector3(4, 7, z), Quaternion.identity).transform.SetParent(this.transform);

}
#endregion

}
