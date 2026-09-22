using UnityEngine;

public class Grid_Manager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;

    public float cellSize;

    private GridCell[,] cells;

    // Genera la grilla de celdas al iniciar el juego
    void Start()
    {
        cells = new GridCell[gridWidth, gridHeight];

        GenerateGrid();
    }

    // funcion que hace que se genera la grilla
    void GenerateGrid()
    {
        // calcula la posición inicial para centrar la grilla en el mundo
        float startX = -(gridWidth * cellSize) / 2f;
        float startZ = -(gridHeight * cellSize) / 2f;

        // crea las celdas de la grilla
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                GameObject cellObject = new GameObject(
                    "GridCell_" + x + "_" + z
                );

                cellObject.transform.SetParent(transform);

                float worldX =
                    startX + (x + 0.5f) * cellSize;

                float worldZ =
                    startZ + (z + 0.5f) * cellSize;

                cellObject.transform.position =
                    new Vector3(worldX, 0.01f, worldZ);

                GridCell cell =
                    cellObject.AddComponent<GridCell>();

                cell.GridPosition =
                    new Vector3Int(x, 0, z);

                cells[x, z] = cell;
            }
        }
    }

    // convierte una posición en el mundo a una posición en la grilla
    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        float startX = -(gridWidth * cellSize) / 2f;
        float startZ = -(gridHeight * cellSize) / 2f;

        int x = Mathf.FloorToInt(
            (worldPos.x - startX) / cellSize
        );

        int z = Mathf.FloorToInt(
            (worldPos.z - startZ) / cellSize
        );

        return new Vector3Int(x, 0, z);
    }

    // convierte una posición en la grilla a una posición en el mundo
    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        float startX = -(gridWidth * cellSize) / 2f;
        float startZ = -(gridHeight * cellSize) / 2f;

        return new Vector3(
            startX + (gridPos.x + 0.5f) * cellSize,
            0,
            startZ + (gridPos.z + 0.5f) * cellSize
        );
    }

    // verifica si una posición en la grilla está dentro de los límites de la grilla
    public bool IsWithinBounds(Vector3Int gridPos)
    {
        return gridPos.x >= 0 &&
               gridPos.x < gridWidth &&
               gridPos.z >= 0 &&
               gridPos.z < gridHeight;
    }

    // obtiene la celda correspondiente a una posición en la grilla
    public GridCell GetCell(Vector3Int gridPos)
    {
        if (!IsWithinBounds(gridPos))
            return null;

        return cells[gridPos.x, gridPos.z];
    }

    // activa o desactiva el modo de construcción en todas las celdas de la grilla
    public void SetBuildMode(bool active)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (cells[x, z] != null)
                    cells[x, z].SetBuildMode(active);
            }
        }
    }
}