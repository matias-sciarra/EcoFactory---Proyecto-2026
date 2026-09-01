using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;

    public int width = 20;
    public int height = 20;

    public float tileSize = 1f;

    void Start()
    {
        GenerateGrid();
    }

    //Genera la plataforma
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(
                    x * tileSize,
                    0,
                    z * tileSize
                );

                Instantiate(
                    tilePrefab,
                    pos,
                    Quaternion.identity,
                    transform
                );
            }
        }
    }
}
