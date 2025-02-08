using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
   void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PieceController clickedPiece = hit.collider.GetComponent<PieceController>();
            PieceMovement clickedTile = hit.collider.GetComponentInParent<PieceMovement>();

            if (clickedPiece != null)
            {
                Debug.Log("Clicked Piece");
                clickedPiece.OnPieceClicked(); // Let PieceController handle selection/capture
            }
            else if (clickedTile != null)
            {
                Vector3 target = hit.collider.transform.position;
                clickedTile.OnTargetTileClicked(target); // Let PieceMovement handle movement
            }
        }
    }
}
}
