using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid_Script : MonoBehaviour
{
    [Header("Grid")]
    public int width = 50;
    public int height = 50;

    [Header("Prefabs")]
    public GameObject tilePrefab;

    void Start()
    {
        GenerateGrid();
    }

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