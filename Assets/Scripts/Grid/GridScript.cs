using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public GameObject tilePrefab;

    void Start()
    {
        GenerateGrid();
    }

    //Genera la grid en la posicion indicada
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x, 0, z);

                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

                tile.name = "Tile (" + x + "," + z + ")";

                tile.transform.parent = transform;
            }
        }
    }
}