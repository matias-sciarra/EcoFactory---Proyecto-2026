using UnityEngine;

public class Grid_Manager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;

    [Header("Tiles")]
    public GameObject tilePrefab;

    private GridCell[,] cells;

    void Awake()
    {
        cells = new GridCell[gridWidth, gridHeight];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = GridToWorld(new Vector3Int(x, 0, z));
                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                GridCell cell = tileObj.GetComponent<GridCell>();

                if (cell == null)
                    cell = tileObj.AddComponent<GridCell>();

                cell.GridPosition = new Vector3Int(x, 0, z);
                cells[x, z] = cell;
            }
        }
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int z = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector3Int(x, 0, z);
    }

    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.z * cellSize);
    }

    public bool IsWithinBounds(Vector3Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.z >= 0 && gridPos.z < gridHeight;
    }

    public GridCell GetCell(Vector3Int gridPos)
    {
        if (!IsWithinBounds(gridPos)) return null;
        return cells[gridPos.x, gridPos.z];
    }
}