using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private Tile currentTile;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();

            if (tile != null)
            {
                if (currentTile != tile)
                {
                    if (currentTile != null)
                    {
                        currentTile.UnHighlight();
                    }

                    currentTile = tile;
                    currentTile.Highlight();
                }
            }
        }
        else
        {
            if (currentTile != null)
            {
                currentTile.UnHighlight();
                currentTile = null;
            }
        }
    }
}
